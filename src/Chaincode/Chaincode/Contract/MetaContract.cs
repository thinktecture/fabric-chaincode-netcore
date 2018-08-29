using System.Threading.Tasks;
using Google.Protobuf;
using Newtonsoft.Json;
using Thinktecture.HyperledgerFabric.Chaincode.Extensions;

namespace Thinktecture.HyperledgerFabric.Chaincode.Contract
{
    public class MetaContract : ContractBase
    {
        public MetaContract()
            : base("org.hyperledger.fabric", null)
        {
        }

        public Task<ByteString> GetMetadata(IContractContext context)
        {
            return Task.FromResult(JsonConvert.SerializeObject(Metadata).ToByteString());
        } 
    }
}
