using System.Threading.Tasks;
using Newtonsoft.Json;
using Thinktecture.HyperledgerFabric.Chaincode.Chaincode;

namespace Thinktecture.HyperledgerFabric.Chaincode.Extensions
{
    /// <summary>
    /// Contains methods for an easier usage of the low level API.
    /// </summary>
    public static class ChaincodeStubExtensions
    {
        /// <summary>
        /// Uses <see cref="ChaincodeStub.GetState"/> and converts the result into <see cref="T"/>
        /// See <see cref="GetState{T}"/> if you need to convert structs.
        /// Will throw, if no data is available.
        /// </summary>
        /// <param name="stub">The <see cref="IChaincodeStub"/> to extend.</param>
        /// <param name="key">State variable key to retrieve from the state store.</param>
        /// <typeparam name="T">The resulting object type.</typeparam>
        /// <returns>The converted object.</returns>
        public static async Task<T> GetStateJson<T>(this IChaincodeStub stub, string key)
            where T : class
        {
            return JsonConvert.DeserializeObject<T>(await stub.GetState<string>(key));
        }

        /// <summary>
        /// Uses <see cref="ChaincodeStub.GetState"/> and converts the result into <see cref="T"/>
        /// See <see cref="GetStateJson{T}"/> if you need to convert objects.
        /// Will throw, if no data is available. See <see cref="TryGetState{T}"/> if you don't want it to throw.
        /// </summary>
        /// <param name="stub">The <see cref="IChaincodeStub"/> to extend.</param>
        /// <param name="key">State variable key to retrieve from the state store.</param>
        /// <typeparam name="T">The resulting object type.</typeparam>
        /// <returns>The converted object.</returns>
        public static async Task<T> GetState<T>(this IChaincodeStub stub, string key)
        {
            var stateResult = await stub.GetState(key);
            return stateResult.Convert<T>();
        }

        /// <summary>
        /// Uses <see cref="ChaincodeStub.GetState"/> and tries to convert the result into <see cref="T"/>
        /// See <see cref="GetStateJson{T}"/> if you need to convert objects.
        /// </summary>
        /// <param name="stub">The <see cref="IChaincodeStub"/> to extend.</param>
        /// <param name="key">State variable key to retrieve from the state store.</param>
        /// <typeparam name="T">The resulting object type.</typeparam>
        /// <returns>The converted object, or null if the conversation was not successful.</returns>
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

        /// <summary>
        /// Uses <see cref="ChaincodeStub.DeleteState"/> to delete <paramref name="key"/> from state store.
        /// </summary>
        /// <param name="stub">The <see cref="IChaincodeStub"/> to extend.</param>
        /// <param name="key">State variable key to retrieve from the state store.</param>
        /// <returns>True, if the deletion was successful, otherwise false.</returns>
        public static async Task<bool> DeleteState(this IChaincodeStub stub, string key)
        {
            return await stub.DeleteState(key).InvokeSafe();
        }

        /// <summary>
        /// Uses <see cref="ChaincodeStub.PutState"/> and converts <paramref name="value"/> to a JsonString
        /// before putting it into the state store.
        /// See <see cref="PutState{T}"/> if you need to convert structs.
        /// </summary>
        /// <param name="stub">The <see cref="IChaincodeStub"/> to extend.</param>
        /// <param name="key">State variable key to retrieve from the state store.</param>
        /// <param name="value">The value to put into the state store.</param>
        /// <returns>True, if the data was accepted, otherwise false.</returns>
        public static Task<bool> PutStateJson<T>(this IChaincodeStub stub, string key, T value)
            where T : class
        {
            return stub.PutState(key, JsonConvert.SerializeObject(value));
        }

        /// <summary>
        /// Uses <see cref="ChaincodeStub.PutState"/> and converts <paramref name="value"/> to a ByteString.
        /// before putting it into the state store.
        /// See <see cref="PutStateJson{T}"/> if you need to convert objects.
        /// </summary>
        /// <param name="stub">The <see cref="IChaincodeStub"/> to extend.</param>
        /// <param name="key">State variable key to retrieve from the state store.</param>
        /// <param name="value">The value to put into the state store.</param>
        /// <returns>True, if the data was accepted, otherwise false.</returns>
        public static async Task<bool> PutState<T>(this IChaincodeStub stub, string key, T value)
        {
            return await stub.PutState(key, value.ToString().ToByteString()).InvokeSafe();
        }

        /// <summary>
        /// Uses <see cref="ChaincodeStub.GetPrivateData"/> and converts the result into <see cref="T"/>
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="stub">The <see cref="IChaincodeStub"/> to extend.</param>
        /// <param name="key">State variable key to retrieve from the state store.</param>
        /// <typeparam name="T">The resulting object type.</typeparam>
        /// <returns>The converted object.</returns>
        public static async Task<T> GetPrivateData<T>(this IChaincodeStub stub, string collection, string key)
        {
            var stateResult = await stub.GetPrivateData(collection, key);
            return stateResult.Convert<T>();
        }

        /// <summary>
        /// Uses <see cref="ChaincodeStub.DeletePrivateData"/> to delete data from the state store.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="stub">The <see cref="IChaincodeStub"/> to extend.</param>
        /// <param name="key">State variable key to retrieve from the state store.</param>
        /// <returns>True, if the deletion was successful, otherwise false.</returns>
        public static async Task<bool> DeletePrivateData(this IChaincodeStub stub, string collection, string key)
        {
            return await stub.DeletePrivateData(collection, key).InvokeSafe();
        }

        /// <summary>
        /// Uses <see cref="ChaincodeStub.PutPrivateData"/> and converts <paramref name="value"/> to a ByteString.
        /// before putting it into the state store.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="stub">The <see cref="IChaincodeStub"/> to extend.</param>
        /// <param name="key">State variable key to retrieve from the state store.</param>
        /// <param name="value">The value to put into the state store.</param>
        /// <returns>True, if the data was accepted, otherwise false.</returns>
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
