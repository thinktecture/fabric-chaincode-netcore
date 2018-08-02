using Chaincode.NET.Protos;
using Grpc.Core;

namespace Thinktecture.HyperledgerFabric.Chaincode.NET.Handler
{
    public class ChaincodeSupportClientFactory : IChaincodeSupportClientFactory
    {
        public ChaincodeSupport.ChaincodeSupportClient Create(Channel channel)
        {
            return new ChaincodeSupport.ChaincodeSupportClient(channel);
        }
    }
}