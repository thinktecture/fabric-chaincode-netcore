using Thinktecture.HyperledgerFabric.Chaincode.Chaincode;

namespace Thinktecture.HyperledgerFabric.Chaincode.Contract
{
    /// <summary>
    /// Factory to create a new <see cref="IContractContext"/>
    /// </summary>
    public interface IContractContextFactory
    {
        /// <summary>
        /// Creates a new <see cref="IContractContext"/>.
        /// </summary>
        /// <param name="stub"></param>
        /// <returns></returns>
        IContractContext Create(IChaincodeStub stub);
    }
}
