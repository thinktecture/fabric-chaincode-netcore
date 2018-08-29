namespace Thinktecture.HyperledgerFabric.Chaincode.Chaincode
{
    public class ClientIdentityFactory : IClientIdentityFactory
    {
        public IClientIdentity Create(IChaincodeStub stub)
        {
            return new ClientIdentity(stub);
        }
    }
}
