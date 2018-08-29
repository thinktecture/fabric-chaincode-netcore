using Thinktecture.HyperledgerFabric.Chaincode.Chaincode;

namespace Thinktecture.HyperledgerFabric.Chaincode.Contract
{
    public class ContractContext : IContractContext
    {
        public IChaincodeStub Stub { get; set; }
        public IClientIdentity ClientIdentity { get; set; }
    }
}