using System.Collections.Generic;

namespace Chaincode.NET.Chaincode
{
    public class Parameters : List<string> {}   
    
    public class ChaincodeFunctionParameterInformation
    {
        public string Function { get; set; }
        public Parameters Parameters { get; set; } = new Parameters();
    }
}
