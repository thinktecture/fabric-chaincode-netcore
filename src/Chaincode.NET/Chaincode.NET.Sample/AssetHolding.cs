using System;
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

        private readonly ChaincodeInvocationMap _invocationMap;

        public AssetHolding(ILogger<AssetHolding> logger)
        {
            _logger = logger;
            _invocationMap = new ChaincodeInvocationMap()
            {
                {"invoke", InternalInvoke},
                {"query", InternalQuery}
            };
        }

        public async Task<Response> Init(IChaincodeStub stub)
        {
            _logger.LogInformation("=================== Example Init ===================");

            var functionAndParameters = stub.GetFunctionAndParameters();

            var args = functionAndParameters.Parameters;

            args.AssertCount(4);

            if (!args.TryGet<int>(1, out var aValue) || 
                !args.TryGet<int>(3, out var bValue))
            {
                return Shim.Error("Expecting integer value for asset holding");
            }

          

            if (await stub.PutState("a", aValue) && await stub.PutState("b", bValue))
            {
                return Shim.Success();
            }

            return Shim.Error("Error during Chaincode init!");
        }

        public Task<Response> Invoke(IChaincodeStub stub)
        {
            _logger.LogInformation("=================== Example Invoke ===================");
            return _invocationMap.Invoke(stub);
        }

        private async Task<ByteString> InternalQuery(IChaincodeStub stub, Parameters args)
        {
            args.AssertCount(1);

            var a = args[0];

            var aValueBytes = await stub.GetState(a);

            if (aValueBytes == null)
            {
                throw new Exception($"Failed to get state of asset holder {a}");
            }

            _logger.LogInformation($"Query Response: name={a}, value={aValueBytes.ToStringUtf8()}");
            return aValueBytes;
        }

        private async Task<ByteString> InternalInvoke(IChaincodeStub stub, Parameters args)
        {
            args.AssertCount(3);

            var a = args.Get<string>(0);
            var b = args.Get<string>(1);

            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            {
                throw new Exception("Asset holding must not be empty");
            }

            var aValue = await stub.TryGetState<int>(a);
            if (!aValue.HasValue)
            {
                throw new Exception("Failed to get state of asset holder A");
            }

            var bValue = await stub.TryGetState<int>(b);
            if (!bValue.HasValue)
            {
                throw new Exception("Failed to get state of asset holder B");
            }

            if (!args.TryGet<int>(2, out var amount))
            {
                throw new Exception("Expecting integer value for amount to be transferred");
            }

            aValue -= amount;
            bValue += amount;

            _logger.LogInformation($"aValue = {aValue}, bValue = {bValue}");

            await stub.PutState(a, aValue);
            await stub.PutState(b, bValue);

            return ByteString.Empty;
        }
    }
}
