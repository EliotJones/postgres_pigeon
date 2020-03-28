namespace PostgresPigeon
{
    using System;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using Internals;

    public class PostgresConnection : IDisposable
    {
        private readonly string connectionString;

        private readonly SemaphoreSlim locker = new SemaphoreSlim(1, 1);

        private ConnectionState state = ConnectionState.Closed;
        private Connector connector;

        public PostgresConnection(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("message", nameof(connectionString));
            }

            this.connectionString = connectionString;
        }

        public async Task Open(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                await locker.WaitAsync(cancellationToken);

                if (state != ConnectionState.Closed)
                {
                    throw new InvalidOperationException();
                }

                connector = new Connector(connectionString);

                state = ConnectionState.Connecting;

                await connector.Open(cancellationToken);

                state = ConnectionState.Open;
            }
            finally
            {
                locker.Release();
            }
        }

        public Task<string> ExecuteCommand(string command)
        {
            if (state != ConnectionState.Open)
            {
                throw new InvalidOperationException();
            }

            return connector.Query(command);
        }

        public void Dispose()
        {
            connector?.Dispose();
            state = ConnectionState.Closed;
            locker?.Dispose();
        }
    }
}
