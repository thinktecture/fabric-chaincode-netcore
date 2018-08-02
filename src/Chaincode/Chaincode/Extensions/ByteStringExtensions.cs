using System.Globalization;
using Google.Protobuf;

namespace Thinktecture.HyperledgerFabric.Chaincode.Extensions
{
    public static class ByteStringExtensions
    {
        public static T Convert<T>(this ByteString byteString)
        {
            var stringValue = byteString.ToStringUtf8();

            if (stringValue is T t) return t;

            return (T) System.Convert.ChangeType(stringValue, typeof(T), CultureInfo.InvariantCulture);
        }
    }
}