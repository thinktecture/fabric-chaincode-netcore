using Thinktecture.HyperledgerFabric.Chaincode.Chaincode;

namespace Thinktecture.HyperledgerFabric.Chaincode.Contract
{
    public interface IContractContextFactory
    {
        IContractContext Create(IChaincodeStub stub);
    }
}