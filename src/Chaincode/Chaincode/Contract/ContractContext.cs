using System.Diagnostics.CodeAnalysis;
using Thinktecture.HyperledgerFabric.Chaincode.Chaincode;

namespace Thinktecture.HyperledgerFabric.Chaincode.Contract
{
    [ExcludeFromCodeCoverage]
    public class ContractContext : IContractContext
    {
        public IChaincodeStub Stub { get; set; }
        public IClientIdentity ClientIdentity { get; set; }
    }
}
