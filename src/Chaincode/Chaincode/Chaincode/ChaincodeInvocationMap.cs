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

    /// <summary>
    /// This class provides an easy way to create a mapping in your <see cref="IChaincode"/> implementation to
    /// map string function identifiers to actual methods. It also handles the response of the method being called
    /// and sends the correct response back to HLF.
    /// An error response (created by <see cref="Shim.Error(string)"/> will be sent back, if:
    ///
    /// <list type="bullet">
    ///    <item>
    ///        <description>the function being invoked is not found within the mapping.</description>
    ///    </item>
    ///    <item>
    ///        <description>the function being invoked throws an exception</description>
    ///    </item>
    /// </list>
    ///
    /// Otherwise, a success message (created by <see cref="Shim.Success(ByteString)"/> will be sent back.
    /// </summary>
    public class ChaincodeInvocationMap : Dictionary<string, ChaincodeInvocationDelegate>
    {
        /// <summary>
        /// Invokes the actual chaincode function determined by the first argument within
        /// <see cref="IChaincodeStub.Args"/>
        /// </summary>
        /// <param name="stub">The stub used for invocation</param>
        /// <returns>A response to be sent back to HLF.</returns>
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

    /// <summary>
    /// In addition to <see cref="ChaincodeInvocationMap"/> this class will also log the start and end of an invocation.
    /// </summary>
    /// <seealso cref="ChaincodeInvocationMap"/>
    public class LoggingChaincodeInvocationMap : ChaincodeInvocationMap
    {
        private readonly ILogger<IChaincode> _logger;

        public LoggingChaincodeInvocationMap(ILogger<IChaincode> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
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
