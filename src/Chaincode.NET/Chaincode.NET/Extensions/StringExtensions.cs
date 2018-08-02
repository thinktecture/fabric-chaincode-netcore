using System;
using System.Text;
using Google.Protobuf;

namespace Thinktecture.HyperledgerFabric.Chaincode.NET.Extensions
{
    public static class StringExtensions
    {
        public static ByteString ToByteString(this string input)
        {
            var base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
            return ByteString.FromBase64(base64String);
        }

        // https://stackoverflow.com/a/311179/959687
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