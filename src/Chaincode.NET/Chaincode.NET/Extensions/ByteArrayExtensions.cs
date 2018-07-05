using System.Text;

namespace Chaincode.NET.Extensions
{
    public static class ByteArrayExtensions
    {
        // https://stackoverflow.com/a/311179/959687
        public static string ByteArrayToString(this byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (var b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}