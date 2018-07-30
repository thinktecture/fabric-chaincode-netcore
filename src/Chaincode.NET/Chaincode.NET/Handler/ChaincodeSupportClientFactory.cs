using Grpc.Core;
using Protos;

namespace Chaincode.NET.Handler
{
    public class ChaincodeSupportClientFactory : IChaincodeSupportClientFactory
    {
        public ChaincodeSupport.ChaincodeSupportClient Create(Channel channel)
        {
            return new ChaincodeSupport.ChaincodeSupportClient(channel);
        }
    }
}