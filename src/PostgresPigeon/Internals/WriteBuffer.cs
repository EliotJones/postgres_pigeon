namespace PostgresPigeon.Internals
{
    using System;
    using System.Net.Sockets;
    using System.Text;

    public class WriteBuffer
    {
        private readonly byte[] buffer = new byte[1024 * 8];

        private int length;

        public Encoding Encoding { get; }

        public WriteBuffer(Encoding encoding)
        {
            Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public void WriteByte(char b) => WriteByte((byte) b);
        public void WriteByte(byte b)
        {
            buffer[length++] = b;
        }

        public void WriteInt32(int i)
        {
            buffer[length++] = (byte)(i >> 24);
            buffer[length++] = (byte)(i >> 16);
            buffer[length++] = (byte)(i >> 8);
            buffer[length++] = (byte) (i >> 0);
        }

        public void WriteBytes(byte[] s)
        {
            Array.Copy(s, 0, buffer, length, s.Length);
            length += s.Length;
        }

        public void Bind(SocketAsyncEventArgs e)
        {
            e.SetBuffer(buffer, 0, length);
        }

        public void Reset()
        {
            length = 0;
        }

        public byte[] GetStringBytes(string s)
        {
            return Encoding.GetBytes(s);
        }
    }
}
