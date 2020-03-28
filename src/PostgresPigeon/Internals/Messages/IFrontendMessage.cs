namespace PostgresPigeon.Internals.Messages
{
    using System.Threading.Tasks;

    internal interface IFrontendMessage
    {
        Task Send(SocketWrapper wrapper, WriteBuffer sender);
    }
}
