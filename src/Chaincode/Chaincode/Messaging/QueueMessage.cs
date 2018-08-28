using System;
using System.Threading.Tasks;
using Protos;

namespace Thinktecture.HyperledgerFabric.Chaincode.Messaging
{
    /// <summary>
    /// A class representing a queued message which is awaiting a response.
    /// </summary>
    public class QueueMessage
    {
        public QueueMessage(ChaincodeMessage message, MessageMethod method)
        {
            Message = message;
            Method = method;
        }

        /// <summary>
        /// The message queued.
        /// </summary>
        public ChaincodeMessage Message { get; }
        
        /// <summary>
        /// The context id of the message which is unique.
        /// </summary>
        public string MessageTxContextId => Message.ChannelId + Message.Txid;
        
        /// <summary>
        /// The method of the message.
        /// </summary>
        public MessageMethod Method { get; }

        /// <summary>
        /// Will be executed when an error response was received.
        /// </summary>
        /// <param name="exception">The exception of the error response.</param>
        /// <exception cref="Exception"></exception>
        public virtual void Fail(Exception exception)
        {
            throw exception;
        }
    }

    /// <inheritdoc />
    /// <typeparam name="T">Inner data type of the queued message</typeparam>
    public class QueueMessage<T> : QueueMessage
    {
        private readonly TaskCompletionSource<T> _taskCompletionSource;

        public QueueMessage(
            ChaincodeMessage message,
            MessageMethod method,
            TaskCompletionSource<T> taskCompletionSource
        )
            : base(message, method)
        {
            _taskCompletionSource = taskCompletionSource;
        }

        /// <summary>
        /// Sets the result of the internal task, so it can complete.
        /// </summary>
        /// <param name="result">The result to set.</param>
        public void Success(T result)
        {
            _taskCompletionSource.SetResult(result);
        }

        /// <summary>
        /// Sets an exception of the internal task, so it will fail.
        /// </summary>
        /// <param name="exception">The exception to set.</param>
        public override void Fail(Exception exception)
        {
            _taskCompletionSource.SetException(exception);
        }
    }
}
