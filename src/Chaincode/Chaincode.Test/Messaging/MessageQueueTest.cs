using System;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Protos;
using Thinktecture.HyperledgerFabric.Chaincode.Handler;
using Thinktecture.HyperledgerFabric.Chaincode.Messaging;
using Xunit;

namespace Thinktecture.HyperledgerFabric.Chaincode.Test.Messaging
{
    public class MessageQueueTest
    {
        [Fact]
        public async Task Correctly_handles_a_response_message()
        {
            var handlerMock = new Mock<IHandler>();
            var streamMock = new Mock<IClientStreamWriter<ChaincodeMessage>>();

            handlerMock.SetupGet(m => m.WriteStream).Returns(streamMock.Object);

            handlerMock.Setup<object>(m => m.ParseResponse(It.IsAny<ChaincodeMessage>(), It.IsAny<MessageMethod>()))
                .Returns("foobar");

            var sut = new MessageQueue(handlerMock.Object, new Mock<ILogger<MessageQueue>>().Object);

            var taskCompletionSource = new TaskCompletionSource<string>();
            var queueMessage = new QueueMessage<string>(new ChaincodeMessage
            {
                ChannelId = "chaincode",
                Txid = "message"
            }, 0, taskCompletionSource);

            await sut.QueueMessage(queueMessage);

            sut.HandleMessageResponse(new ChaincodeMessage
            {
                ChannelId = "chaincode",
                Txid = "message"
            });

            var result = await taskCompletionSource.Task;

            result.Should().Be("foobar");
        }

        [Fact]
        public async void Directly_sends_a_single_message()
        {
            var handlerMock = new Mock<IHandler>();
            var streamMock = new Mock<IClientStreamWriter<ChaincodeMessage>>();

            handlerMock.SetupGet(m => m.WriteStream).Returns(streamMock.Object);

            var sut = new MessageQueue(handlerMock.Object, new Mock<ILogger<MessageQueue>>().Object);

            var chaincodeMessage = new ChaincodeMessage
            {
                Txid = "bar",
                ChannelId = "foo"
            };

            await sut.QueueMessage(new QueueMessage(chaincodeMessage, 0));

            streamMock.Verify(m => m.WriteAsync(chaincodeMessage), Times.Once);
        }

        [Fact]
        public async void Does_not_send_two_queued_message_at_the_same_time()
        {
            var handlerMock = new Mock<IHandler>();
            var streamMock = new Mock<IClientStreamWriter<ChaincodeMessage>>();

            handlerMock.SetupGet(m => m.WriteStream).Returns(streamMock.Object);

            var sut = new MessageQueue(handlerMock.Object, new Mock<ILogger<MessageQueue>>().Object);

            var chaincodeMessage = new ChaincodeMessage
            {
                Txid = "bar",
                ChannelId = "foo"
            };

            await sut.QueueMessage(new QueueMessage(chaincodeMessage, 0));
            await sut.QueueMessage(new QueueMessage(new ChaincodeMessage(), 0));

            streamMock.Verify(m => m.WriteAsync(chaincodeMessage), Times.Once);
        }

        [Fact]
        public async Task Throws_an_exception_when_message_could_not_be_parsed()
        {
            var handlerMock = new Mock<IHandler>();
            var streamMock = new Mock<IClientStreamWriter<ChaincodeMessage>>();

            handlerMock.SetupGet(m => m.WriteStream).Returns(streamMock.Object);

            handlerMock.Setup<object>(m => m.ParseResponse(It.IsAny<ChaincodeMessage>(), It.IsAny<MessageMethod>()))
                .Throws<Exception>();

            var sut = new MessageQueue(handlerMock.Object, new Mock<ILogger<MessageQueue>>().Object);

            var taskCompletionSource = new TaskCompletionSource<string>();
            var queueMessage = new QueueMessage<string>(new ChaincodeMessage
            {
                ChannelId = "chaincode",
                Txid = "message"
            }, 0, taskCompletionSource);

            await sut.QueueMessage(queueMessage);

            sut.HandleMessageResponse(new ChaincodeMessage
            {
                ChannelId = "chaincode",
                Txid = "message"
            });

            taskCompletionSource.Awaiting(t => t.Task)
                .Should().Throw<Exception>();
        }

        [Fact]
        public void Throws_an_exception_when_message_is_not_found()
        {
            var sut = new MessageQueue(new Mock<IHandler>().Object, new Mock<ILogger<MessageQueue>>().Object);
            sut.Invoking(s => s.HandleMessageResponse(new ChaincodeMessage
                {
                    ChannelId = "foo",
                    Txid = "bar"
                }))
                .Should().Throw<Exception>();
        }
    }
}
