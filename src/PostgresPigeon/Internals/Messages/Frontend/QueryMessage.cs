namespace PostgresPigeon.Internals.Messages.Frontend
{
    using System.Threading.Tasks;

    internal class QueryMessage : IFrontendMessage
    {
        public string Query { get; }

        public QueryMessage(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new System.ArgumentException("message", nameof(query));
            }

            Query = query;
        }

        public async Task Send(SocketWrapper wrapper, WriteBuffer sender)
        {
            var bytes = sender.GetStringBytes(Query);
            var length = 4 + bytes.Length + 1;

            sender.Reset();
            sender.WriteByte('Q');
            sender.WriteInt32(length);
            sender.WriteBytes(bytes);
            sender.WriteByte(0);

            await wrapper.Send(sender);
        }
    }
}
