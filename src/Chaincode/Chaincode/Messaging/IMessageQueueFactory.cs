using Thinktecture.HyperledgerFabric.Chaincode.Handler;

namespace Thinktecture.HyperledgerFabric.Chaincode.Messaging
{
    public interface IMessageQueueFactory
    {
        IMessageQueue Create(IHandler handler);
    }
}