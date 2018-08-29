using System.Collections.Generic;
using Google.Protobuf;
using Thinktecture.HyperledgerFabric.Chaincode.Chaincode;

namespace Thinktecture.HyperledgerFabric.Chaincode.Contract
{
    public interface IContract
    {
        IContractContext BeforeInvocation(IContractContext context);
        void AfterInvocation(IContractContext context, ByteString result);
        void UnknownFunctionCalled(IContractContext context, string functionName);

        IClientIdentity ClientIdentity { get; }
        IDictionary<string, string> Metadata { get; }
        string Namespace { get; set; }
    }
}
