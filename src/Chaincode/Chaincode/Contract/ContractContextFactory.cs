using System.Diagnostics.CodeAnalysis;
using Thinktecture.HyperledgerFabric.Chaincode.Chaincode;

namespace Thinktecture.HyperledgerFabric.Chaincode.Contract
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public class ContractContextFactory : IContractContextFactory
    {
        private readonly IClientIdentityFactory _clientIdentityFactory;

        public ContractContextFactory(IClientIdentityFactory clientIdentityFactory)
        {
            _clientIdentityFactory = clientIdentityFactory;
        }

        public IContractContext Create(IChaincodeStub stub)
        {
            return new ContractContext()
            {
                Stub = stub,
                ClientIdentity = _clientIdentityFactory.Create(stub)
            };
        }
    }
}
