using System.Threading.Tasks;
using Google.Protobuf;
using Newtonsoft.Json;
using Thinktecture.HyperledgerFabric.Chaincode.Extensions;

namespace Thinktecture.HyperledgerFabric.Chaincode.Contract
{
    /// <summary>
    /// Stub for an upcoming meta contract which will describe the actual <see cref="ChaincodeFromContracts"/>.
    /// </summary>
    public class MetaContract : ContractBase
    {
        public MetaContract()
            : base("org.hyperledger.fabric")
        {
        }

        /// <summary>
        /// Returns meta information about the contract.
        /// </summary>
        /// <param name="context">The <see cref="IContractContext"/>.</param>
        /// <returns>meta information.</returns>
        public Task<ByteString> GetMetadata(IContractContext context)
        {
            return Task.FromResult(JsonConvert.SerializeObject(Metadata).ToByteString());
        } 
    }
}
