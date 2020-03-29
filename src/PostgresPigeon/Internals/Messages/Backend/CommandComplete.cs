namespace PostgresPigeon.Internals.Messages.Backend
{
    internal class CommandComplete
    {
        public string CommandTag { get; }

        public CommandComplete(string commandTag)
        {
            CommandTag = commandTag;
        }
    }
}
