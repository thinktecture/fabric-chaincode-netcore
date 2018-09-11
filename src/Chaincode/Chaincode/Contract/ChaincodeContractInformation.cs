using System.Collections.Generic;

namespace Thinktecture.HyperledgerFabric.Chaincode.Contract
{
    public class ChaincodeContractInformation
    {
        public string Namespace { get; set; }
        public IList<string> FunctionNames { get; set; }
        public IContract Contract { get; set; }
    }
}