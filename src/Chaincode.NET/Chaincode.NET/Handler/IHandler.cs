using System.Collections.Generic;
using System.Threading.Tasks;
using Chaincode.NET.Handler.Iterators;
using Chaincode.NET.Messaging;
using Google.Protobuf;
using Grpc.Core;
using Protos;

namespace Chaincode.NET.Handler
{
    public interface IHandler
    {
        IClientStreamWriter<ChaincodeMessage> WriteStream { get; }

        States State { get; set; } // For testing only, will be removed with the impl. of a message handler
        void Close();
        Task Chat(ChaincodeMessage conversationStarterMessage);
        object ParseResponse(ChaincodeMessage response, MessageMethod messageMethod);
        Task<ByteString> HandleGetState(string collection, string key, string channelId, string txId);
        Task<ByteString> HandlePutState(string collection, string key, ByteString value, string channelId, string txId);
        Task<ByteString> HandleDeleteState(string collection, string key, string channelId, string txId);

        Task<StateQueryIterator> HandleGetStateByRange(
            string collection,
            string startKey,
            string endKey,
            string channelId,
            string txId
        );

        Task<QueryResponse> HandleQueryStateNext(string id, string channelId, string txId);
        Task<QueryResponse> HandleQueryCloseState(string id, string channelId, string txId);
        Task<StateQueryIterator> HandleGetQueryResult(string collection, string query, string channelId, string txId);
        Task<HistoryQueryIterator> HandleGetHistoryForKey(string key, string channelId, string txId);

        Task<Response> HandleInvokeChaincode(
            string chaincodeName,
            IEnumerable<ByteString> args,
            string channelId,
            string txId
        );
    }
}