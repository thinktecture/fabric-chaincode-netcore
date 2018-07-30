using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Chaincode.NET.Handler;
using Microsoft.Extensions.Logging;
using Protos;

namespace Chaincode.NET.Messaging
{
    public class MessageQueue : IMessageQueue
    {
        private readonly IHandler _handler;
        private readonly ILogger<MessageQueue> _logger;

        private readonly ConcurrentDictionary<string, ConcurrentQueue<QueueMessage>> _txQueues =
            new ConcurrentDictionary<string, ConcurrentQueue<QueueMessage>>();

        public MessageQueue(IHandler handler, ILogger<MessageQueue> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        public Task QueueMessage(QueueMessage queueMessage)
        {
            var messageQueue = _txQueues.GetOrAdd(queueMessage.MessageTxContextId, new ConcurrentQueue<QueueMessage>());

            messageQueue.Enqueue(queueMessage);

            if (messageQueue.Count == 1)
            {
                return SendMessage(queueMessage.MessageTxContextId);
            }

            return Task.CompletedTask;
        }

        private Task SendMessage(string messageTxContextId)
        {
            var message = GetCurrentMessage(messageTxContextId);

            if (message == null) return Task.CompletedTask;

            try
            {
                return _handler.WriteStream.WriteAsync(message.Message);
            }
            catch (Exception ex)
            {
                message.Fail(ex);
                return Task.CompletedTask;
            }
        }

        private QueueMessage GetCurrentMessage(string messageTxContextId)
        {
            if (_txQueues.TryGetValue(messageTxContextId, out var messageQueue) &&
                messageQueue.TryPeek(out var message))
            {
                return message;
            }

            _logger.LogError($"Failed to find a message for transaction context id {messageTxContextId}");
            return null;
        }

        private void RemoveCurrentAndSendNextMessage(string messageTxContextId)
        {
            if (!_txQueues.TryGetValue(messageTxContextId, out var messageQueue) || messageQueue.Count <= 0) return;

            messageQueue.TryDequeue(out _);

            if (messageQueue.Count == 0)
            {
                _txQueues.TryRemove(messageTxContextId, out _);
                return;
            }

            SendMessage(messageTxContextId);
        }

        public void HandleMessageResponse(ChaincodeMessage response)
        {
            var messageTxContextId = response.ChannelId + response.Txid;

            var message = GetCurrentMessage(messageTxContextId) as dynamic; // TODO: This needs to be done better

            HandleResponseMessage(message, messageTxContextId, response);
        }

        private void HandleResponseMessage<T>(
            QueueMessage<T> message,
            string messageTxContextId,
            ChaincodeMessage response
        )
        {
            try
            {
                var parsedResponse = _handler.ParseResponse(response, message.Method);
                message.Success((T) parsedResponse);
            }
            catch (Exception ex)
            {
                message.Fail(ex);
            }

            RemoveCurrentAndSendNextMessage(messageTxContextId);
        }
    }
}