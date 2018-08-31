using System.Diagnostics.CodeAnalysis;
using Grpc.Core;
using Protos;

namespace Thinktecture.HyperledgerFabric.Chaincode.Handler
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage] 
    public class ChaincodeSupportClientFactory : IChaincodeSupportClientFactory
    {
        public ChaincodeSupport.ChaincodeSupportClient Create(Channel channel)
        {
            return new ChaincodeSupport.ChaincodeSupportClient(channel);
        }
    }
}
