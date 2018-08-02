using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Protos;
using Thinktecture.HyperledgerFabric.Chaincode.Chaincode;
using Thinktecture.HyperledgerFabric.Chaincode.Extensions;
using Thinktecture.HyperledgerFabric.Chaincode.Handler;
using Thinktecture.HyperledgerFabric.Chaincode.Handler.Iterators;
using Thinktecture.HyperledgerFabric.Chaincode.Messaging;
using Xunit;
using Metadata = Grpc.Core.Metadata;

namespace Thinktecture.HyperledgerFabric.Chaincode.Sample.Handler
{
    public class HandlerTest
    {
        private class NullChaincodeSupportClientFactory : ChaincodeSupport.ChaincodeSupportClient
        {
            protected override ChaincodeSupport.ChaincodeSupportClient NewInstance(
                ClientBaseConfiguration configuration
            )
            {
                return null;
            }

            public override AsyncDuplexStreamingCall<ChaincodeMessage, ChaincodeMessage> Register(
                Metadata headers = null,
                DateTime? deadline = null,
                CancellationToken cancellationToken = default(CancellationToken)
            )
            {
                return new AsyncDuplexStreamingCall<ChaincodeMessage, ChaincodeMessage>(
                    new Mock<IClientStreamWriter<ChaincodeMessage>>().Object,
                    new Mock<IAsyncStreamReader<ChaincodeMessage>>().Object,
                    null, null, null, null
                );
            }

            public override AsyncDuplexStreamingCall<ChaincodeMessage, ChaincodeMessage>
                Register(CallOptions options)
            {
                return null;
            }
        }

        private IHandler CreateHandler(
            IMessageQueueFactory messageQueueFactory,
            IChaincodeSupportClientFactory chaincodeSupportClientFactory,
            IChaincodeStubFactory chaincodeStubFactory = null,
            IChaincode chaincode = null
        )
        {
            return new global::Thinktecture.HyperledgerFabric.Chaincode.Handler.Handler(
                chaincode ?? new Mock<IChaincode>().Object,
                "example.test",
                9999,
                chaincodeStubFactory ?? new Mock<IChaincodeStubFactory>().Object,
                new Mock<ILogger<global::Thinktecture.HyperledgerFabric.Chaincode.Handler.Handler>>().Object,
                messageQueueFactory,
                chaincodeSupportClientFactory
            );
        }

        private IHandler CreateValidHandler()
        {
            return CreateHandlerWithChainsupportClientFactory(new Mock<IMessageQueueFactory>().Object);
        }

        private IHandler CreateHandlerWithChainsupportClientFactory(IMessageQueueFactory messageQueueFactory)
        {
            var chaincodeSupportClientFactoryMock = new Mock<IChaincodeSupportClientFactory>();
            chaincodeSupportClientFactoryMock.Setup(m => m.Create(It.IsAny<Channel>()))
                .Returns(new NullChaincodeSupportClientFactory());

            return CreateHandler(messageQueueFactory, chaincodeSupportClientFactoryMock.Object);
        }

        private (IHandler Handler, Mock<IMessageQueue> MessageQueueMock)
            CreateValidHandlerWithMessageQueueExpectation(
                MessageMethod messageMethod,
                ChaincodeMessage.Types.Type type
            )
        {
            var messageQueueMock = new Mock<IMessageQueue>();
            messageQueueMock.Setup(m => m.QueueMessage(It.Is(
                (QueueMessage message) => message.Method == messageMethod && message.Message.Type == type
            )));

            var messageQueueFactoryMock = new Mock<IMessageQueueFactory>();
            messageQueueFactoryMock.Setup(m => m.Create(It.IsAny<IHandler>()))
                .Returns(messageQueueMock.Object);

            return (CreateHandlerWithChainsupportClientFactory(messageQueueFactoryMock.Object), messageQueueMock);
        }

        private (Mock<IAsyncStreamReader<ChaincodeMessage>> responseStreamMock,
            Mock<ChaincodeSupport.ChaincodeSupportClient> chaincodeSupportClientMock,
            Mock<IChaincodeSupportClientFactory> chaincodeSupportClientFactoryMock,
            IHandler handler)
            CreateHandlerMockWithStreamExpectation(
                ChaincodeMessage responseMessage,
                IClientStreamWriter<ChaincodeMessage> requestStream = null,
                IMessageQueueFactory messageQueueFactory = null,
                IChaincodeStubFactory chaincodeStubFactory = null,
                IChaincode chaincode = null
            )
        {
            var responseStreamMock = new Mock<IAsyncStreamReader<ChaincodeMessage>>();
            responseStreamMock.SetupSequence(m => m.MoveNext(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            responseStreamMock.Setup(m => m.Current)
                .Returns(responseMessage);

            var asyncDuplexStream = new AsyncDuplexStreamingCall<ChaincodeMessage, ChaincodeMessage>(
                requestStream ?? new Mock<IClientStreamWriter<ChaincodeMessage>>().Object,
                responseStreamMock.Object,
                null, null, null, null
            );

            var chaincodeSupportClientMock = new Mock<ChaincodeSupport.ChaincodeSupportClient>();
            chaincodeSupportClientMock.Setup(m => m.Register(null, null, It.IsAny<CancellationToken>()))
                .Returns(asyncDuplexStream);

            var chaincodeSupportClientFactoryMock = new Mock<IChaincodeSupportClientFactory>();
            chaincodeSupportClientFactoryMock.Setup(m => m.Create(It.IsAny<Channel>()))
                .Returns(chaincodeSupportClientMock.Object);

            var handler = CreateHandler(messageQueueFactory ?? new Mock<IMessageQueueFactory>().Object,
                chaincodeSupportClientFactoryMock.Object, chaincodeStubFactory, chaincode);
            return (responseStreamMock, chaincodeSupportClientMock, chaincodeSupportClientFactoryMock, handler);
        }

        [Fact]
        public async void
            Chat_handles_the_message_with_message_queue_when_state_is_ready_and_response_is_of_type_error()
        {
            var responseMessage = new ChaincodeMessage
            {
                Type = ChaincodeMessage.Types.Type.Error
            };

            var messageQueueMock = new Mock<IMessageQueue>();
            messageQueueMock.Setup(m => m.HandleMessageResponse(responseMessage));

            var messageQueueFactoryMock = new Mock<IMessageQueueFactory>();
            messageQueueFactoryMock.Setup(m => m.Create(It.IsAny<global::Thinktecture.HyperledgerFabric.Chaincode.Handler.Handler>()))
                .Returns(messageQueueMock.Object);

            var (responseStreamMock, chaincodeSupportClientMock, chaincodeSupportClientFactoryMock, handler) =
                CreateHandlerMockWithStreamExpectation(responseMessage, null, messageQueueFactoryMock.Object);

            handler.State = States.Ready;

            await handler.Chat(null);

            chaincodeSupportClientFactoryMock.VerifyAll();
            chaincodeSupportClientMock.VerifyAll();
            responseStreamMock.VerifyAll();
            messageQueueFactoryMock.VerifyAll();
            messageQueueMock.VerifyAll();
        }

        [Fact]
        public async void
            Chat_handles_the_message_with_message_queue_when_state_is_ready_and_response_is_of_type_response()
        {
            var responseMessage = new ChaincodeMessage
            {
                Type = ChaincodeMessage.Types.Type.Response
            };

            var messageQueueMock = new Mock<IMessageQueue>();
            messageQueueMock.Setup(m => m.HandleMessageResponse(responseMessage));

            var messageQueueFactoryMock = new Mock<IMessageQueueFactory>();
            messageQueueFactoryMock.Setup(m => m.Create(It.IsAny<global::Thinktecture.HyperledgerFabric.Chaincode.Handler.Handler>()))
                .Returns(messageQueueMock.Object);

            var (responseStreamMock, chaincodeSupportClientMock, chaincodeSupportClientFactoryMock, handler) =
                CreateHandlerMockWithStreamExpectation(responseMessage, null, messageQueueFactoryMock.Object);

            handler.State = States.Ready;

            await handler.Chat(null);

            chaincodeSupportClientFactoryMock.VerifyAll();
            chaincodeSupportClientMock.VerifyAll();
            responseStreamMock.VerifyAll();
            messageQueueFactoryMock.VerifyAll();
            messageQueueMock.VerifyAll();
        }

        [Fact]
        public async void Chat_initializes_the_chaincode_when_it_receives_an_init_message()
        {
            var responseMessage = new ChaincodeMessage
            {
                Type = ChaincodeMessage.Types.Type.Init,
                Payload = new ChaincodeInput().ToByteString(),
                ChannelId = "ChannelId",
                Txid = "TxId"
            };

            var chaincodeStub = new Mock<IChaincodeStub>().Object;

            var chaincodeStubFactoryMock = new Mock<IChaincodeStubFactory>();
            chaincodeStubFactoryMock.Setup<IChaincodeStub>(m => m.Create(It.IsAny<global::Thinktecture.HyperledgerFabric.Chaincode.Handler.Handler>(), "ChannelId", "TxId",
                    It.IsAny<ChaincodeInput>(), It.IsAny<SignedProposal>()))
                .Returns(chaincodeStub);

            var response = new Response {Status = (int) ResponseCodes.Ok};

            var chaincodeMock = new Mock<IChaincode>();
            chaincodeMock.Setup(m => m.Init(chaincodeStub))
                .ReturnsAsync(response);

            var conversationStartMessage = new ChaincodeMessage();
            var requestStreamMock = new Mock<IClientStreamWriter<ChaincodeMessage>>();
            requestStreamMock.Setup(m => m.WriteAsync(conversationStartMessage)).Returns(Task.CompletedTask);
            requestStreamMock.Setup(m => m.WriteAsync(It.Is(
                (ChaincodeMessage message) => message.Type == ChaincodeMessage.Types.Type.Completed &&
                                              message.Txid == "TxId" &&
                                              message.ChannelId == "ChannelId" &&
                                              message.Payload == response.ToByteString()
            ))).Returns(Task.CompletedTask);

            var (responseStreamMock, chaincodeSupportClientMock, chaincodeSupportClientFactoryMock, handler) =
                CreateHandlerMockWithStreamExpectation(responseMessage, requestStreamMock.Object, null,
                    chaincodeStubFactoryMock.Object, chaincodeMock.Object);

            handler.State = States.Ready;

            await handler.Chat(conversationStartMessage);

            chaincodeSupportClientFactoryMock.VerifyAll();
            chaincodeSupportClientMock.VerifyAll();
            responseStreamMock.VerifyAll();
            chaincodeStubFactoryMock.VerifyAll();
            chaincodeMock.VerifyAll();
            requestStreamMock.VerifyAll();
        }

        [Fact]
        public async void Chat_invokes_the_chaincode_when_it_receives_a_transaction_message()
        {
            var responseMessage = new ChaincodeMessage
            {
                Type = ChaincodeMessage.Types.Type.Transaction,
                Payload = new ChaincodeInput().ToByteString(),
                ChannelId = "ChannelId",
                Txid = "TxId"
            };

            var chaincodeStub = new Mock<IChaincodeStub>().Object;

            var chaincodeStubFactoryMock = new Mock<IChaincodeStubFactory>();
            chaincodeStubFactoryMock.Setup<IChaincodeStub>(m => m.Create(It.IsAny<global::Thinktecture.HyperledgerFabric.Chaincode.Handler.Handler>(), "ChannelId", "TxId",
                    It.IsAny<ChaincodeInput>(), It.IsAny<SignedProposal>()))
                .Returns(chaincodeStub);

            var response = new Response {Status = (int) ResponseCodes.Ok};

            var chaincodeMock = new Mock<IChaincode>();
            chaincodeMock.Setup(m => m.Invoke(chaincodeStub))
                .ReturnsAsync(response);

            var conversationStartMessage = new ChaincodeMessage();
            var requestStreamMock = new Mock<IClientStreamWriter<ChaincodeMessage>>();
            requestStreamMock.Setup(m => m.WriteAsync(conversationStartMessage)).Returns(Task.CompletedTask);
            requestStreamMock.Setup(m => m.WriteAsync(It.Is(
                (ChaincodeMessage message) => message.Type == ChaincodeMessage.Types.Type.Completed &&
                                              message.Txid == "TxId" &&
                                              message.ChannelId == "ChannelId" &&
                                              message.Payload == response.ToByteString()
            ))).Returns(Task.CompletedTask);

            var (responseStreamMock, chaincodeSupportClientMock, chaincodeSupportClientFactoryMock, handler) =
                CreateHandlerMockWithStreamExpectation(responseMessage, requestStreamMock.Object, null,
                    chaincodeStubFactoryMock.Object, chaincodeMock.Object);

            handler.State = States.Ready;

            await handler.Chat(conversationStartMessage);

            chaincodeSupportClientFactoryMock.VerifyAll();
            chaincodeSupportClientMock.VerifyAll();
            responseStreamMock.VerifyAll();
            chaincodeStubFactoryMock.VerifyAll();
            chaincodeMock.VerifyAll();
            requestStreamMock.VerifyAll();
        }

        [Fact]
        public async void Chat_registers_at_the_chaincode_support_client()
        {
            var clientWriter = new Mock<IClientStreamWriter<ChaincodeMessage>>().Object;
            var asyncDuplexStream = new AsyncDuplexStreamingCall<ChaincodeMessage, ChaincodeMessage>(
                clientWriter,
                new Mock<IAsyncStreamReader<ChaincodeMessage>>().Object,
                null, null, null, null
            );
            var chaincodeSupportClientMock = new Mock<ChaincodeSupport.ChaincodeSupportClient>();
            chaincodeSupportClientMock.Setup(m => m.Register(null, null, It.IsAny<CancellationToken>()))
                .Returns(asyncDuplexStream);

            var chaincodeSupportClientFactoryMock = new Mock<IChaincodeSupportClientFactory>();
            chaincodeSupportClientFactoryMock.Setup(m => m.Create(It.IsAny<Channel>()))
                .Returns(chaincodeSupportClientMock.Object);

            var handler = CreateHandler(new Mock<IMessageQueueFactory>().Object,
                chaincodeSupportClientFactoryMock.Object);

            await handler.Chat(null);

            handler.WriteStream.Should().BeSameAs(clientWriter);
            chaincodeSupportClientMock.VerifyAll();
        }

        [Fact]
        public async void
            Chat_sends_an_error_to_peer_when_it_should_handle_a_transaction_but_chaincode_invocation_responded_with_unknown_response_code()
        {
            var responseMessage = new ChaincodeMessage
            {
                Type = ChaincodeMessage.Types.Type.Transaction,
                Payload = new ChaincodeInput().ToByteString(),
                ChannelId = "ChannelId",
                Txid = "TxId"
            };

            var chaincodeStub = new Mock<IChaincodeStub>().Object;

            var chaincodeStubFactoryMock = new Mock<IChaincodeStubFactory>();
            chaincodeStubFactoryMock.Setup<IChaincodeStub>(m => m.Create(It.IsAny<global::Thinktecture.HyperledgerFabric.Chaincode.Handler.Handler>(), "ChannelId", "TxId",
                    It.IsAny<ChaincodeInput>(), It.IsAny<SignedProposal>()))
                .Returns(chaincodeStub);

            var response = new Response {Status = 0};

            var chaincodeMock = new Mock<IChaincode>();
            chaincodeMock.Setup(m => m.Invoke(chaincodeStub))
                .ReturnsAsync(response);

            var conversationStartMessage = new ChaincodeMessage();
            var requestStreamMock = new Mock<IClientStreamWriter<ChaincodeMessage>>();
            requestStreamMock.Setup(m => m.WriteAsync(conversationStartMessage)).Returns(Task.CompletedTask);
            requestStreamMock.Setup(m => m.WriteAsync(It.Is(
                (ChaincodeMessage message) => message.Type == ChaincodeMessage.Types.Type.Error &&
                                              message.Txid == "TxId" &&
                                              message.ChannelId == "ChannelId" &&
                                              message.Payload.ToStringUtf8().Contains("has not called success or error")
            ))).Returns(Task.CompletedTask);

            var (responseStreamMock, chaincodeSupportClientMock, chaincodeSupportClientFactoryMock, handler) =
                CreateHandlerMockWithStreamExpectation(responseMessage, requestStreamMock.Object, null,
                    chaincodeStubFactoryMock.Object, chaincodeMock.Object);

            handler.State = States.Ready;

            await handler.Chat(conversationStartMessage);

            chaincodeSupportClientFactoryMock.VerifyAll();
            chaincodeSupportClientMock.VerifyAll();
            responseStreamMock.VerifyAll();
            chaincodeStubFactoryMock.VerifyAll();
            chaincodeMock.VerifyAll();
            requestStreamMock.VerifyAll();
        }

        [Fact]
        public async void
            Chat_sends_an_error_to_peer_when_it_should_handle_a_transaction_but_input_parameter_is_incorrect()
        {
            var responseMessage = new ChaincodeMessage
            {
                Type = ChaincodeMessage.Types.Type.Transaction,
                Payload = "unittest".ToByteString(),
                ChannelId = "ChannelId",
                Txid = "TxId"
            };

            var conversationStartMessage = new ChaincodeMessage();
            var requestStreamMock = new Mock<IClientStreamWriter<ChaincodeMessage>>();
            requestStreamMock.Setup(m => m.WriteAsync(conversationStartMessage)).Returns(Task.CompletedTask);
            requestStreamMock.Setup(m => m.WriteAsync(It.Is(
                (ChaincodeMessage message) => message.Type == ChaincodeMessage.Types.Type.Error &&
                                              message.Txid == "TxId" &&
                                              message.ChannelId == "ChannelId" &&
                                              message.Payload == responseMessage.Payload
            ))).Returns(Task.CompletedTask);

            var (responseStreamMock, chaincodeSupportClientMock, chaincodeSupportClientFactoryMock, handler) =
                CreateHandlerMockWithStreamExpectation(responseMessage, requestStreamMock.Object);

            handler.State = States.Ready;

            await handler.Chat(conversationStartMessage);

            chaincodeSupportClientFactoryMock.VerifyAll();
            chaincodeSupportClientMock.VerifyAll();
            responseStreamMock.VerifyAll();
            requestStreamMock.VerifyAll();
        }

        [Fact]
        public async void
            Chat_sends_an_error_to_peer_when_it_should_handle_a_transaction_but_stub_can_not_be_created()
        {
            var responseMessage = new ChaincodeMessage
            {
                Type = ChaincodeMessage.Types.Type.Transaction,
                Payload = new ChaincodeInput().ToByteString(),
                ChannelId = "ChannelId",
                Txid = "TxId"
            };

            var chaincodeStubFactoryMock = new Mock<IChaincodeStubFactory>();
            chaincodeStubFactoryMock.Setup<IChaincodeStub>(m => m.Create(It.IsAny<global::Thinktecture.HyperledgerFabric.Chaincode.Handler.Handler>(), "ChannelId", "TxId",
                    It.IsAny<ChaincodeInput>(), It.IsAny<SignedProposal>()))
                .Throws(new Exception("unittest"));

            var conversationStartMessage = new ChaincodeMessage();
            var requestStreamMock = new Mock<IClientStreamWriter<ChaincodeMessage>>();
            requestStreamMock.Setup(m => m.WriteAsync(conversationStartMessage)).Returns(Task.CompletedTask);
            requestStreamMock.Setup(m => m.WriteAsync(It.Is(
                (ChaincodeMessage message) => message.Type == ChaincodeMessage.Types.Type.Error &&
                                              message.Txid == "TxId" &&
                                              message.ChannelId == "ChannelId" &&
                                              message.Payload.ToStringUtf8().Contains("unittest")
            ))).Returns(Task.CompletedTask);

            var (responseStreamMock, chaincodeSupportClientMock, chaincodeSupportClientFactoryMock, handler) =
                CreateHandlerMockWithStreamExpectation(responseMessage, requestStreamMock.Object, null,
                    chaincodeStubFactoryMock.Object);

            handler.State = States.Ready;

            await handler.Chat(conversationStartMessage);

            chaincodeSupportClientFactoryMock.VerifyAll();
            chaincodeSupportClientMock.VerifyAll();
            responseStreamMock.VerifyAll();
            chaincodeStubFactoryMock.VerifyAll();
            requestStreamMock.VerifyAll();
        }

        [Fact]
        public async void
            Chat_sends_an_error_to_the_peer_when_it_is_in_state_created_but_it_does_not_receive_a_registered_response()
        {
            var responseMessage = new ChaincodeMessage
            {
                Type = ChaincodeMessage.Types.Type.Completed
            };

            var requestStreamMock = new Mock<IClientStreamWriter<ChaincodeMessage>>();
            requestStreamMock.Setup(m => m.WriteAsync(null)).Returns(Task.CompletedTask);
            requestStreamMock.Setup(m => m.WriteAsync(It.Is(
                (ChaincodeMessage message) => message != null && message.Type == ChaincodeMessage.Types.Type.Error
            ))).Returns(Task.CompletedTask);

            var (responseStreamMock, chaincodeSupportClientMock, chaincodeSupportClientFactoryMock, handler) =
                CreateHandlerMockWithStreamExpectation(responseMessage, requestStreamMock.Object);

            await handler.Chat(null);

            chaincodeSupportClientFactoryMock.VerifyAll();
            chaincodeSupportClientMock.VerifyAll();
            responseStreamMock.VerifyAll();
            requestStreamMock.VerifyAll();
            handler.State.Should().Be(States.Created);
        }

        [Fact]
        public async void
            Chat_sends_an_error_to_the_peer_when_it_is_in_state_established_but_it_does_not_receive_a_ready_response()
        {
            var responseMessage = new ChaincodeMessage
            {
                Type = ChaincodeMessage.Types.Type.Completed
            };

            var requestStreamMock = new Mock<IClientStreamWriter<ChaincodeMessage>>();
            requestStreamMock.Setup(m => m.WriteAsync(null)).Returns(Task.CompletedTask);
            requestStreamMock.Setup(m => m.WriteAsync(It.Is(
                (ChaincodeMessage message) => message != null && message.Type == ChaincodeMessage.Types.Type.Error
            ))).Returns(Task.CompletedTask);

            var (responseStreamMock, chaincodeSupportClientMock, chaincodeSupportClientFactoryMock, handler) =
                CreateHandlerMockWithStreamExpectation(responseMessage, requestStreamMock.Object);

            handler.State = States.Established;
            await handler.Chat(null);

            chaincodeSupportClientFactoryMock.VerifyAll();
            chaincodeSupportClientMock.VerifyAll();
            responseStreamMock.VerifyAll();
            requestStreamMock.VerifyAll();
            handler.State.Should().Be(States.Established);
        }

        [Fact]
        public async void Chat_sends_the_conversation_starter_message()
        {
            var conversationStarterMessage = new ChaincodeMessage
            {
                Type = ChaincodeMessage.Types.Type.Register
            };

            var clientWriterMock = new Mock<IClientStreamWriter<ChaincodeMessage>>();
            clientWriterMock.Setup(m => m.WriteAsync(conversationStarterMessage))
                .Returns(Task.CompletedTask);

            var clientWriter = clientWriterMock.Object;
            var asyncDuplexStream = new AsyncDuplexStreamingCall<ChaincodeMessage, ChaincodeMessage>(
                clientWriter,
                new Mock<IAsyncStreamReader<ChaincodeMessage>>().Object,
                null, null, null, null
            );

            var chaincodeSupportClientMock = new Mock<ChaincodeSupport.ChaincodeSupportClient>();
            chaincodeSupportClientMock.Setup(m => m.Register(null, null, It.IsAny<CancellationToken>()))
                .Returns(asyncDuplexStream);

            var chaincodeSupportClientFactoryMock = new Mock<IChaincodeSupportClientFactory>();
            chaincodeSupportClientFactoryMock.Setup(m => m.Create(It.IsAny<Channel>()))
                .Returns(chaincodeSupportClientMock.Object);

            var handler = CreateHandler(new Mock<IMessageQueueFactory>().Object,
                chaincodeSupportClientFactoryMock.Object);

            await handler.Chat(conversationStarterMessage);

            clientWriterMock.VerifyAll();
        }

        [Fact]
        public async void Chat_switches_state_from_created_to_established_when_it_receives_a_registered_response()
        {
            var responseMessage = new ChaincodeMessage
            {
                Type = ChaincodeMessage.Types.Type.Registered
            };

            var (responseStreamMock, chaincodeSupportClientMock, chaincodeSupportClientFactoryMock, handler) =
                CreateHandlerMockWithStreamExpectation(responseMessage);

            await handler.Chat(null);

            chaincodeSupportClientFactoryMock.VerifyAll();
            chaincodeSupportClientMock.VerifyAll();
            responseStreamMock.VerifyAll();
            handler.State.Should().Be(States.Established);
        }

        [Fact]
        public async void Chat_switches_state_from_established_to_ready_when_it_receives_a_ready_response()
        {
            var responseMessage = new ChaincodeMessage
            {
                Type = ChaincodeMessage.Types.Type.Ready
            };

            var (responseStreamMock, chaincodeSupportClientMock, chaincodeSupportClientFactoryMock, handler) =
                CreateHandlerMockWithStreamExpectation(responseMessage);

            handler.State = States.Established;
            await handler.Chat(null);

            chaincodeSupportClientFactoryMock.VerifyAll();
            chaincodeSupportClientMock.VerifyAll();
            responseStreamMock.VerifyAll();
            handler.State.Should().Be(States.Ready);
        }

        [Fact]
        public async void Close_stops_the_long_running_chat_method()
        {
            var handler = CreateValidHandler();

            var task = handler.Chat(new ChaincodeMessage());

            handler.Close();

            await task;

            task.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public void HandleDeleteState_queues_a_message_of_type_DelState()
        {
            var sut = CreateValidHandlerWithMessageQueueExpectation(MessageMethod.DelState,
                ChaincodeMessage.Types.Type.DelState);

            sut.Handler.HandleDeleteState(string.Empty, string.Empty, string.Empty, string.Empty);

            sut.MessageQueueMock.VerifyAll();
        }

        [Fact]
        public void HandleGetHistoryForKey_queues_a_message_of_type_GetHistoryForKey()
        {
            var sut = CreateValidHandlerWithMessageQueueExpectation(MessageMethod.GetHistoryForKey,
                ChaincodeMessage.Types.Type.GetHistoryForKey);

            sut.Handler.HandleGetHistoryForKey(string.Empty, string.Empty, string.Empty);

            sut.MessageQueueMock.VerifyAll();
        }

        [Fact]
        public void HandleGetQueryResult_queues_a_message_of_type_GetQueryResult()
        {
            var sut = CreateValidHandlerWithMessageQueueExpectation(MessageMethod.GetQueryResult,
                ChaincodeMessage.Types.Type.GetQueryResult);

            sut.Handler.HandleGetQueryResult(string.Empty, string.Empty, string.Empty, string.Empty);

            sut.MessageQueueMock.VerifyAll();
        }

        [Fact]
        public void HandleGetState_queues_a_message_of_type_GetState()
        {
            var sut = CreateValidHandlerWithMessageQueueExpectation(MessageMethod.GetState,
                ChaincodeMessage.Types.Type.GetState);

            sut.Handler.HandleGetState(string.Empty, string.Empty, string.Empty, string.Empty);

            sut.MessageQueueMock.VerifyAll();
        }

        [Fact]
        public void HandleGetStateByRange_queues_a_message_of_type_GetStateByRange()
        {
            var sut = CreateValidHandlerWithMessageQueueExpectation(MessageMethod.GetStateByRange,
                ChaincodeMessage.Types.Type.GetStateByRange);

            sut.Handler.HandleGetStateByRange(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

            sut.MessageQueueMock.VerifyAll();
        }

        [Fact]
        public async void HandleInvokeChaincode_returns_a_response()
        {
            QueueMessage<ChaincodeMessage> queuedMessage = null;
            var messageQueueMock = new Mock<IMessageQueue>();
            messageQueueMock.Setup(m => m.QueueMessage(It.IsAny<QueueMessage>()))
                .Callback<QueueMessage>(message => queuedMessage = message as QueueMessage<ChaincodeMessage>);

            var messageQueueFactoryMock = new Mock<IMessageQueueFactory>();
            messageQueueFactoryMock.Setup(m => m.Create(It.IsAny<global::Thinktecture.HyperledgerFabric.Chaincode.Handler.Handler>()))
                .Returns(messageQueueMock.Object);

            var handler = CreateHandlerWithChainsupportClientFactory(messageQueueFactoryMock.Object);

            var task = handler.HandleInvokeChaincode("ChaincodeName", new ByteString[0], "ChannelId", "TxId");

            queuedMessage.Should().NotBeNull();

            queuedMessage.Success(new ChaincodeMessage
            {
                Type = ChaincodeMessage.Types.Type.Completed,
                Payload = new Response
                {
                    Message = "unittest"
                }.ToByteString()
            });

            var result = await task;
            result.Message.Should().Be("unittest");
        }

        [Fact]
        public void HandleInvokeChaincode_throws_an_exception_when_response_is_of_an_unknown_type()
        {
            QueueMessage<ChaincodeMessage> queuedMessage = null;
            var messageQueueMock = new Mock<IMessageQueue>();
            messageQueueMock.Setup(m => m.QueueMessage(It.IsAny<QueueMessage>()))
                .Callback<QueueMessage>(message => queuedMessage = message as QueueMessage<ChaincodeMessage>);

            var messageQueueFactoryMock = new Mock<IMessageQueueFactory>();
            messageQueueFactoryMock.Setup(m => m.Create(It.IsAny<global::Thinktecture.HyperledgerFabric.Chaincode.Handler.Handler>()))
                .Returns(messageQueueMock.Object);

            var handler = CreateHandlerWithChainsupportClientFactory(messageQueueFactoryMock.Object);

            var task = handler.HandleInvokeChaincode("ChaincodeName", new ByteString[0], "ChannelId", "TxId");

            queuedMessage.Should().NotBeNull();

            queuedMessage.Success(new ChaincodeMessage
            {
                Type = (ChaincodeMessage.Types.Type) 99999
            });

            Func<Task> act = async () => await task;

            act.Should().Throw<Exception>()
                .WithMessage("Something went somewhere horribly wrong");
        }

        [Fact]
        public void HandleInvokeChaincode_throws_an_exception_when_response_is_of_type_error()
        {
            QueueMessage<ChaincodeMessage> queuedMessage = null;
            var messageQueueMock = new Mock<IMessageQueue>();
            messageQueueMock.Setup(m => m.QueueMessage(It.IsAny<QueueMessage>()))
                .Callback<QueueMessage>(message => queuedMessage = message as QueueMessage<ChaincodeMessage>);

            var messageQueueFactoryMock = new Mock<IMessageQueueFactory>();
            messageQueueFactoryMock.Setup(m => m.Create(It.IsAny<global::Thinktecture.HyperledgerFabric.Chaincode.Handler.Handler>()))
                .Returns(messageQueueMock.Object);

            var handler = CreateHandlerWithChainsupportClientFactory(messageQueueFactoryMock.Object);

            var task = handler.HandleInvokeChaincode("ChaincodeName", new ByteString[0], "ChannelId", "TxId");

            queuedMessage.Should().NotBeNull();

            queuedMessage.Success(new ChaincodeMessage
            {
                Type = ChaincodeMessage.Types.Type.Error,
                Payload = "unittest".ToByteString()
            });

            Func<Task> act = async () => await task;

            act.Should().Throw<Exception>()
                .WithMessage("unittest");
        }

        [Fact]
        public void HandlePutState_queues_a_message_of_type_PutState()
        {
            var sut = CreateValidHandlerWithMessageQueueExpectation(MessageMethod.PutState,
                ChaincodeMessage.Types.Type.PutState);

            sut.Handler.HandlePutState(string.Empty, string.Empty, ByteString.Empty, string.Empty, string.Empty);

            sut.MessageQueueMock.VerifyAll();
        }

        [Fact]
        public void HandleQueryCloseState_queues_a_message_of_type_QueryCloseState()
        {
            var sut = CreateValidHandlerWithMessageQueueExpectation(MessageMethod.QueryStateClose,
                ChaincodeMessage.Types.Type.QueryStateClose);

            sut.Handler.HandleQueryCloseState(string.Empty, string.Empty, string.Empty);

            sut.MessageQueueMock.VerifyAll();
        }

        [Fact]
        public void HandleQueryStateNext_queues_a_message_of_type_QueryStateNext()
        {
            var sut = CreateValidHandlerWithMessageQueueExpectation(MessageMethod.QueryStateNext,
                ChaincodeMessage.Types.Type.QueryStateNext);

            sut.Handler.HandleQueryStateNext(string.Empty, string.Empty, string.Empty);

            sut.MessageQueueMock.VerifyAll();
        }

        [Fact]
        public void ParseObject_returns_a_ByteString_for_non_special_method_type()
        {
            var handler = CreateValidHandler();

            var result =
                handler.ParseResponse(new ChaincodeMessage {Type = ChaincodeMessage.Types.Type.Response},
                    MessageMethod.GetState);

            result.Should().BeOfType<ByteString>();
        }

        [Fact]
        public void ParseObject_returns_a_ChaincodeMessage_for_response_of_method_type_InvokeChaincode()
        {
            var handler = CreateValidHandler();

            var result =
                handler.ParseResponse(new ChaincodeMessage {Type = ChaincodeMessage.Types.Type.Response},
                    MessageMethod.InvokeChaincode);

            result.Should().BeOfType<ChaincodeMessage>();
        }

        [Fact]
        public void ParseObject_returns_a_HistoryQueryIterator_for_response_of_method_type_GetHistoryForKey()
        {
            var handler = CreateValidHandler();

            var result =
                handler.ParseResponse(new ChaincodeMessage {Type = ChaincodeMessage.Types.Type.Response},
                    MessageMethod.GetHistoryForKey);

            result.Should().BeOfType<HistoryQueryIterator>();
        }

        [Fact]
        public void ParseObject_returns_a_QueryParser_for_response_of_method_type_QueryNextState()
        {
            var handler = CreateValidHandler();

            var result =
                handler.ParseResponse(new ChaincodeMessage {Type = ChaincodeMessage.Types.Type.Response},
                    MessageMethod.QueryStateNext);

            result.Should().BeOfType<QueryResponse>();
        }

        [Fact]
        public void ParseObject_returns_a_QueryParser_for_response_of_method_type_QueryStateClose()
        {
            var handler = CreateValidHandler();

            var result =
                handler.ParseResponse(new ChaincodeMessage {Type = ChaincodeMessage.Types.Type.Response},
                    MessageMethod.QueryStateClose);

            result.Should().BeOfType<QueryResponse>();
        }

        [Fact]
        public void ParseObject_returns_a_StateQueryIterator_for_response_of_method_type_GetQueryResult()
        {
            var handler = CreateValidHandler();

            var result =
                handler.ParseResponse(new ChaincodeMessage {Type = ChaincodeMessage.Types.Type.Response},
                    MessageMethod.GetQueryResult);

            result.Should().BeOfType<StateQueryIterator>();
        }

        [Fact]
        public void ParseObject_returns_a_StateQueryIterator_for_response_of_method_type_GetStateByRange()
        {
            var handler = CreateValidHandler();

            var result =
                handler.ParseResponse(new ChaincodeMessage {Type = ChaincodeMessage.Types.Type.Response},
                    MessageMethod.GetStateByRange);

            result.Should().BeOfType<StateQueryIterator>();
        }

        [Fact]
        public void ParseObject_throws_an_exception_if_ChaincodeMessage_type_is_unknown()
        {
            var handler = CreateValidHandler();

            handler.Invoking(h => h.ParseResponse(new ChaincodeMessage
                {
                    Type = (ChaincodeMessage.Types.Type) 999999
                }, 0))
                .Should().Throw<Exception>()
                .WithMessage("*Received incorrect chaincode in response*");
        }

        [Fact]
        public void ParseObject_throws_an_exception_if_response_is_of_type_error()
        {
            var handler = CreateValidHandler();

            handler.Invoking(h => h.ParseResponse(new ChaincodeMessage
                {
                    Type = ChaincodeMessage.Types.Type.Error,
                    Payload = "error response".ToByteString()
                }, 0))
                .Should().Throw<Exception>()
                .WithMessage("error response");
        }
    }
}
