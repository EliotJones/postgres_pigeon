namespace PostgresPigeon.Internals.Messages.Backend
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal class BackendMessageReader
    {
        // 1 byte for the type and 4 for the length.
        private const int HeaderLength = 1 + 4;

        private readonly byte[] buffer = new byte[1024 * 8];
        private readonly Encoding encoding;

        public BackendMessageReader(Encoding encoding)
        {
            this.encoding = encoding;
        }

        public async Task<object> ReadMessage(Stream stream, CancellationToken cancellationToken)
        {
            await ReadExpectedLength(stream, HeaderLength, cancellationToken);
            var type = (char) buffer[0];
            var length = ReadInt(1) - 4;

            // TODO: read chunks.
            if (length > buffer.Length)
            {
                throw new NotSupportedException($"Can't read more ({length}) bytes than can fit in the buffer {buffer.Length} for message {type}.");
            }

            await ReadExpectedLength(stream, length, cancellationToken);

            switch (type)
            {
                case 'C':
                {
                    var str = encoding.GetString(buffer, 0, length - 1);
                    return new CommandComplete(str);
                }
                case 'D':
                    return GetDataRow(length);
                case 'R':
                    return GetAuthMessage(length);
                case 'S':
                    return GetParameterStatus(length);
                case 'T':
                    return GetRowDescription(length);
                case 'K':
                    return new BackendKeyData(ReadInt(0), ReadInt(4));
                case 'Z':
                    return ReadyForQuery.Instance;
                default:
                    throw new NotSupportedException($"Unrecognized message type: {type}.");
            }
        }

        private ParameterStatus GetParameterStatus(int length)
        {
            // ignore last /0 byte and /0 byte following value
            length = length - 2;

            var x = 0;
            for (var i = 0; i < length; i++)
            {
                if (buffer[i] == 0)
                {
                    x = i;
                    break;
                }
            }

            if (x == 0)
            {
                x = length;
            }

            var key = encoding.GetString(buffer, 0, x);
            var val = encoding.GetString(buffer, x + 1, length - x);

            return new ParameterStatus(key, val);
        }

        private object GetRowDescription(int length)
        {
            var numberOfFields = ReadShort(0);

            var offset = 2;

            var columns = new List<RowDescription.Value>();

            for (var i = 0; i < numberOfFields; i++)
            {
                if (offset > length)
                {
                    throw new InvalidOperationException("We read too much data for a preceding row!");
                }

                var startAt = offset;
                while (offset < length)
                {
                    if (buffer[offset] == 0)
                    {
                        break;
                    }
                    offset++;
                }

                var name = encoding.GetString(buffer, startAt, offset - startAt);
                offset++;

                var tableObjectId = ReadInt(offset);
                offset += 4;
                var columnAttributeNumber = ReadShort(offset);
                offset += 2;
                var fieldDataTypeObjectId = ReadInt(offset);
                offset += 4;
                var dataTypeSize = ReadShort(offset);
                offset += 2;
                var typeModifier = ReadInt(offset);
                offset += 4;
                var formatCode = ReadShort(offset);
                offset += 2;

                columns.Add(new RowDescription.Value(name, tableObjectId, columnAttributeNumber,
                    fieldDataTypeObjectId,
                    dataTypeSize,
                    typeModifier, 
                    formatCode));
            }

            return new RowDescription(columns);
        }

        private object GetDataRow(int length)
        {
            var offset = 0;
            var numberOfColumns = ReadShort(0);
            offset += 2;

            var values = new byte[numberOfColumns][];

            for (var i = 0; i < numberOfColumns; i++)
            {
                var valueLength = ReadInt(offset);
                offset += 4;
                if (valueLength == -1)
                {
                    values[i] = null;
                }
                else
                {
                    var val = new byte[valueLength];
                    Array.Copy(buffer, offset, val, 0, valueLength);
                    values[i] = val;
                    offset += valueLength;
                }
            }

            return new DataRow(values);
        }

        private BackendAuthMessage GetAuthMessage(int length)
        {
            var authType = (AuthType)ReadInt(0);

            var data = new byte[length - 4];

            Array.Copy(buffer, 4, data, 0, length - 4);

            return new BackendAuthMessage(authType, data);
        }

        private async Task ReadExpectedLength(Stream stream, int length, CancellationToken ct)
        {
            var read = await stream.ReadAsync(buffer, 0, length, ct);

            if (read != length)
            {
                throw new InvalidOperationException($"Expected to read {length} bytes but only got {read} bytes.");
            }
        }

        private int ReadInt(int startAt)
        {
            return ((buffer[startAt] << 24) + (buffer[startAt + 1] << 16) + (buffer[startAt + 2] << 8) + buffer[startAt + 3]);
        }

        private short ReadShort(int startAt)
        {
            return (short)((buffer[startAt] << 8) + buffer[startAt + 1]);
        }
    }

    internal enum AuthType
    {
        Success = 0,
        KerberosV5 = 2,
        Cleartext = 3,
        Md5 = 5,
        Scm = 6,
        Gss = 7,
        GssContinue = 8,
        Sspi = 9,
        Sasl = 10,
        SaslChallenge = 11,
        SaslFinal = 12
    }
}
