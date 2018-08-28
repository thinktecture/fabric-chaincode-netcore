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

        // https://stackoverflow.com/a/311179/959687
        /// <summary>
        /// Converts a hex string back to its byte array representation.
        /// </summary>
        /// <param name="hex">The hex string to convert.</param>
        /// <returns>A byte array representing the converted the string.</returns>
        public static byte[] StringToByteArray(this string hex)
        {
            var numberChars = hex.Length;
            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}
