namespace PostgresPigeon.Internals
{
    using System;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class SocketFactory
    {
        private const AddressFamily Ipv4AddressFamily = AddressFamily.InterNetwork;
        private const int DefaultPostgresPort = 5432;

        public static async Task<Socket> Create(DbConnectionStringBuilder csb, CancellationToken cancellationToken)
        {
            var host = csb["Host"].ToString();
            var connectionStringPort = csb["Port"];

            var port = DefaultPostgresPort;

            if (connectionStringPort != null && int.TryParse(connectionStringPort.ToString(), out port))
            {
            }

            var addresses = await Dns.GetHostAddressesAsync(host);

            cancellationToken.ThrowIfCancellationRequested();

            foreach (var ipAddress in addresses)
            {
                var endpoint = new IPEndPoint(ipAddress, port);
                try
                {
                    var protocolType = ipAddress.AddressFamily == Ipv4AddressFamily ? ProtocolType.Tcp : ProtocolType.IP;

                    var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, protocolType);

                    try
                    {
                        await socket.ConnectAsync(endpoint);

                        cancellationToken.ThrowIfCancellationRequested();

                        if (socket.AddressFamily == AddressFamily.InterNetwork)
                        {
                            socket.NoDelay = true;
                        }

                        return socket;
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            socket.Dispose();
                        }
                        catch
                        {
                            // ignored
                        }
                        // ignored

                        if (ex is TaskCanceledException)
                        {
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Failed due to an exception: {ex}.");

                    if (ex is TaskCanceledException)
                    {
                        throw;
                    }
                }
            }

            throw new InvalidOperationException($"Could not connect to the server {host}.");
        }
    }
}
