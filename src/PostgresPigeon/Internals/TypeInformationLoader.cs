namespace PostgresPigeon.Internals
{
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Messages.Backend;
    using Messages.Frontend;

    internal class TypeInformationLoader
    {
        private readonly NetworkStream stream;
        private readonly BackendMessageReader backendMessageReader;
        private readonly WriteBuffer writeBuffer;
        private readonly SocketWrapper writeSocket;
        private readonly Encoding encoding;

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        private bool loaded;
        private readonly Dictionary<int, string> oidsToTypeNames = new Dictionary<int, string>();

        public TypeInformationLoader(NetworkStream stream, BackendMessageReader backendMessageReader,
            WriteBuffer writeBuffer,
            SocketWrapper writeSocket,
            Encoding encoding)
        {
            this.stream = stream;
            this.backendMessageReader = backendMessageReader;
            this.writeBuffer = writeBuffer;
            this.writeSocket = writeSocket;
            this.encoding = encoding;
        }

        public async Task<string> GetTypeNameForOid(int oid)
        {
            try
            {
                await semaphore.WaitAsync();

                if (loaded)
                {
                    return oidsToTypeNames[oid];
                }

                await new QueryMessage(@"select oid, typname from pg_type;")
                    .Send(writeSocket, writeBuffer);

                object message;

                do
                {
                    message = await backendMessageReader.ReadMessage(stream, CancellationToken.None);

                    if (message is DataRow row)
                    {
                        var oidR = int.Parse(encoding.GetString(row.Values[0]));
                        var typename = encoding.GetString(row.Values[1]);

                        oidsToTypeNames[oidR] = typename;
                    }
                } while (!(message is CommandComplete));

                loaded = true;

                return oidsToTypeNames[oid];
            }
            finally
            {
                semaphore.Release();
            }
        }

        private static int ReadShort(byte[] value)
        {
            return ((value[0] << 8) + value[1]);
        }
    }
}
