using System;
using System.Threading.Tasks;
using Chaincode.NET.Messaging;
using FluentAssertions;
using Protos;
using Xunit;

namespace Chaincode.NET.Test.Messaging
{
    public class QueueMessageTest
    {
        [Fact]
        public void Fail_throws_the_given_exception()
        {
            var sut = new QueueMessage(null, 0);

            sut.Invoking(s => s.Fail(new Exception("something went wrong")))
                .Should().Throw<Exception>()
                .WithMessage("something went wrong");
        }

        [Fact]
        public void MessageTxContextId_is_built_by_MessageChannelId_and_MessageTxid()
        {
            var sut = new QueueMessage(new ChaincodeMessage()
            {
                ChannelId = "foo",
                Txid = "bar"
            }, 0);

            sut.MessageTxContextId.Should().Be("foobar");
        }
    }

    public class QueueMessageGenericTest
    {
        [Fact]
        public async Task Success_should_result_of_TaskCompletionSource()
        {
            var taskCompletionSource = new TaskCompletionSource<int>();
            var sut = new QueueMessage<int>(null, 0, taskCompletionSource);
            sut.Success(100);

            var result = await taskCompletionSource.Task;
            result.Should().Be(100);
        }

        [Fact]
        public void Fail_should_set_exception_of_TaskCompletionSource()
        {
            var taskCompletionSource = new TaskCompletionSource<int>();
            var sut = new QueueMessage<int>(null, 0, taskCompletionSource);
            sut.Fail(new Exception("something went wrong"));

            taskCompletionSource.Awaiting(t => t.Task)
                .Should().Throw<Exception>()
                .WithMessage("something went wrong");
        }
    }
}