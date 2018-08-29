using Thinktecture.HyperledgerFabric.Chaincode.Chaincode;

namespace Thinktecture.HyperledgerFabric.Chaincode.Contract
{
    public interface IContractContext
    {
        IChaincodeStub Stub { get; set; }
        IClientIdentity ClientIdentity { get; set; }
    }
}