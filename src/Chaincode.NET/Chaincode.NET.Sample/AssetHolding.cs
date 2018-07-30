using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chaincode.NET.Chaincode;
using Chaincode.NET.Extensions;
using Chaincode.NET.Handler;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Protos;

namespace Chaincode.NET.Sample
{
    public class AssetHolding : IChaincode
    {
        private readonly ILogger<AssetHolding> _logger;

        public AssetHolding(ILogger<AssetHolding> logger)
        {
            _logger = logger;
        }

        public async Task<Response> Init(IChaincodeStub stub)
        {
            _logger.LogInformation("=================== Example Init ===================");

            var functionAndParameters = stub.GetFunctionAndParameters();

            var args = functionAndParameters.Parameters;

            if (args.Count != 4)
            {
                return Shim.Error("Incorrect number of arguments, expecting 4");
            }

            if (!int.TryParse(args[1], out var aValue) || !int.TryParse(args[3], out var bValue))
            {
                return Shim.Error("Expecting integer value for asset holding");
            }

            try
            {
                await stub.PutState("a", aValue.ToString().ToByteString()); // TODO: Better conversion stuff
                await stub.PutState("b", bValue.ToString().ToByteString());
                return Shim.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Chaincode init");
                return Shim.Error(ex.ToString());
            }
        }

        public async Task<Response> Invoke(IChaincodeStub stub)
        {
            _logger.LogInformation("=================== Example Invoke ===================");

            var functionAndParameters = stub.GetFunctionAndParameters();

            try
            {
                ByteString payload = null;

                if (functionAndParameters.Function == "invoke")
                {
                    payload = await InternalInvoke(stub, functionAndParameters.Parameters);
                }

                if (functionAndParameters.Function == "query")
                {
                    payload = await InternalQuery(stub, functionAndParameters.Parameters);
                }

                if (payload == null)
                {
                    return Shim.Error($"Chaincode invoked with unknown method name: {functionAndParameters.Function}");
                }

                return Shim.Success(payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Chaincode invocation");
                return Shim.Error(ex.ToString());
            }
        }

        private async Task<ByteString> InternalQuery(IChaincodeStub stub, IList<string> args)
        {
            if (args.Count != 1)
            {
                throw new Exception("Incorrect number of arguments. Expecting 1");
            }

            var a = args[0];

            var aValueBytes = await stub.GetState(a);

            if (aValueBytes == null)
            {
                throw new Exception("Failed to get state of asset holder A");
            }

            _logger.LogInformation($"Query Response: name={a}, value={aValueBytes.ToStringUtf8()}");
            return aValueBytes;
        }

        private async Task<ByteString> InternalInvoke(IChaincodeStub stub, IList<string> args)
        {
            if (args.Count != 3)
            {
                throw new Exception("Incorrect number of arguments. Expecting 3");
            }

            var a = args[0];
            var b = args[1];

            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            {
                throw new Exception("Asset holding must not be empty");
            }

            var aValueBytes = await stub.GetState(a);
            if (aValueBytes == null)
            {
                throw new Exception("Failed to get state of asset holder A");
            }

            var aValue = int.Parse(aValueBytes.ToStringUtf8());


            var bValueBytes = await stub.GetState(b);
            if (bValueBytes == null)
            {
                throw new Exception("Failed to get state of asset holder B");
            }

            var bValue = int.Parse(bValueBytes.ToStringUtf8());

            if (!int.TryParse(args[2], out var amount))
            {
                throw new Exception("Expecting integer value for amount to be transferred");
            }

            aValue -= amount;
            bValue += amount;

            _logger.LogInformation($"aValue = {aValue}, bValue = {bValue}");

            await stub.PutState(a, aValue.ToString().ToByteString());
            await stub.PutState(b, bValue.ToString().ToByteString());

            return ByteString.Empty;
        }
    }
}
