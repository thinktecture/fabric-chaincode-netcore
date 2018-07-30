using System.Threading.Tasks;
using Protos;

namespace Chaincode.NET.Chaincode
{
    public interface IChaincode
    {
        Task<Response> Init(IChaincodeStub stub);
        Task<Response> Invoke(IChaincodeStub stub);
    }
}