using Thinktecture.HyperledgerFabric.Chaincode.Chaincode;

namespace Thinktecture.HyperledgerFabric.Chaincode.Contract
{
    /// <summary>
    /// Data class providing the context of a contract.
    /// </summary>
    public interface IContractContext
    {
        /// <summary>
        /// An <see cref="IChaincodeStub"/> used to communicate with the peer.
        /// </summary>
        IChaincodeStub Stub { get; set; }

        /// <summary>
        /// The <see cref="IClientIdentity"/>.
        /// </summary>
        IClientIdentity ClientIdentity { get; set; }
    }
}
