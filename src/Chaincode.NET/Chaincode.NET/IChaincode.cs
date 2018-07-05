using System.Threading.Tasks;

namespace Chaincode.NET
{
    public interface IChaincode
    {
        Task Init();
        Task Invoke();
    }
}
