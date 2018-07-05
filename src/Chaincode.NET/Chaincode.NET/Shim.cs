using Microsoft.Extensions.Logging;

namespace Chaincode.NET
{
    public class Shim
    {
        private readonly ILogger<Shim> _logger;

        public Shim(IChaincode chaincode, ILogger<Shim> logger)
        {
            _logger = logger;
            
            logger.LogInformation($"Instantiating shim with chaincode of type {chaincode.GetType().Name}");
        }
        
        public void Start()
        {
        }
    }
}
