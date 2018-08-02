using System.Threading.Tasks;
using Newtonsoft.Json;
using Thinktecture.HyperledgerFabric.Chaincode.NET.Chaincode;

namespace Thinktecture.HyperledgerFabric.Chaincode.NET.Extensions
{
    public static class ChaincodeStubExtensions
    {
        public static async Task<T> GetStateJson<T>(this IChaincodeStub stub, string key)
            where T : class
        {
            return JsonConvert.DeserializeObject<T>(await stub.GetState<string>(key));
        }

        public static async Task<T> GetState<T>(this IChaincodeStub stub, string key)
        {
            var stateResult = await stub.GetState(key);
            return stateResult.Convert<T>();
        }

        public static async Task<T?> TryGetState<T>(this IChaincodeStub stub, string key)
            where T : struct
        {
            try
            {
                var stateResult = await stub.GetState(key);
                return stateResult.Convert<T>();
            }
            catch
            {
                return null;
            }
        }

        public static async Task<bool> DeleteState(this IChaincodeStub stub, string key)
        {
            return await stub.DeleteState(key).InvokeSafe();
        }

        public static Task<bool> PutStateJson<T>(this IChaincodeStub stub, string key, T value)
            where T : class
        {
            return stub.PutState(key, JsonConvert.SerializeObject(value));
        }

        public static async Task<bool> PutState<T>(this IChaincodeStub stub, string key, T value)
        {
            return await stub.PutState(key, value.ToString().ToByteString()).InvokeSafe();
        }

        public static async Task<T> GetPrivateData<T>(this IChaincodeStub stub, string collection, string key)
        {
            var stateResult = await stub.GetPrivateData(collection, key);
            return stateResult.Convert<T>();
        }

        public static async Task<bool> DeletePrivateData(this IChaincodeStub stub, string collection, string key)
        {
            return await stub.DeletePrivateData(collection, key).InvokeSafe();
        }

        public static async Task<bool> PutPrivateData<T>(
            this IChaincodeStub stub,
            string collection,
            string key,
            T value
        )
        {
            return await stub.PutPrivateData(collection, key, value.ToString().ToByteString()).InvokeSafe();
        }
    }
}