using System;
using System.Collections.Generic;
using Google.Protobuf;
using Thinktecture.HyperledgerFabric.Chaincode.Chaincode;

namespace Thinktecture.HyperledgerFabric.Chaincode.Contract
{
    /// <summary>
    /// Recommended base class to build custom contracts.
    /// </summary>
    public abstract class ContractBase : IContract
    {
        public virtual IContractContext BeforeInvocation(IContractContext context)
        {
            return context;
        }

        public virtual void AfterInvocation(IContractContext context, ByteString result)
        {
        }

        public virtual void UnknownFunctionCalled(IContractContext context, string functionName)
        {
            throw new Exception($"Invocation of {functionName} failed: Function does not exist.");
        }

        public IClientIdentity ClientIdentity
        {
            get { throw new NotImplementedException(); }
        }

        public IDictionary<string, string> Metadata { get; }
        public string Namespace { get; set; }

        public ContractBase(string @namespace, IDictionary<string, string> metadata = null)
        {
            Namespace = @namespace.Trim();
            Metadata = metadata ?? new Dictionary<string, string>();

            if (String.IsNullOrWhiteSpace(Namespace))
            {
                Namespace = "contract";
            }
        }
    }
}
