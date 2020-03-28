namespace PostgresPigeon.Internals.Messages.Backend
{
    internal class ReadyForQuery
    {
        public static ReadyForQuery Instance { get; } = new ReadyForQuery();

        private ReadyForQuery()
        {
        }

        public override string ToString()
        {
            return $"Ready for query! 🎉.";
        }
    }
}
