using Protos;
using Thinktecture.HyperledgerFabric.Chaincode.Handler;

namespace Thinktecture.HyperledgerFabric.Chaincode.Chaincode
{
    /// <summary>
    /// Factory to create <see cref="IChaincodeStub"/> objects
    /// </summary>
    public interface IChaincodeStubFactory
    {
        IChaincodeStub Create(
            IHandler handler,
            string channelId,
            string txId,
            ChaincodeInput chaincodeInput,
            SignedProposal signedProposal
        );
    }
}
