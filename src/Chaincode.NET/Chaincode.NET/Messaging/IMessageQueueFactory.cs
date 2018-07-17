using Chaincode.NET.Handler;

namespace Chaincode.NET.Messaging
{
    public interface IMessageQueueFactory
    {
        IMessageQueue Create(IHandler handler);
    }
}