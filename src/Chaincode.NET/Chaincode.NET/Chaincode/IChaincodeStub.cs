using System.Collections.Generic;
using System.Threading.Tasks;
using Chaincode.NET.Handler.Iterators;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Msp;
using Protos;

namespace Chaincode.NET.Chaincode
{
    public interface IChaincodeStub
    {
        ChaincodeEvent ChaincodeEvent { get; }
        string Binding { get; }
        Timestamp TxTimestamp { get; }
        DecodedSignedProposal DecodedSignedProposal { get; }
        MapField<string, ByteString> TransientMap { get; }
        string ChannelId { get; }
        string TxId { get; }
        IList<string> Args { get; }
        SerializedIdentity Creator { get; }
        ChaincodeFunctionParameterInformation GetFunctionAndParameters();
        Task<ByteString> GetState(string key);
        Task<ByteString> PutState(string key, ByteString value);
        Task<ByteString> DeleteState(string key);
        Task<StateQueryIterator> GetStateByRange(string startKey, string endKey);
        Task<StateQueryIterator> GetQueryResult(string query);
        Task<HistoryQueryIterator> GetHistoryForKey(string key);
        Task InvokeChaincode(string chaincodeName, IEnumerable<ByteString> args, string channel = "");
        void SetEvent(string name, ByteString payload);
        string CreateCompositeKey(string objectType, IEnumerable<string> attributes);
        (string ObjectType, IList<string> Attributes) SplitCompositeKey(string compositeKey);
        Task<StateQueryIterator> GetStateByPartialCompositeKey(string objectType, IList<string> attributes);
        Task<ByteString> GetPrivateData(string collection, string key);
        Task<ByteString> PutPrivateData(string collection, string key, ByteString value);
        Task<ByteString> DeletePrivateData(string collection, string key);
        Task<StateQueryIterator> GetPrivateDataByRange(string collection, string startKey, string endKey);

        Task<StateQueryIterator> GetPrivateDataByPartialCompositeKey(
            string collection,
            string objectType,
            IList<string> attributes
        );

        Task<StateQueryIterator> GetPrivateDataQueryResult(string collection, string query);
    }
}