using System;
using System.Collections.Generic;

namespace Thinktecture.HyperledgerFabric.Chaincode.NET.Chaincode
{
    public class Parameters : List<string>
    {
        public void AssertCount(int count)
        {
            if (Count != count) throw new Exception($"Incorrect number of arguments. Expecting {count}, got {Count}");
        }
    }

    public class ChaincodeFunctionParameterInformation
    {
        public string Function { get; set; }
        public Parameters Parameters { get; set; } = new Parameters();
    }
}