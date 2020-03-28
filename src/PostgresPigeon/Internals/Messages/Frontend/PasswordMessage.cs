namespace PostgresPigeon.Internals.Messages.Frontend
{
    using System;
    using System.Threading.Tasks;

    internal class PasswordMessage : IFrontendMessage
    {
        public byte[] Bytes { get; }

        public PasswordMessage(byte[] bytes)
        {
            Bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
        }

        public async Task Send(SocketWrapper wrapper, WriteBuffer sender)
        {
            var length = sizeof(int)
                           + Bytes.Length + 1;

            sender.Reset();
            sender.WriteByte('p');
            sender.WriteInt32(length);
            sender.WriteBytes(Bytes);
            sender.WriteByte(0);

            await wrapper.Send(sender);
        }
    }
}
