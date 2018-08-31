using System.Collections.Generic;
using Google.Protobuf;
using Thinktecture.HyperledgerFabric.Chaincode.Chaincode;

namespace Thinktecture.HyperledgerFabric.Chaincode.Contract
{
    /// <summary>
    /// Base interface to implement when using <see cref="ChaincodeFromContracts"/>.
    /// It's recommended to use <see cref="ContractBase"/> as base instead.
    /// </summary>
    public interface IContract
    {
        /// <summary>
        /// Hook which is executed before the actual chaincode invocation.
        /// </summary>
        /// <param name="context">The contract context.</param>
        /// <returns>A (new) IContractContext.</returns>
        IContractContext BeforeInvocation(IContractContext context);
        
        /// <summary>
        /// Hook which is executed after the actual chaincode invocation.
        /// </summary>
        /// <param name="context">The contract context.</param>
        /// <param name="result">Result of the actual chaincode invocation.</param>
        void AfterInvocation(IContractContext context, ByteString result);
        
        /// <summary>
        /// Methods which is called when an unknown function within the contract has been called.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="functionName"></param>
        void UnknownFunctionCalled(IContractContext context, string functionName);

        /// <summary>
        /// The <see cref="IClientIdentity"/>.
        /// </summary>
        IClientIdentity ClientIdentity { get; }
        
        /// <summary>
        /// Metadata about the contract.
        /// </summary>
        IDictionary<string, string> Metadata { get; }
        
        /// <summary>
        /// The namespace of the contract. 
        /// </summary>
        string Namespace { get; set; }
    }
}
