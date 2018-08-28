using System.Text;

namespace Thinktecture.HyperledgerFabric.Chaincode.Extensions
{
    public static class ByteArrayExtensions
    {
        // https://stackoverflow.com/a/311179/959687
        /// <summary>
        /// Converts a byte array to an lower cased hex string.
        /// </summary>
        /// <param name="byteArray">The byte array to convert.</param>
        /// <returns>Lower cased hex string.</returns>
        public static string ByteArrayToHexString(this byte[] byteArray)
        {
            var hex = new StringBuilder(byteArray.Length * 2);
            foreach (var b in byteArray)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}
