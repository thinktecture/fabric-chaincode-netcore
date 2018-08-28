using Grpc.Core;
using Protos;

namespace Thinktecture.HyperledgerFabric.Chaincode.Handler
{
    /// <inheritdoc />
    public class ChaincodeSupportClientFactory : IChaincodeSupportClientFactory
    {
        public ChaincodeSupport.ChaincodeSupportClient Create(Channel channel)
        {
            return new ChaincodeSupport.ChaincodeSupportClient(channel);
        }
    }
}
