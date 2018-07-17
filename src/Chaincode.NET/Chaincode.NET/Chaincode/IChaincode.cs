using System.Threading.Tasks;
using Protos;

namespace Chaincode.NET.Chaincode
{
    public interface IChaincode
    {
        Task<Response> Init(ChaincodeStub stub);
        Task<Response> Invoke(ChaincodeStub stub);
    }
}