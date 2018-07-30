using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Chaincode.NET.Chaincode
{
    public class ClientIdentity : IClientIdentity
    {
        private readonly IChaincodeStub _chaincodeStub;
        private const string FabricCertAttrOid = "1.2.3.4.5.6.7.8.1";

        public string Id { get; private set; }
        public string Mspid { get; private set; }
        public X509Certificate2 X509Certificate { get; private set; }

        public IDictionary<string, string> Attributes { get; private set; } =
            new ConcurrentDictionary<string, string>();

        public ClientIdentity(IChaincodeStub chaincodeStub)
        {
            _chaincodeStub = chaincodeStub;
            LoadFromStub();
        }

        public string GetAttributeValue(string attributeName) =>
            Attributes.TryGetValue(attributeName, out var value) ? value : null;

        public bool AssertAttributeValue(string attributeName, string attributeValue) =>
            GetAttributeValue(attributeName) == attributeValue;

        private void LoadFromStub()
        {
            var signingId = _chaincodeStub.Creator;

            Mspid = signingId.Mspid;

            var idBytes = signingId.IdBytes.ToByteArray();
            var normalizedCertficate = NormalizeCertificate(Encoding.UTF8.GetString(idBytes));
            X509Certificate = new X509Certificate2(Encoding.UTF8.GetBytes(normalizedCertficate));

            var extension = X509Certificate?.Extensions[FabricCertAttrOid];

            if (extension != null)
            {
                var attributesJsonString = Encoding.UTF8.GetString(extension.RawData);
                var attributeObject = JsonConvert.DeserializeObject<JObject>(attributesJsonString);

                if (attributeObject.ContainsKey("attrs"))
                {
                    Attributes = attributeObject.Value<JObject>("attrs")
                        .Properties()
                        .ToDictionary(token => token.Name, token => token.Value.ToString());
                }
            }

            Id = $"x509::/{X509Certificate.Subject}::/{X509Certificate.Issuer}";
        }

        private string NormalizeCertificate(string raw)
        {
            var matches = Regex.Match(raw,
                @"(\-\-\-\-\-\s*BEGIN ?[^-]+?\-\-\-\-\-)([\s\S]*)(\-\-\-\-\-\s*END ?[^-]+?\-\-\-\-\-)");

            if (!matches.Success || matches.Groups.Count != 4)
            {
                throw new Exception("Failed to find start line or end line of the certificate.");
            }

            var trimmedMatches = new string[3];

            for (var i = 1; i < matches.Groups.Count; i++)
            {
                trimmedMatches[i - 1] = matches.Groups[i].Value.Trim();
            }

            return $"{string.Join("\n", trimmedMatches)}\n";
        }
    }
}