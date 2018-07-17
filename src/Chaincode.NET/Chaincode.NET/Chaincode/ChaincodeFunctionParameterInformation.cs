using System.Collections.Generic;

namespace Chaincode.NET.Chaincode
{
    public class ChaincodeFunctionParameterInformation
    {
        public string Function { get; set; }
        public IList<string> Parameters { get; set; } = new List<string>();
    }
}