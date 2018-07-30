using System;
using Chaincode.NET.Extensions;
using Chaincode.NET.Handler;
using Chaincode.NET.Handler.Iterators;
using FluentAssertions;
using Google.Protobuf;
using Moq;
using Protos;
using Queryresult;
using Xunit;

namespace Chaincode.NET.Test.Handler
{
    // All tests are done with StateQueryIterator, since they apply to the HistoryQueryIterator as well
    public class IteratorTest
    {
        [Fact]
        public async void Close_calls_the_handlers_HandleQueryCloseState()
        {
            var handlerMock = new Mock<IHandler>();
            handlerMock.Setup(m => m.HandleQueryCloseState("Id", "ChannelId", "TxId"))
                .ReturnsAsync(new QueryResponse() {HasMore = false, Id = "Id"});

            var response = new QueryResponse() {Id = "Id"};

            var iterator = new StateQueryIterator(handlerMock.Object, "ChannelId", "TxId", response);

            var result = await iterator.Close();

            handlerMock.VerifyAll();
            result.Id.Should().Be("Id");
        }

        [Fact]
        public async void Next_emits_OnEnd_when_no_more_result_sets_are_available()
        {
            var eventDispatched = false;

            var iterator = new StateQueryIterator(null, null, null, new QueryResponse()
            {
                HasMore = false
            });

            iterator.End += () => eventDispatched = true;

            var result = await iterator.Next();

            eventDispatched.Should().BeTrue();
            result.Done.Should().BeTrue();
        }

        [Fact]
        public async void Next_emits_OnData_when_data_is_available()
        {
            var response = new QueryResponse() {HasMore = true};
            response.Results.AddRange(new[]
            {
                new QueryResultBytes()
                {
                    ResultBytes = new KV()
                        {
                            Key = "key1",
                            Namespace = "namespace1",
                            Value = "foo".ToByteString()
                        }
                        .ToByteString()
                },
                new QueryResultBytes()
                {
                    ResultBytes = new KV()
                        {
                            Key = "key2",
                            Namespace = "namespace2",
                            Value = "bar".ToByteString()
                        }
                        .ToByteString()
                }
            });

            QueryResult<KV> queryResult = null;

            var iterator = new StateQueryIterator(null, null, null, response);

            iterator.Data += result => queryResult = result;

            var iteratorResult = await iterator.Next();

            iteratorResult.Should().NotBeNull();
            queryResult.Should().NotBeNull();
            iteratorResult.Should().BeSameAs(queryResult);

            queryResult.Value.Key.Should().Be("key1");
            queryResult.Value.Namespace.Should().Be("namespace1");
            queryResult.Value.Value.ToStringUtf8().Should().Be("foo");

            iteratorResult = await iterator.Next();

            iteratorResult.Should().NotBeNull();
            queryResult.Should().NotBeNull();
            iteratorResult.Should().BeSameAs(queryResult);

            queryResult.Value.Key.Should().Be("key2");
            queryResult.Value.Namespace.Should().Be("namespace2");
            queryResult.Value.Value.ToStringUtf8().Should().Be("bar");
        }

        [Fact]
        public async void
            Next_calls_handlers_HandleQueryNextState_when_results_are_processed_but_more_results_are_available()
        {
            var response = new QueryResponse();
            response.Results.Add(new QueryResultBytes()
            {
                ResultBytes =
                    new KV()
                    {
                        Key = "key",
                        Namespace = "namespace",
                        Value = "value".ToByteString()
                    }.ToByteString()
            });

            var handlerMock = new Mock<IHandler>();
            handlerMock.Setup(m => m.HandleQueryStateNext("Id", "ChannelId", "TxId"))
                .ReturnsAsync(response);

            var iterator = new StateQueryIterator(handlerMock.Object, "ChannelId", "TxId",
                new QueryResponse() {HasMore = true, Id = "Id"});

            var result = await iterator.Next();

            handlerMock.VerifyAll();
            result.Value.Key.Should().Be("key");
            result.Value.Namespace.Should().Be("namespace");
            result.Value.Value.ToStringUtf8().Should().Be("value");
        }

        [Fact]
        public void
            Next_throws_an_exception_on_next_if_getting_the_next_query_state_throws_and_no_error_handler_is_assigned()
        {
            var handlerMock = new Mock<IHandler>();
            handlerMock.Setup(m => m.HandleQueryStateNext(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("unittest"));

            var iterator = new StateQueryIterator(handlerMock.Object, null, null, new QueryResponse() {HasMore = true});

            iterator.Awaiting(i => i.Next())
                .Should().Throw<Exception>()
                .WithMessage("unittest");
        }

        [Fact]
        public void
            Next_emits_error_on_next_if_getting_the_next_query_state_throws_and_an_error_handler_is_assigned()
        {
            var handlerMock = new Mock<IHandler>();
            handlerMock.Setup(m => m.HandleQueryStateNext(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("unittest"));

            var iterator = new StateQueryIterator(handlerMock.Object, null, null, new QueryResponse() {HasMore = true});

            Exception exception = null;

            iterator.Error += ex => exception = ex;

            iterator.Awaiting(i => i.Next())
                .Should().NotThrow();

            exception.Should().NotBeNull();
            exception.Message.Should().Be("unittest");
        }

        [Fact]
        public async void HistoryQueryIterator_next_emits_OnData_when_data_is_available()
        {
            var response = new QueryResponse() {HasMore = true};
            response.Results.AddRange(new[]
            {
                new QueryResultBytes()
                {
                    ResultBytes = new KeyModification()
                        {
                            TxId = "txid1",
                            IsDelete = false,
                            Value = "foo".ToByteString()
                        }
                        .ToByteString()
                },
                new QueryResultBytes()
                {
                    ResultBytes = new KeyModification()
                        {
                            TxId = "txid2",
                            IsDelete = true,
                            Value = "foo".ToByteString()
                        }
                        .ToByteString()
                }
            });

            QueryResult<KeyModification> queryResult = null;

            var iterator = new HistoryQueryIterator(null, null, null, response);

            iterator.Data += result => queryResult = result;

            var iteratorResult = await iterator.Next();

            iteratorResult.Should().NotBeNull();
            queryResult.Should().NotBeNull();
            iteratorResult.Should().BeSameAs(queryResult);

            queryResult.Value.IsDelete.Should().BeFalse();
            queryResult.Value.TxId.Should().Be("txid1");
            queryResult.Value.Value.ToStringUtf8().Should().Be("foo");

            iteratorResult = await iterator.Next();

            iteratorResult.Should().NotBeNull();
            queryResult.Should().NotBeNull();
            iteratorResult.Should().BeSameAs(queryResult);

            queryResult.Value.IsDelete.Should().BeTrue();
            queryResult.Value.TxId.Should().Be("txid2");
            queryResult.Value.Value.ToStringUtf8().Should().Be("foo");
        }
    }
}