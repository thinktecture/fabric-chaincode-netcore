using System.Collections.Generic;

namespace Thinktecture.HyperledgerFabric.Chaincode.Contract
{
    public class MetaContract : ContractBase
    {
        public MetaContract(string @namespace, IDictionary<string, string> metadata = null)
            : base(@namespace, metadata)
        {
        }
    }
}
