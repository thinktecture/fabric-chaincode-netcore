using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Thinktecture.HyperledgerFabric.Chaincode.Chaincode
{
    /// <summary>
    /// ClientIdentity represents information about the identity that submitted the
    /// transaction. Chaincodes can use this class to obtain information about the submitting
    /// identity including a unique ID, the MSP (Membership Service Provider) ID, and attributes.
    /// Such information is useful in enforcing access control by the chaincode.
    /// </summary>
    public interface IClientIdentity
    {
        /// <summary>
        /// Returns the ID associated with the invoking identity. This ID
        /// is guaranteed to be unique within the MSP.
        /// The string is the format: "x509::{subject DN}::{issuer DN}".
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Returns the MSP ID of the invoking identity.
        /// </summary>
        string Mspid { get; }

        /// <summary>
        /// Represents an <see cref="X509Certificate"/>.
        /// returns the X509 certificate associated with the invoking identity,
        /// or null if it was not identified by an X509 certificate, for instance if the MSP is
        /// implemented with an alternative to PKI such as <<see href="https://jira.hyperledger.org/browse/FAB-5673" /> 
        /// </summary>
        X509Certificate2 X509Certificate { get; }
        
        /// <summary>
        /// Represents a dictionary of all attributes within this <see cref="IClientIdentity"/>.
        /// In most cases, either <see cref="GetAttributeValue"/> or <see cref="AssertAttributeValue"/> should be used
        /// instead of accessing this dictionary.
        /// </summary>
        IDictionary<string, string> Attributes { get; }

        /// <summary>
        /// Returns the value of the client's attribute named `attrName`.
        /// If the invoking identity possesses the attribute, returns the value of the attribute.
        /// If the invoking identity does not possess the attribute, returns null.
        /// </summary>
        /// <param name="attributeName">Name of the attribute to retrieve the value from the
        /// identity's credentials (such as x.509 certificate for PKI-based MSPs).</param>
        /// <returns>Value of the attribute or null if the invoking identity does not possess the attribute.</returns>
        string GetAttributeValue(string attributeName);

        /// <summary>
        /// verifies that the invoking identity has the attribute named `attrName` with a value of `attrValue`.
        /// </summary>
        /// <param name="attributeName">Name of the attribute to retrieve the value from the
        /// identity's credentials (such as x.509 certificate for PKI-based MSPs).</param>
        /// <param name="attributeValue">Expected value of the attribute</param>
        /// <returns>True, if the invoking identity possesses the attribute and the attribute value matches the
        /// expected value. Otherwise, returns false.</returns>
        bool AssertAttributeValue(string attributeName, string attributeValue);
    }
}
