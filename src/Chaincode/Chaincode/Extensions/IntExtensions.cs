using System.Diagnostics.CodeAnalysis;
using Google.Protobuf;

namespace Thinktecture.HyperledgerFabric.Chaincode.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class IntExtensions
    {
        /// <summary>
        /// Converts an int to an <see cref="ByteString"/>.
        /// </summary>
        /// <param name="value">The int to convert.</param>
        /// <returns>A <see cref="ByteString"/> representing the converted integer</returns>
        public static ByteString ToByteString(this int value)
        {
            return value.ToString().ToByteString();
        }
    }
}
