using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Google.Protobuf;
using Newtonsoft.Json;
using Thinktecture.HyperledgerFabric.Chaincode.Extensions;

namespace Thinktecture.HyperledgerFabric.Chaincode.Contract
{
    /// <summary>
    /// This is a contract that determines functions that can be invoked to provide general information.
    /// </summary>
    public class SystemContract : ContractBase
    {
        private readonly ChaincodeFromContracts _chaincodeFromContracts;

        public SystemContract(ChaincodeFromContracts chaincodeFromContracts)
            : base("org.hyperledger.fabric")
        {
            _chaincodeFromContracts = chaincodeFromContracts;
        }

        /// <summary>
        /// Returns meta information about the contract.
        /// </summary>
        /// <param name="context">The <see cref="IContractContext"/>.</param>
        /// <returns>meta information.</returns>
        [ExcludeFromCodeCoverage]
        public Task<ByteString> GetMetadata(IContractContext context)
        {
            return Task.FromResult(JsonConvert.SerializeObject(_chaincodeFromContracts.GetContracts()).ToByteString());
        } 
    }
}
