using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Thinktecture.HyperledgerFabric.Chaincode.Chaincode
{
    public interface IClientIdentity
    {
        string Id { get; }
        string Mspid { get; }
        X509Certificate2 X509Certificate { get; }
        IDictionary<string, string> Attributes { get; }
        string GetAttributeValue(string attributeName);
        bool AssertAttributeValue(string attributeName, string attributeValue);
    }
}