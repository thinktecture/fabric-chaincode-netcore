using System.Threading.Tasks;
using Protos;

namespace Thinktecture.HyperledgerFabric.Chaincode.Messaging
{
    /// <summary>
    /// This intefface handles queuing messages to be sent to the peer based on transaction id
    /// The peer can access requests coming from different transactions concurrently but
    /// cannot handle concurrent requests for the same transaction. Given the nature of async
    /// programming this could present a problem so this implementation provides a way to allow
    /// code to perform concurrent request by serialising the calls to the peer.
    /// </summary>
    public interface IMessageQueue
    {
        /// <summary>
        /// Queue a message to be sent to the peer. If it is the first
        /// message on the queue then send the message to the peer
        /// </summary>
        /// <param name="queueMessage">The message to queue.</param>
        /// <returns>A task which is completed when the message has got a reply.</returns>
        Task QueueMessage(QueueMessage queueMessage);

        /// <summary>
        /// Handle a response to a message. this looks at the top of
        /// the queue for the specific txn id to get the message this
        /// response is associated with so it can drive the promise waiting
        /// on this message response. it then removes that message from the
        /// queue and sends the next message on the queue if there is one.
        /// </summary>
        /// <param name="response">The received response.</param>
        void HandleMessageResponse(ChaincodeMessage response);
    }
}
