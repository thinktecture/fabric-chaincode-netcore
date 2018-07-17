using Chaincode.NET.Handler;
using Protos;

namespace Chaincode.NET.Chaincode
{
    public interface IChaincodeStubFactory
    {
        ChaincodeStub CreateChaincodeStub(
            IHandler handler,
            string channelId,
            string txId,
            ChaincodeInput chaincodeInput,
            SignedProposal signedProposal
        );
    }
}