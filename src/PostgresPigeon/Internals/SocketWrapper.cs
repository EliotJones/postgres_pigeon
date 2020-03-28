namespace PostgresPigeon.Internals
{
    using System;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;

    public class SocketWrapper : INotifyCompletion
    {
        private readonly Socket socket;
        private readonly SocketAsyncEventArgs args;

        public bool IsCompleted { get; private set; }

        public SocketWrapper(Socket socket, SocketAsyncEventArgs args)
        {
            this.socket = socket;
            this.args = args;
            args.Completed += (sender, eventArgs) => { };
        }

        public SocketWrapper Send(WriteBuffer sender)
        {
            sender.Bind(args);

            IsCompleted = false;

            var result = socket.SendAsync(args);

            if (!result)
            {
                IsCompleted = true;
            }

            return this;
        }

        public SocketWrapper GetAwaiter() => this;

        public void GetResult() {}

        public void OnCompleted(Action continuation)
        {
            continuation?.Invoke();
        }
    }
}
