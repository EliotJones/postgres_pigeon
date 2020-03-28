namespace PostgresPigeon.Internals.Messages.Backend
{
    internal class ParameterStatus
    {
        public string Name { get; }

        public string Value { get; }

        public ParameterStatus(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            return $"[{Name}] = {Value}.";
        }
    }
}
