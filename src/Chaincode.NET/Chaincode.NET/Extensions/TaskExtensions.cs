using System.Threading.Tasks;

namespace Chaincode.NET.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<bool> InvokeSafe(this Task task)
        {
            try
            {
                await task;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}