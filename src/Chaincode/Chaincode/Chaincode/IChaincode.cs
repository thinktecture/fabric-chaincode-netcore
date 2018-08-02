using System.Threading.Tasks;
using Protos;

namespace Thinktecture.HyperledgerFabric.Chaincode.Chaincode
{
    public interface IChaincode
    {
        Task<Response> Init(IChaincodeStub stub);
        Task<Response> Invoke(IChaincodeStub stub);
    }
}
