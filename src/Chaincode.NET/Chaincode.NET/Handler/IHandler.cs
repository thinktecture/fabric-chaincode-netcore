using System.Threading.Tasks;
using Chaincode.NET.Messaging;
using Google.Protobuf;
using Grpc.Core;
using Protos;

namespace Chaincode.NET.Handler
{
    public interface IHandler
    {
        void Close();
        Task Chat(ChaincodeMessage conversationStarterMessage);
        IClientStreamWriter<ChaincodeMessage> WriteStream { get; }
        object ParseResponse(ChaincodeMessage response, MessageMethod messageMethod);
        Task<ByteString> HandleGetState(string collection, string key, string channelId, string txId);
        Task<ByteString> HandlePutState(string collection, string key, ByteString value, string channelId, string txId);
        Task<ByteString> HandleDeleteState(string collection, string key, string channelId, string txId);
        Task HandleGetStateByRange(string collection, string startKey, string endKey, string channelId, string txId);
        Task<QueryResponse> HandleQueryStateNext(string id, string channelId, string txId);
        Task<QueryResponse> HandleQueryCloseState(string id, string channelId, string txId);
        Task HandleGetQueryResult(string collection, string query, string channelId, string txId);
        Task HandleGetHistoryForKey(string key, string channelId, string txId);
        Task<Response> HandleInvokeChaincode(string chaincodeName, byte[][] args, string channelId, string txId);
    }
}
