using System.Threading.Tasks;

namespace Thinktecture.HyperledgerFabric.Chaincode.Extensions
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