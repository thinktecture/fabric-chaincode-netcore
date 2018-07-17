using System;
using System.Threading.Tasks;
using Protos;

namespace Chaincode.NET.Messaging
{
    public class QueueMessage
    {
        public QueueMessage(ChaincodeMessage message, MessageMethod method)
        {
            Message = message;
            Method = method;
        }

        public ChaincodeMessage Message { get; }
        public string MessageTxContextId => Message.ChannelId + Message.Txid;
        public MessageMethod Method { get; }

        public virtual void Fail(Exception exception)
        {
            throw exception;
        }
    }

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

        public void Success(T result)
        {
            _taskCompletionSource.SetResult(result);
        }

        public override void Fail(Exception exception)
        {
            _taskCompletionSource.SetException(exception);
        }
    }
}