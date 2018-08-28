using Grpc.Core;

namespace Thinktecture.HyperledgerFabric.Chaincode.Handler
{
    /// <summary>
    /// A factory to create an <see cref="IHandler"/>.
    /// </summary>
    public interface IHandlerFactory
    {
        IHandler Create(string host, int port, ChannelCredentials channelCredentials);
    }
}
