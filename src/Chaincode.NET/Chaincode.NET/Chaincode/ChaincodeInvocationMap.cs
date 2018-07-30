using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chaincode.NET.Handler;
using Google.Protobuf;
using Protos;

namespace Chaincode.NET.Chaincode
{
    public delegate Task<ByteString> ChaincodeInvocationDelegate(IChaincodeStub stub, Parameters parameters);

    public class ChaincodeInvocationMap : Dictionary<string, ChaincodeInvocationDelegate>
    {
        public async Task<Response> Invoke(IChaincodeStub stub)
        {
            var functionParameterInformation = stub.GetFunctionAndParameters();

            if (!ContainsKey(functionParameterInformation.Function))
            {
                return Shim.Error(
                    $"Chaincode invoked with unknown method name: {functionParameterInformation.Function}");
            }

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
}
