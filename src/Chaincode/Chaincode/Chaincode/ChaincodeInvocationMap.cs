using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Protos;
using Thinktecture.HyperledgerFabric.Chaincode.Handler;

namespace Thinktecture.HyperledgerFabric.Chaincode.Chaincode
{
    public delegate Task<ByteString> ChaincodeInvocationDelegate(IChaincodeStub stub, Parameters parameters);

    public class ChaincodeInvocationMap : Dictionary<string, ChaincodeInvocationDelegate>
    {
        public ChaincodeInvocationMap()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public virtual async Task<Response> Invoke(IChaincodeStub stub)
        {
            var functionParameterInformation = stub.GetFunctionAndParameters();

            if (!ContainsKey(functionParameterInformation.Function))
                return Shim.Error(
                    $"Chaincode invoked with unknown method name: {functionParameterInformation.Function}");

            try
            {
                return Shim.Success(
                    await this[functionParameterInformation.Function](stub, functionParameterInformation.Parameters)
                );
            }
            catch (Exception ex)
            {
                return Shim.Error(ex);
            }
        }
    }

    public class LoggingChaincodeInvocationMap : ChaincodeInvocationMap
    {
        private readonly ILogger<IChaincode> _logger;

        public LoggingChaincodeInvocationMap(ILogger<IChaincode> logger)
        {
            _logger = logger;
        }

        public override async Task<Response> Invoke(IChaincodeStub stub)
        {
            var function = stub.GetFunctionAndParameters().Function;

            _logger.LogInformation($"========== START: {function} ==========");
            var result = await base.Invoke(stub);
            _logger.LogInformation($"========== END: {function} ==========");

            return result;
        }
    }
}
