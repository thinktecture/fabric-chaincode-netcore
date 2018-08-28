using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Protos;
using Thinktecture.HyperledgerFabric.Chaincode.Chaincode;
using Thinktecture.HyperledgerFabric.Chaincode.Handler.Iterators;
using Thinktecture.HyperledgerFabric.Chaincode.Messaging;

namespace Thinktecture.HyperledgerFabric.Chaincode.Handler
{
    /// <summary>
    /// The handler represents the base for all remote nodes, peer, orderer and MemberServicesPeer
    /// </summary>
    public interface IHandler
    {
        /// <summary>
        /// The WriteStream to write data back to the connected node.
        /// </summary>
        IClientStreamWriter<ChaincodeMessage> WriteStream { get; }

        States State { get; set; } // For testing only, will be removed with the impl. of a message handler
        
        /// <summary>
        /// Closes the handler, thus gracefully closes the connection.
        /// </summary>
        void Close();
        
        /// <summary>
        /// Starts the communication process.
        /// Careful, this is a long running method. The task will only complete on miserable errors or when the connection
        /// is closed.
        /// </summary>
        /// <param name="conversationStarterMessage"></param>
        /// <returns></returns>
        Task Chat(ChaincodeMessage conversationStarterMessage);
        
        /// <summary>
        /// Parses a response returning the payload, iterator, <see cref="QueryResponse"/> or <see cref="ChaincodeMessage"/>
        /// depending on the <paramref name="messageMethod"/>. 
        /// </summary>
        /// <param name="response">The response to parse.</param>
        /// <param name="messageMethod">The method determining the output type</param>
        /// <returns>payload, iterator, <see cref="QueryResponse"/> or <see cref="ChaincodeMessage"/></returns>
        object ParseResponse(ChaincodeMessage response, MessageMethod messageMethod);
        
        /// <summary>
        /// Handles the result of <see cref="IChaincodeStub.GetState"/>.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="key">State variable key to retrieve from the state store.</param>
        /// <param name="channelId">The chaincode's channel Id</param>
        /// <param name="txId">The transaction id</param>
        /// <returns>The payload of the received response.</returns>
        Task<ByteString> HandleGetState(string collection, string key, string channelId, string txId);
        
        /// <summary>
        /// Handles the result of <see cref="IChaincodeStub.PutState"/>.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="key">State variable key to set in the state store.</param>
        /// <param name="value">The value to put into the state store.</param> 
        /// <param name="channelId">The chaincode's channel Id</param>
        /// <param name="txId">The transaction id</param>
        /// <returns>The payload of the received response.</returns>
        Task<ByteString> HandlePutState(string collection, string key, ByteString value, string channelId, string txId);
        
        /// <summary>
        /// Handles the result of <see cref="IChaincodeStub.DeleteState"/>.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="key">State variable key to deletefrom the state store.</param>
        /// <param name="channelId">The chaincode's channel Id</param>
        /// <param name="txId">The transaction id</param>
        /// <returns>The payload of the received response.</returns>
        Task<ByteString> HandleDeleteState(string collection, string key, string channelId, string txId);

        /// <summary>
        /// Handles the result of <see cref="IChaincodeStub.GetStateByRange"/>.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="startKey">State variable key as the start of the key range (inclusive)</param>
        /// <param name="endKey">State variable key as the end of the key range (exclusive)</param>
        /// <param name="channelId">The chaincode's channel Id</param>
        /// <param name="txId">The transaction id</param>
        /// <returns>A <see cref="StateQueryIterator"/>.</returns>
        Task<StateQueryIterator> HandleGetStateByRange(
            string collection,
            string startKey,
            string endKey,
            string channelId,
            string txId
        );

        /// <summary>
        /// Handles the result of <see cref="CommonIterator{T}.Next"/>.
        /// </summary>
        /// <param name="id">The id of the iteration.</param>
        /// <param name="channelId">The chaincode's channel Id</param>
        /// <param name="txId">The transaction id</param>
        /// <returns>A <see cref="QueryResponse"/>.</returns>
        Task<QueryResponse> HandleQueryStateNext(string id, string channelId, string txId);
        
        /// <summary>
        /// Handles the result of <see cref="CommonIterator{T}.Close"/>.
        /// </summary>
        /// <param name="id">The id of the iteration.</param>
        /// <param name="channelId">The chaincode's channel Id</param>
        /// <param name="txId">The transaction id</param>
        /// <returns>A <see cref="QueryResponse"/>.</returns>
        Task<QueryResponse> HandleQueryCloseState(string id, string channelId, string txId);
        
        /// <summary>
        /// Handles the result of <see cref="IChaincodeStub.GetQueryResult"/>.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="query">Query string native to the underlying state database</param>
        /// <param name="channelId">The chaincode's channel Id</param>
        /// <param name="txId">The transaction id</param>
        /// <returns>A <see cref="QueryResponse"/>.</returns>
        Task<StateQueryIterator> HandleGetQueryResult(string collection, string query, string channelId, string txId);
        
        /// <summary>
        /// Handles the result of <see cref="IChaincodeStub.GetQueryResult"/>.
        /// </summary>
        /// <param name="key">The state variable key</param>
        /// <param name="channelId">The chaincode's channel Id</param>
        /// <param name="txId">The transaction id</param>
        /// <returns>A <see cref="QueryResponse"/>.</returns>
        Task<HistoryQueryIterator> HandleGetHistoryForKey(string key, string channelId, string txId);

        /// <summary>
        /// Handles the result of <see cref="IChaincodeStub.InvokeChaincode"/>.
        /// </summary>
        /// <param name="chaincodeName">The name of the invoked chaincode.</param>
        /// <param name="args">The arguments for the invoked chaincode.</param>
        /// <param name="channelId">The chaincode's channel Id</param>
        /// <param name="txId">The transaction id</param>
        /// <returns>A <see cref="QueryResponse"/>.</returns>
        Task<Response> HandleInvokeChaincode(
            string chaincodeName,
            IEnumerable<ByteString> args,
            string channelId,
            string txId
        );
    }
}
