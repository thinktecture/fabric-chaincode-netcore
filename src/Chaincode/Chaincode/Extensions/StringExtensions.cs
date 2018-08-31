using System;
using System.Text;
using Google.Protobuf;

namespace Thinktecture.HyperledgerFabric.Chaincode.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a string to a <see cref="ByteString"/>
        /// </summary>
        /// <param name="input">The string to convert.</param>
        /// <returns>A <see cref="ByteString"/> representing the input.</returns>
        public static ByteString ToByteString(this string input)
        {
            var base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
            return ByteString.FromBase64(base64String);
        }
    }
}
