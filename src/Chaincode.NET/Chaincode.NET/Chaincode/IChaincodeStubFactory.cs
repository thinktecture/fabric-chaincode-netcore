using Chaincode.NET.Protos;
using Thinktecture.HyperledgerFabric.Chaincode.NET.Handler;

namespace Thinktecture.HyperledgerFabric.Chaincode.NET.Chaincode
{
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