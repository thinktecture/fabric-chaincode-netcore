using Grpc.Core;

namespace Thinktecture.HyperledgerFabric.Chaincode.Handler
{
    public interface IHandlerFactory
    {
        IHandler Create(string host, int port, ChannelCredentials channelCredentials);
    }
}
