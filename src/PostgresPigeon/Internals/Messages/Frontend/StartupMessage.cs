namespace PostgresPigeon.Internals.Messages.Frontend
{
    using System.Threading.Tasks;

    internal class StartupMessage
    {
        private const int ProtocolVersion = 196608;

        private const string User = "user";
        private const string Database = "database";

        private readonly string username;
        private readonly string database;

        public StartupMessage(string username, string database)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new System.ArgumentException("message", nameof(username));
            }

            if (string.IsNullOrWhiteSpace(database))
            {
                throw new System.ArgumentException("message", nameof(database));
            }

            this.username = username;
            this.database = database;
        }

        public async Task Send(SocketWrapper wrapper, WriteBuffer sender)
        {
            var userBytes = sender.GetStringBytes(User);
            var userValueBytes = sender.GetStringBytes(username);

            var databaseBytes = sender.GetStringBytes(Database);
            var databaseValueBytes = sender.GetStringBytes(database);

            var length = sizeof(int) + sizeof(int)
                                     + userBytes.Length + 1
                                     + userValueBytes.Length + 1
                                     + databaseBytes.Length + 1
                                     + databaseValueBytes.Length + 1
                                     + 1;
            
            sender.WriteInt32(length);
            sender.WriteInt32(ProtocolVersion);

            sender.WriteBytes(userBytes);
            sender.WriteByte(0);
            sender.WriteBytes(userValueBytes);
            sender.WriteByte(0);

            sender.WriteBytes(databaseBytes);
            sender.WriteByte(0);
            sender.WriteBytes(databaseValueBytes);
            sender.WriteByte(0);

            sender.WriteByte(0);

            await wrapper.Send(sender);
            sender.Reset();
        }
    }

}
