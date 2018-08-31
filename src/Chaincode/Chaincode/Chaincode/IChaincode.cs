using System.Threading.Tasks;
using Protos;

namespace Thinktecture.HyperledgerFabric.Chaincode.Chaincode
{
    /// <summary>
    /// Chaincodes must implement the methods in this interface. The <see cref="Init"/> method is called during
    /// chaincode <code>instantiation</code> or <code>upgrade</code> to preform any necessary initialization
    /// of the application state. <see cref="Invoke"/> is called by <code>invoke transaction</code> or <code>query</code>
    /// requests. Both methods are provided with a <see cref="ChaincodeStub"/> object that can be used to
    /// discover information on the request (invoking identity, target channel, arguments, etc.) as well as
    /// talking with the peer to retrieve or update application state.
    /// </summary>
    public interface IChaincode
    {
        /// <summary>
        /// Called during chaincode instantiate and upgrade. This method can be used to initialise asset store
        /// </summary>
        /// <param name="stub">The chaincode stub is implemented by the <code>fabric-shim</code> library and passed
        /// to the ChaincodeInterface calls by the Hyperledger Fabric platform. The stub encapsulates the APIs
        /// between the chaincode implementation and the Fabric peer.</param>
        /// <returns></returns>
        Task<Response> Init(IChaincodeStub stub);

        /// <summary>
        /// Called throughout the life time of the chaincode to carry out business transaction logic and effect the
        /// asset state.
        /// </summary>
        /// <param name="stub"></param>
        /// <returns></returns>
        Task<Response> Invoke(IChaincodeStub stub);
    }
}
