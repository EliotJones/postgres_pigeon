namespace PostgresPigeon.Internals.Messages.Backend
{
    using System.Collections.Generic;
    using System.Diagnostics;

    internal class RowDescription
    {
        public IReadOnlyList<Value> Values { get; }

        public RowDescription(IReadOnlyList<Value> values)
        {
            Values = values;
        }

        public class Value
        {
            public string Name { get; set; }

            public int TableObjectId { get; }

            public short ColumnAttributeNumber { get; }

            public int FieldDataTypeObjectId { get; }

            public short DataTypeSize { get; }

            public int TypeModifier { get; }

            public short FormatCode { get; }

            public Value(string name, int tableObjectId, short columnAttributeNumber, 
                int fieldDataTypeObjectId,
                short dataTypeSize,
                int typeModifier, 
                short formatCode)
            {
                Name = name;
                TableObjectId = tableObjectId;
                ColumnAttributeNumber = columnAttributeNumber;
                FieldDataTypeObjectId = fieldDataTypeObjectId;
                DataTypeSize = dataTypeSize;
                TypeModifier = typeModifier;
                FormatCode = formatCode;
            }
        }
    }
}
