namespace PostgresPigeon.Internals.Messages.Backend
{
    using System;

    internal class BackendAuthMessage
    {
        public AuthType Type { get; }

        public byte[] Data { get; }

        public BackendAuthMessage(AuthType type, byte[] data)
        {
            Type = type;
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public override string ToString()
        {
            return $"{Type} with {Data.Length} bytes.";
        }
    }
}
