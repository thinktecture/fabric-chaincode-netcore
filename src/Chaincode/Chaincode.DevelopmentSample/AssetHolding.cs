using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Thinktecture.HyperledgerFabric.Chaincode.Contract;
using Thinktecture.HyperledgerFabric.Chaincode.Extensions;

namespace Thinktecture.HyperledgerFabric.Chaincode.DevelopmentSample
{
    public class AssetHolding : ContractBase
    {
        private readonly ILogger<AssetHolding> _logger;
        
        public AssetHolding(ILogger<AssetHolding> logger)
            : base("AssetHolding")
        {
            _logger = logger;
        }

        /*public async Task<Response> Init(IChaincodeStub stub)
        {
            _logger.LogInformation("=================== Example Init ===================");

            var functionAndParameters = stub.GetFunctionAndParameters();

            var args = functionAndParameters.Parameters;

            args.AssertCount(4);

            if (!args.TryGet<int>(1, out var aValue) ||
                !args.TryGet<int>(3, out var bValue))
                return Shim.Error("Expecting integer value for asset holding");


            if (await stub.PutState("a", aValue) && await stub.PutState("b", bValue)) return Shim.Success();

            return Shim.Error("Error during Chaincode init!");
        }*/

        public async Task<ByteString> Query(IContractContext context, string account)
        {
            var accountValueBytes = await context.Stub.GetState(account);

            if (accountValueBytes == null) throw new Exception($"Failed to get state of asset holder {account}");

            _logger.LogInformation($"Query Response: name={account}, value={accountValueBytes.ToStringUtf8()}");
            return accountValueBytes;            
        }

        public async Task<ByteString> Invoke(IContractContext context, string firstAccount, string secondAccount, string amountString)
        {
            if (string.IsNullOrEmpty(firstAccount) || string.IsNullOrEmpty(secondAccount))
                throw new Exception("Asset holding must not be empty");

            var aValue = await context.Stub.TryGetState<int>(firstAccount);
            if (!aValue.HasValue) throw new Exception("Failed to get state of asset holder A");

            var bValue = await context.Stub.TryGetState<int>(secondAccount);
            if (!bValue.HasValue) throw new Exception("Failed to get state of asset holder B");

            if (!int.TryParse(amountString, out var amount))
                throw new Exception("Expecting integer value for amount to be transferred");

            aValue -= amount;
            bValue += amount;

            _logger.LogInformation($"aValue = {aValue}, bValue = {bValue}");

            await context.Stub.PutState(firstAccount, aValue);
            await context.Stub.PutState(secondAccount, bValue);

            return ByteString.Empty;
        }
    }
}
