namespace PostgresPigeon.Internals.Messages.Backend
{
    using System;

    internal class DataRow
    {
        public byte[][] Values { get; set; }

        public DataRow(byte[][] values)
        {
            Values = values ?? throw new ArgumentNullException(nameof(values));
        }
    }
}
