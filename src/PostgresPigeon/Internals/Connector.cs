namespace PostgresPigeon.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Messages.Backend;
    using Messages.Frontend;

    internal class Connector : IDisposable
    {
        private readonly string connectionString;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        private readonly BackendMessageReader messageReader = new BackendMessageReader(Encoding.UTF8);

        private Socket currentSocket;
        private NetworkStream stream;
        private Encoding streamEncoding;

        private readonly SocketAsyncEventArgs eOut = new SocketAsyncEventArgs();
        private WriteBuffer writeBuffer;
        private SocketWrapper writeSocket;
        private TypeInformationLoader typeInformationLoader;

        public Connector(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task Open(CancellationToken cancellationToken = default(CancellationToken))
        {
            var csb = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };

            try
            {
                await semaphore.WaitAsync(cancellationToken);

                currentSocket = await SocketFactory.Create(csb, cancellationToken);

                stream = new NetworkStream(currentSocket, true);
                streamEncoding = Encoding.UTF8;

                writeBuffer = new WriteBuffer(streamEncoding);
                writeSocket = new SocketWrapper(currentSocket, eOut);

                await new StartupMessage(csb["Username"].ToString(), csb["Database"].ToString())
                    .Send(writeSocket, writeBuffer);

                var message = await messageReader.ReadMessage(stream, cancellationToken);

                var authMessage = message as BackendAuthMessage;

                if (authMessage == null)
                {
                    throw new InvalidOperationException($"Failed to authenticate, unexpected response from the server: {message}.");
                }

                await new PasswordMessage(streamEncoding.GetBytes(csb["Password"].ToString())).Send(writeSocket, writeBuffer);

                authMessage = await messageReader.ReadMessage(stream, cancellationToken) as BackendAuthMessage;

                if (authMessage?.Type != AuthType.Success)
                {
                    throw new InvalidOperationException($"Failed to authenticate, unexpected response after sending password. Received {authMessage}.");
                }

                var messages = new List<object>();

                message = await messageReader.ReadMessage(stream, cancellationToken);
                while (!(message is ReadyForQuery))
                {
                    messages.Add(message);
                    message = await messageReader.ReadMessage(stream, cancellationToken);
                }

                typeInformationLoader = new TypeInformationLoader(stream, messageReader, writeBuffer, writeSocket, streamEncoding);
                await typeInformationLoader.GetTypeNameForOid(16);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to open the connection due to an error: {ex}.");
                stream?.Dispose();
                currentSocket?.Dispose();
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task<string> Query(string query)
        {
            if (currentSocket == null)
            {
                throw new InvalidOperationException("Not connected! :( .");
            }

            try
            {
                await semaphore.WaitAsync();

                await new QueryMessage(query).Send(writeSocket, writeBuffer);

                var types = new List<string>();
                var rows = new List<List<string>>();

                object message;
                do
                {
                    message = await messageReader.ReadMessage(stream, CancellationToken.None);

                    if (message is RowDescription rd)
                    {
                        foreach (var rdValue in rd.Values)
                        {
                            var type = await typeInformationLoader.GetTypeNameForOid(rdValue.FieldDataTypeObjectId);
                            types.Add(type);
                        }
                    }
                    else if (message is DataRow dr)
                    {
                        var row = new List<string>();

                        foreach (var value in dr.Values)
                        {
                            if (value == null)
                            {
                                row.Add("null");
                                continue;
                            }
                            row.Add(streamEncoding.GetString(value));
                        }

                        rows.Add(row);
                    }
                } while (message != null && !(message is CommandComplete));

                return string.Join("\r\n", rows.Select(x => string.Join(", ", x)));
            }
            finally
            {
                semaphore.Release();
            }
        }

        public void Dispose()
        {
            currentSocket?.Dispose();
            stream?.Dispose();
            semaphore?.Dispose();
        }
    }
}