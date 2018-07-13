using System;
using System.Threading.Tasks;
using Protos;

namespace Chaincode.NET.Messaging
{
    public class QueueMessage
    {
        private readonly ChaincodeMessage _message;
        private readonly MessageMethod _method;
        public Type GenericType { get; }

        public QueueMessage(ChaincodeMessage message, MessageMethod method, Type genericType)
        {
            _message = message;
            _method = method;
            GenericType = genericType;
        }

        public ChaincodeMessage Message => _message;
        public string MessageTxContextId => _message.ChannelId + _message.Txid;
        public MessageMethod Method => _method;

        public virtual void Fail(Exception exception)
        {
            throw exception;
        }
    }
    
    public class QueueMessage<T> : QueueMessage
    {
        private readonly TaskCompletionSource<T> _taskCompletionSource;

        public QueueMessage(ChaincodeMessage message, MessageMethod method, TaskCompletionSource<T> taskCompletionSource)
            : base(message, method, typeof(T))
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
