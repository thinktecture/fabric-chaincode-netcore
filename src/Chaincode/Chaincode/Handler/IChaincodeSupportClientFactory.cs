using Grpc.Core;
using Protos;

namespace Thinktecture.HyperledgerFabric.Chaincode.Handler
{
    public interface IChaincodeSupportClientFactory
    {
        ChaincodeSupport.ChaincodeSupportClient Create(Channel channel);
    }
}
