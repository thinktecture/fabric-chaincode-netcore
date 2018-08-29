namespace Thinktecture.HyperledgerFabric.Chaincode.Chaincode
{
    public interface IClientIdentityFactory
    {
        IClientIdentity Create(IChaincodeStub stub);
    }
}
