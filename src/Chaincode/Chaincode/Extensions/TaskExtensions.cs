using System.Threading.Tasks;

namespace Thinktecture.HyperledgerFabric.Chaincode.Extensions
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Safely invokes a task.
        /// </summary>
        /// <param name="task">The task to invoke.</param>
        /// <returns>Returns true, if the task invocation was successful.</returns>
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
