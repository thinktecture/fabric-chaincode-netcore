using Thinktecture.HyperledgerFabric.Chaincode.Handler;

namespace Thinktecture.HyperledgerFabric.Chaincode.Messaging
{
    /// <summary>
    /// Factory for creating an <see cref="IMessageQueue"/>.
    /// </summary>
    public interface IMessageQueueFactory
    {
        IMessageQueue Create(IHandler handler);
    }
}
