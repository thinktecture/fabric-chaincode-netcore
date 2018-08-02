using Thinktecture.HyperledgerFabric.Chaincode.NET.Handler;

namespace Thinktecture.HyperledgerFabric.Chaincode.NET.Messaging
{
    public interface IMessageQueueFactory
    {
        IMessageQueue Create(IHandler handler);
    }
}