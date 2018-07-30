using Grpc.Core;
using Protos;

namespace Chaincode.NET.Handler
{
    public interface IChaincodeSupportClientFactory
    {
        ChaincodeSupport.ChaincodeSupportClient Create(Channel channel);
    }
}