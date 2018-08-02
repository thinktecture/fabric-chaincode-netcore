using Chaincode.NET.Protos;
using Grpc.Core;

namespace Thinktecture.HyperledgerFabric.Chaincode.NET.Handler
{
    public interface IChaincodeSupportClientFactory
    {
        ChaincodeSupport.ChaincodeSupportClient Create(Channel channel);
    }
}