using System.Threading.Tasks;
using Chaincode.NET.Protos;

namespace Thinktecture.HyperledgerFabric.Chaincode.NET.Chaincode
{
    public interface IChaincode
    {
        Task<Response> Init(IChaincodeStub stub);
        Task<Response> Invoke(IChaincodeStub stub);
    }
}