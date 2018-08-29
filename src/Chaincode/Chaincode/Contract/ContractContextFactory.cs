using Thinktecture.HyperledgerFabric.Chaincode.Chaincode;

namespace Thinktecture.HyperledgerFabric.Chaincode.Contract
{
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