namespace PostgresPigeon.Internals.Messages.Backend
{
    internal class BackendKeyData
    {
        public int ProcessId { get; }

        public int SecretKey { get; }

        public BackendKeyData(int processId, int secretKey)
        {
            ProcessId = processId;
            SecretKey = secretKey;
        }

        public override string ToString()
        {
            return $"Process ID: {ProcessId}, Key: {SecretKey}.";
        }
    }
}
