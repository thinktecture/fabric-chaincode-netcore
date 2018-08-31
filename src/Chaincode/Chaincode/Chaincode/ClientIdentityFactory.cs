using System.Diagnostics.CodeAnalysis;

namespace Thinktecture.HyperledgerFabric.Chaincode.Chaincode
{
    [ExcludeFromCodeCoverage]
    public class ClientIdentityFactory : IClientIdentityFactory
    {
        public IClientIdentity Create(IChaincodeStub stub)
        {
            return new ClientIdentity(stub);
        }
    }
}
