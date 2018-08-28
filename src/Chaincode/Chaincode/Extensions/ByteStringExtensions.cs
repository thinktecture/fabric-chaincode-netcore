using System.Globalization;
using Google.Protobuf;

namespace Thinktecture.HyperledgerFabric.Chaincode.Extensions
{
    public static class ByteStringExtensions
    {
        /// <summary>
        /// Generic conversation method to convert a <see cref="ByteString"/> to <see cref="T"/>.
        /// </summary>
        /// <param name="byteString">The <see cref="ByteString"/> to convert.</param>
        /// <typeparam name="T">The resulting object type.</typeparam>
        /// <returns>Returns the converted object.</returns>
        public static T Convert<T>(this ByteString byteString)
        {
            var stringValue = byteString.ToStringUtf8();

            if (stringValue is T t) return t;

            return (T) System.Convert.ChangeType(stringValue, typeof(T), CultureInfo.InvariantCulture);
        }
    }
}
