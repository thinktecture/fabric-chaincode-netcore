using Grpc.Core;
using Protos;

namespace Thinktecture.HyperledgerFabric.Chaincode.Handler
{
    /// <summary>
    /// Factory to create a <see cref="ChaincodeSupport.ChaincodeSupportClient"/>.
    /// </summary>
    public interface IChaincodeSupportClientFactory
    {
        ChaincodeSupport.ChaincodeSupportClient Create(Channel channel);
    }
}
