using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chaincode.NET.Protos;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Thinktecture.HyperledgerFabric.Chaincode.NET.Chaincode;
using Thinktecture.HyperledgerFabric.Chaincode.NET.Extensions;
using Thinktecture.HyperledgerFabric.Chaincode.NET.Handler.Iterators;
using Thinktecture.HyperledgerFabric.Chaincode.NET.Messaging;

namespace Thinktecture.HyperledgerFabric.Chaincode.NET.Handler
{
    public class Handler : IHandler
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IChaincode _chaincode;
        private readonly IChaincodeStubFactory _chaincodeStubFactory;
        private readonly ChaincodeSupport.ChaincodeSupportClient _client;
        private readonly ILogger _logger;
        private readonly IMessageQueue _messageQueue;
        private AsyncDuplexStreamingCall<ChaincodeMessage, ChaincodeMessage> _stream;

        public Handler(
            IChaincode chaincode,
            string host,
            int port,
            IChaincodeStubFactory chaincodeStubFactory,
            ILogger<Handler> logger,
            IMessageQueueFactory messageQueueFactory,
            IChaincodeSupportClientFactory chaincodeSupportClientFactory
        )
        {
            _chaincode = chaincode;
            _chaincodeStubFactory = chaincodeStubFactory;
            _logger = logger;

            // TODO: Secure channel?
            _client = chaincodeSupportClientFactory.Create(new Channel(host, port, ChannelCredentials.Insecure,
                new List<ChannelOption>
                {
                    new ChannelOption("request-timeout", 30000)
                }));
            _messageQueue = messageQueueFactory.Create(this);
        }

        // For testing only, will be removed with the impl. of a message handler
        public States State { get; set; } = States.Created;

        public IClientStreamWriter<ChaincodeMessage> WriteStream => _stream.RequestStream;

        public void Close()
        {
            _cancellationTokenSource.Cancel();
        }

        public async Task Chat(ChaincodeMessage conversationStarterMessage)
        {
            _stream = _client.Register(null, null, _cancellationTokenSource.Token);

            // TODO: Write a message handler

            await _stream.RequestStream.WriteAsync(conversationStarterMessage);

            await Task.Run(async () =>
            {
                while (await _stream.ResponseStream.MoveNext(_cancellationTokenSource.Token))
                {
                    var message = _stream.ResponseStream.Current;

                    _logger.LogDebug($"Received chat message from peer: {message} {State}");

                    if (State == States.Ready)
                    {
                        var type = message.Type;

                        if (type != ChaincodeMessage.Types.Type.Registered && type != ChaincodeMessage.Types.Type.Ready)
                        {
                            if (type == ChaincodeMessage.Types.Type.Response ||
                                type == ChaincodeMessage.Types.Type.Error)
                            {
                                _logger.LogDebug($"[{message.ChannelId}-{message.Txid}] Received {message.Type}, " +
                                                 "handling good or error response");
                                _messageQueue.HandleMessageResponse(message);
                            }
                            else if (type == ChaincodeMessage.Types.Type.Init)
                            {
                                _logger.LogDebug($"[{message.ChannelId}-{message.Txid}], Received {message.Type}, " +
                                                 "initializing chaincode");
#pragma warning disable 4014
                                HandleInit(message);
#pragma warning restore 4014
                            }
                            else if (type == ChaincodeMessage.Types.Type.Transaction)
                            {
                                _logger.LogDebug($"[{message.ChannelId}-{message.Txid}], Received {message.Type}, " +
                                                 $"invoking transaction on chaincode (state: {State})");
#pragma warning disable 4014
                                HandleTransaction(message);
#pragma warning restore 4014
                            }
                            else
                            {
                                _logger.LogCritical("Received unknown message from peer, exiting");
                                _cancellationTokenSource.Cancel();
                            }
                        }
                    }

                    if (State == States.Established)
                    {
                        if (message.Type == ChaincodeMessage.Types.Type.Ready)
                        {
                            _logger.LogInformation(
                                "Successfully established communication with peer node. State transferred to \"ready\"");
                            State = States.Ready;
                        }
                        else
                        {
                            _logger.LogError("Chaincode is in ready, can only process messages of type " +
                                             $"'established', but received {message.Type}");
#pragma warning disable 4014
                            _stream.RequestStream.WriteAsync(NewErrorMessage(message,
#pragma warning restore 4014
                                State));
                        }
                    }

                    if (State == States.Created)
                    {
                        if (message.Type == ChaincodeMessage.Types.Type.Registered)
                        {
                            _logger.LogInformation(
                                "Successfully registered with peer node. State transferred to \"established\"");
                            State = States.Established;
                        }
                        else
                        {
                            _logger.LogError("Chaincode is in \"created\" state, can only process message of type " +
                                             $"\"registered\", but received {message.Type}");
#pragma warning disable 4014
                            _stream.RequestStream.WriteAsync(NewErrorMessage(message,
#pragma warning restore 4014
                                State));
                        }
                    }
                }

                _logger.LogCritical("Chaincode ended??");
            }, _cancellationTokenSource.Token);
        }

        public object ParseResponse(ChaincodeMessage response, MessageMethod messageMethod)
        {
            if (response.Type == ChaincodeMessage.Types.Type.Response)
            {
                _logger.LogInformation(
                    $"[{response.ChannelId}-{response.Txid}] Received {messageMethod} successful response");

                switch (messageMethod)
                {
                    case MessageMethod.GetStateByRange:
                    case MessageMethod.GetQueryResult:
                        return new StateQueryIterator(this, response.ChannelId, response.Txid,
                            QueryResponse.Parser.ParseFrom(response.Payload));

                    case MessageMethod.GetHistoryForKey:
                        return new HistoryQueryIterator(this, response.ChannelId, response.Txid,
                            QueryResponse.Parser.ParseFrom(response.Payload));

                    case MessageMethod.QueryStateNext:
                    case MessageMethod.QueryStateClose:
                        return QueryResponse.Parser.ParseFrom(response.Payload);

                    case MessageMethod.InvokeChaincode:
                        return ChaincodeMessage.Parser.ParseFrom(response.Payload);

                    default:
                        return response.Payload;
                }
            }

            if (response.Type == ChaincodeMessage.Types.Type.Error)
            {
                _logger.LogInformation(
                    $"[{response.ChannelId}-{response.Txid}] Received {messageMethod} error response");
                throw new Exception(response.Payload.ToStringUtf8());
            }

            var errorMessage = $"[{response.ChannelId}-{response.Txid}] Received incorrect chaincode " +
                               $"in response to the {messageMethod} call: " +
                               $"type={response.Type}, expecting \"RESPONSE\"";
            _logger.LogInformation(errorMessage);
            throw new Exception(errorMessage);
        }

        public Task<ByteString> HandleGetState(string collection, string key, string channelId, string txId)
        {
            var payload = new GetState
            {
                Key = key,
                Collection = collection
            };
            return CreateMessageAndListen<ByteString>(MessageMethod.GetState, ChaincodeMessage.Types.Type.GetState,
                payload, channelId, txId);
        }

        public Task<ByteString> HandlePutState(
            string collection,
            string key,
            ByteString value,
            string channelId,
            string txId
        )
        {
            var payload = new PutState
            {
                Key = key,
                Value = value,
                Collection = collection
            };

            return CreateMessageAndListen<ByteString>(MessageMethod.PutState, ChaincodeMessage.Types.Type.PutState,
                payload,
                channelId, txId);
        }

        public Task<ByteString> HandleDeleteState(string collection, string key, string channelId, string txId)
        {
            var payload = new DelState
            {
                Key = key,
                Collection = collection
            };

            return CreateMessageAndListen<ByteString>(MessageMethod.DelState, ChaincodeMessage.Types.Type.DelState,
                payload, channelId, txId);
        }

        public Task<StateQueryIterator> HandleGetStateByRange(
            string collection,
            string startKey,
            string endKey,
            string channelId,
            string txId
        )
        {
            var payload = new GetStateByRange
            {
                StartKey = startKey,
                EndKey = endKey,
                Collection = collection
            };

            return CreateMessageAndListen<StateQueryIterator>(MessageMethod.GetStateByRange,
                ChaincodeMessage.Types.Type.GetStateByRange,
                payload, channelId, txId);
        }

        public Task<QueryResponse> HandleQueryStateNext(string id, string channelId, string txId)
        {
            var payload = new QueryStateNext {Id = id};

            return CreateMessageAndListen<QueryResponse>(MessageMethod.QueryStateNext,
                ChaincodeMessage.Types.Type.QueryStateNext, payload, channelId, txId);
        }

        public Task<QueryResponse> HandleQueryCloseState(string id, string channelId, string txId)
        {
            var payload = new QueryStateClose {Id = id};

            return CreateMessageAndListen<QueryResponse>(MessageMethod.QueryStateClose,
                ChaincodeMessage.Types.Type.QueryStateClose, payload, channelId, txId);
        }

        public Task<StateQueryIterator> HandleGetQueryResult(
            string collection,
            string query,
            string channelId,
            string txId
        )
        {
            var payload = new GetQueryResult
            {
                Query = query,
                Collection = collection
            };

            return CreateMessageAndListen<StateQueryIterator>(MessageMethod.GetQueryResult,
                ChaincodeMessage.Types.Type.GetQueryResult, payload, channelId, txId);
        }

        public Task<HistoryQueryIterator> HandleGetHistoryForKey(string key, string channelId, string txId)
        {
            var payload = new GetHistoryForKey {Key = key};

            return CreateMessageAndListen<HistoryQueryIterator>(MessageMethod.GetHistoryForKey,
                ChaincodeMessage.Types.Type.GetHistoryForKey, payload, channelId, txId);
        }

        public async Task<Response> HandleInvokeChaincode(
            string chaincodeName,
            IEnumerable<ByteString> args,
            string channelId,
            string txId
        )
        {
            var chaincodeId = new ChaincodeID {Name = chaincodeName};
            var inputArgs = new RepeatedField<ByteString>().Concat(args);

            var chaincodeInput = new ChaincodeInput();
            chaincodeInput.Args.AddRange(inputArgs);

            var payload = new ChaincodeSpec
            {
                ChaincodeId = chaincodeId,
                Input = chaincodeInput
            };

            var response = await CreateMessageAndListen<ChaincodeMessage>(MessageMethod.InvokeChaincode,
                ChaincodeMessage.Types.Type.InvokeChaincode, payload, channelId, txId);

            if (response.Type == ChaincodeMessage.Types.Type.Completed)
                return Response.Parser.ParseFrom(response.Payload);

            if (response.Type == ChaincodeMessage.Types.Type.Error)
                throw new Exception(response.Payload.ToStringUtf8());

            throw new Exception("Something went somewhere horribly wrong");
        }

        private Task HandleTransaction(ChaincodeMessage message)
        {
            return HandleMessage(message, HandleMessageAction.Invoke);
        }

        private Task HandleInit(ChaincodeMessage message)
        {
            return HandleMessage(message, HandleMessageAction.Init);
        }

        private async Task HandleMessage(ChaincodeMessage message, HandleMessageAction action)
        {
            ChaincodeMessage nextMessage = null;
            ChaincodeInput input = null;

            try
            {
                input = ChaincodeInput.Parser.ParseFrom(message.Payload);
            }
            catch
            {
                _logger.LogError(
                    $"{message.ChannelId}-{message.Txid} Incorrect payload format. Sending ERROR message back to peer");
                nextMessage = new ChaincodeMessage
                {
                    Txid = message.Txid,
                    ChannelId = message.ChannelId,
                    Type = ChaincodeMessage.Types.Type.Error,
                    Payload = message.Payload
                };
            }

            if (input == null)
            {
                await WriteStream.WriteAsync(nextMessage);
                return;
            }

            IChaincodeStub stub = null;
            try
            {
                stub = _chaincodeStubFactory.Create(this, message.ChannelId, message.Txid, input,
                    message.Proposal);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to construct a chaincode stub instance for the INIT message: {ex}");
                nextMessage = new ChaincodeMessage
                {
                    Type = ChaincodeMessage.Types.Type.Error,
                    Payload = ex.ToString().ToByteString(),
                    Txid = message.Txid,
                    ChannelId = message.ChannelId
                };
            }

            if (stub == null)
            {
                await WriteStream.WriteAsync(nextMessage);
                return;
            }

            Response response;
            if (action == HandleMessageAction.Init)
                response = await _chaincode.Init(stub);
            else
                response = await _chaincode.Invoke(stub);

            if (response.Status == 0)
            {
                var errorMessage = $"[{message.ChannelId}-{message.Txid}] Calling chaincode {action} " +
                                   "has not called success or error";
                _logger.LogError(errorMessage);
                response = Shim.Error(errorMessage);
            }

            _logger.LogInformation($"[{message.ChaincodeEvent}]-{message.Txid} Calling chaincode {action}, " +
                                   $"response status {response.Status}");

            if (response.Status >= (int) ResponseCodes.Error)
            {
                _logger.LogError($"[{message.ChannelId}-{message.Txid}] Calling chaincode {action} " +
                                 $"returned error response {response.Message}. " +
                                 "Sending ERROR message back to peer");

                nextMessage = new ChaincodeMessage
                {
                    Type = ChaincodeMessage.Types.Type.Error,
                    Payload = response.Message.ToByteString(),
                    Txid = message.Txid,
                    ChannelId = message.ChannelId
                };
            }
            else
            {
                _logger.LogInformation($"[{message.ChannelId}-{message.Txid}] Calling chaincode {action} " +
                                       "succeeded. Sending COMPLETED message back to peer");
                nextMessage = new ChaincodeMessage
                {
                    Type = ChaincodeMessage.Types.Type.Completed,
                    Payload = response.ToByteString(),
                    Txid = message.Txid,
                    ChannelId = message.ChannelId,
                    ChaincodeEvent = stub.ChaincodeEvent
                };
            }

            await WriteStream.WriteAsync(nextMessage);
        }

        private ChaincodeMessage NewErrorMessage(ChaincodeMessage message, States state)
        {
            var errorString = $"[{message.ChannelId}-{message.Txid}] Chaincode Handler FSM cannot " +
                              $"handle message ({message.Type}, with payload size {message.Payload.Length} " +
                              $"while in state {state}";

            return new ChaincodeMessage
            {
                Type = ChaincodeMessage.Types.Type.Error,
                Payload = errorString.ToByteString(),
                Txid = message.Txid,
                ChannelId = message.ChannelId
            };
        }

        private ChaincodeMessage CreateMessage(
            ChaincodeMessage.Types.Type type,
            IMessage payload,
            string channelId,
            string txId
        )
        {
            return new ChaincodeMessage
            {
                Type = type,
                Payload = payload.ToByteString(),
                Txid = txId,
                ChannelId = channelId
            };
        }

        private Task<T> CreateMessageAndListen<T>(
            MessageMethod method,
            ChaincodeMessage.Types.Type type,
            IMessage payload,
            string channelId,
            string txId
        )
        {
            return AskPeerAndListen<T>(CreateMessage(type, payload, channelId, txId), method);
        }

        private Task<T> AskPeerAndListen<T>(ChaincodeMessage message, MessageMethod method)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();

            var queueMessage = new QueueMessage<T>(message, method, taskCompletionSource);
            _messageQueue.QueueMessage(queueMessage);

            return taskCompletionSource.Task;
        }
    }

    internal enum HandleMessageAction
    {
        Init,
        Invoke
    }
}
