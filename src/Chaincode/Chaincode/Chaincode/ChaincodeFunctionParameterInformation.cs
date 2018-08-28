using System;
using System.Collections.Generic;

namespace Thinktecture.HyperledgerFabric.Chaincode.Chaincode
{
    public class Parameters : List<string>
    {
        /// <summary>
        /// Checks if there are as many attributes as defined in <paramref name="count"/> <see cref="count"/>
        /// </summary>
        /// <param name="count">The expected parameter count</param>
        /// <exception cref="Exception"></exception>
        public void AssertCount(int count)
        {
            if (Count != count) throw new Exception($"Incorrect number of arguments. Expecting {count}, got {Count}");
        }
    }

    /// <summary>
    /// This class provides information about the actual function being called in the <see cref="IChaincode"/>
    /// implementation. Additionally, it provides a list of parameters.
    /// </summary>
    public class ChaincodeFunctionParameterInformation
    {
        /// <summary>
        /// The actual function being called.
        /// </summary>
        public string Function { get; set; }
        
        /// <summary>
        /// A list of parameters for the function being called.
        /// </summary>
        public Parameters Parameters { get; set; } = new Parameters();
    }
}
