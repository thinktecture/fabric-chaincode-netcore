using System.Threading.Tasks;
using Chaincode.NET.Chaincode;
using Grpc.Core.Logging;
using Microsoft.Extensions.Logging;
using Protos;

namespace Chaincode.NET.Sample
{
    public class FabCar : IChaincode
    {
        private readonly ILogger<FabCar> _logger;

        public FabCar(ILogger<FabCar> logger)
        {
            _logger = logger;
        }
        
        public Task<Response> Init(ChaincodeStub stub)
        {
            _logger.LogInformation("FabCar Chaincode Init");
            throw new System.NotImplementedException();
        }

        public Task<Response> Invoke(ChaincodeStub stub)
        {
            _logger.LogInformation("FabCar Chaincode Invoke");
            
            throw new System.NotImplementedException();
        }
    }
}
