using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chaincode.NET.Chaincode;
using Chaincode.NET.Extensions;
using Chaincode.NET.Handler;
using Chaincode.NET.Handler.Iterators;
using Common;
using FluentAssertions;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Msp;
using Protos;
using Xunit;

namespace Chaincode.NET.Test.Chaincode
{
    public class ChaincodeStubTest
    {
        private readonly string _maxUnicodeRuneValue = char.ConvertFromUtf32(0x10ffff);

        private SignedProposal CreateValidSignedProposal()
        {
            const int ticksPerMacroSecond = 10;
            const int nanosecondsPerTick = 100;
            var now = DateTime.Now;

            var timestamp = new Timestamp()
            {
                Nanos = (int) (now.Ticks % TimeSpan.TicksPerMillisecond % ticksPerMacroSecond) * nanosecondsPerTick,
                Seconds = now.Ticks / 1000
            };

            return new SignedProposal()
            {
                ProposalBytes = new Proposal()
                {
                    Header = new Header()
                    {
                        ChannelHeader = new ChannelHeader()
                        {
                            ChannelId = "ChannelId",
                            Epoch = 10,
                            Timestamp = timestamp
                        }.ToByteString(),
                        SignatureHeader = new SignatureHeader()
                        {
                            Nonce = "nonce".ToByteString(),
                            Creator = new SerializedIdentity()
                            {
                                IdBytes = "creator".ToByteString(),
                                Mspid = "testMSP"
                            }.ToByteString(),
                        }.ToByteString()
                    }.ToByteString(),
                    Payload = new ChaincodeProposalPayload()
                    {
                        Input = "foobar".ToByteString(),
                        TransientMap =
                        {
                            {"testKey", "testValue".ToByteString()}
                        }
                    }.ToByteString()
                }.ToByteString()
            };
        }

        private ChaincodeStub CreateChaincodeStub(
            IHandler handler,
            string channelId,
            string txId,
            ChaincodeInput chaincodeInput,
            SignedProposal signedProposal
        ) =>
            new ChaincodeStub(handler, channelId, txId, chaincodeInput, signedProposal,
                new NullLogger<ChaincodeStub>());

        private ChaincodeStub CreateTestChaincodeStub(SignedProposal proposal) =>
            CreateChaincodeStub(null, "ChannelId", "TxId", new ChaincodeInput(), proposal);

        private ChaincodeStub CreateValidChaincodeStubWithHandler(IHandler handler) =>
            CreateChaincodeStub(handler, "ChannelId", "TxId", new ChaincodeInput(), CreateValidSignedProposal());

        private ChaincodeStub CreateValidChaincodeStub() => CreateValidChaincodeStubWithHandler(null);

        [Fact]
        public void Throws_an_exception_when_ChaincodeInput_is_not_set()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action act = () => new ChaincodeStub(null, string.Empty, string.Empty, null, null, null);

            act.Should().Throw<Exception>();
        }

        [Fact]
        public void DecodedSignedProposal_should_be_null_if_not_set()
        {
            var chaincodeStub = new ChaincodeStub(null, string.Empty, string.Empty, new ChaincodeInput(), null, null);

            chaincodeStub.DecodedSignedProposal.Should().BeNull();
        }

        [Fact]
        public void Throws_an_exception_when_proposal_bytes_are_garbage()
        {
            Action act = () => CreateTestChaincodeStub(new SignedProposal()
            {
                ProposalBytes = "foobar".ToByteString()
            });

            act.Should().Throw<Exception>().WithMessage("Failed extracting proposal from signedProposal*");
        }

        [Fact]
        public void Throws_an_exception_when_proposal_header_bytes_are_not_set()
        {
            Action act = () => CreateTestChaincodeStub(new SignedProposal());

            act.Should().Throw<Exception>().WithMessage("Proposal header is empty");
        }

        [Fact]
        public void Throws_an_exception_when_proposal_header_bytes_are_empty()
        {
            Action act = () => CreateTestChaincodeStub(new SignedProposal()
            {
                ProposalBytes = new Proposal()
                {
                    Header = ByteString.Empty
                }.ToByteString()
            });

            act.Should().Throw<Exception>().WithMessage("Proposal header is empty");
        }

        [Fact]
        public void Throws_an_exception_when_proposal_payload_bytes_are_not_set()
        {
            Action act = () => CreateTestChaincodeStub(new SignedProposal()
            {
                ProposalBytes = new Proposal()
                {
                    Header = "foobar".ToByteString()
                }.ToByteString()
            });

            act.Should().Throw<Exception>().WithMessage("Proposal payload is empty");
        }

        [Fact]
        public void Throws_an_exception_when_proposal_payload_bytes_are_empty()
        {
            Action act = () => CreateTestChaincodeStub(new SignedProposal()
            {
                ProposalBytes = new Proposal()
                {
                    Header = "foobar".ToByteString(),
                    Payload = ByteString.Empty
                }.ToByteString()
            });

            act.Should().Throw<Exception>().WithMessage("Proposal payload is empty");
        }

        [Fact]
        public void Throws_an_exception_when_proposal_header_bytes_are_garbage()
        {
            Action act = () => CreateTestChaincodeStub(new SignedProposal()
            {
                ProposalBytes = new Proposal()
                {
                    Header = "foobar".ToByteString(),
                    Payload = "foobar".ToByteString()
                }.ToByteString()
            });

            act.Should().Throw<Exception>().WithMessage("Could not extract the header from the proposal*");
        }

        [Fact]
        public void Throws_an_exception_when_proposal_header_signature_header_bytes_are_garbage()
        {
            Action act = () => CreateTestChaincodeStub(new SignedProposal()
            {
                ProposalBytes = new Proposal()
                {
                    Header = new Header()
                    {
                        SignatureHeader = "foobar".ToByteString()
                    }.ToByteString(),
                    Payload = "foobar".ToByteString()
                }.ToByteString()
            });

            act.Should().Throw<Exception>().WithMessage("Decoding SignatureHeader failed*");
        }

        [Fact]
        public void Throws_an_exception_when_proposal_header_signature_header_creator_bytes_are_garbage()
        {
            Action act = () => CreateTestChaincodeStub(new SignedProposal()
            {
                ProposalBytes = new Proposal()
                {
                    Header = new Header()
                    {
                        SignatureHeader = new SignatureHeader()
                        {
                            Creator = "foobar".ToByteString()
                        }.ToByteString()
                    }.ToByteString(),
                    Payload = "foobar".ToByteString()
                }.ToByteString()
            });

            act.Should().Throw<Exception>().WithMessage("Decoding SerializedIdentity failed*");
        }

        [Fact]
        public void Throws_an_exception_when_proposal_header_channel_header_is_garbage()
        {
            Action act = () => CreateTestChaincodeStub(new SignedProposal()
            {
                ProposalBytes = new Proposal()
                {
                    Header = new Header()
                    {
                        ChannelHeader = "foobar".ToByteString(),
                        SignatureHeader = new SignatureHeader()
                        {
                            Creator = new SerializedIdentity().ToByteString(),
                        }.ToByteString()
                    }.ToByteString(),
                    Payload = "foobar".ToByteString()
                }.ToByteString()
            });

            act.Should().Throw<Exception>().WithMessage("Decoding ChannelHeader failed*");
        }

        [Fact]
        public void Throws_an_exception_when_proposal_payload_is_garbage()
        {
            Action act = () => CreateTestChaincodeStub(new SignedProposal()
            {
                ProposalBytes = new Proposal()
                {
                    Header = new Header()
                    {
                        ChannelHeader = new ChannelHeader()
                        {
                            ChannelId = "ChannelId"
                        }.ToByteString(),
                        SignatureHeader = new SignatureHeader()
                        {
                            Creator = new SerializedIdentity().ToByteString(),
                        }.ToByteString()
                    }.ToByteString(),
                    Payload = "foobar".ToByteString()
                }.ToByteString()
            });

            act.Should().Throw<Exception>().WithMessage("Decoding ChaincodeProposalPayload failed*");
        }

        [Fact]
        public void Correctly_decodes_a_signed_proposal()
        {
            Action act = () => CreateTestChaincodeStub(new SignedProposal()
            {
                ProposalBytes = new Proposal()
                {
                    Header = new Header()
                    {
                        ChannelHeader = new ChannelHeader()
                        {
                            ChannelId = "ChannelId"
                        }.ToByteString(),
                        SignatureHeader = new SignatureHeader()
                        {
                            Creator = new SerializedIdentity().ToByteString(),
                        }.ToByteString()
                    }.ToByteString(),
                    Payload = new ChaincodeProposalPayload()
                    {
                        Input = "foobar".ToByteString()
                    }.ToByteString()
                }.ToByteString()
            });

            act.Should().NotThrow();
        }

        // Values are taken from Node.js test sample
        [Fact]
        public void Calculates_the_correct_binding()
        {
            var sut = CreateTestChaincodeStub(new SignedProposal()
            {
                ProposalBytes = new Proposal()
                {
                    Header = new Header()
                    {
                        ChannelHeader = new ChannelHeader()
                        {
                            ChannelId = "ChannelId",
                            Epoch = 10
                        }.ToByteString(),
                        SignatureHeader = new SignatureHeader()
                        {
                            Nonce = "nonce".ToByteString(),
                            Creator = new SerializedIdentity()
                            {
                                IdBytes = "creator".ToByteString(),
                                Mspid = "testMSP"
                            }.ToByteString(),
                        }.ToByteString()
                    }.ToByteString(),
                    Payload = new ChaincodeProposalPayload()
                    {
                        Input = "foobar".ToByteString()
                    }.ToByteString()
                }.ToByteString()
            });

            sut.Binding.Should().Be("81dd35bc764b01dd7f3f38513c6c0e5d5583d4e5568fa74c4847fd29228b51e4");
        }

        // In Node.js TxTimestamp is checked for correct types. Not needed for C#

        [Fact]
        public async void InvokeChaincode_invokes_the_handlers_InvokeChaincode_handler()
        {
            var handlerMock = new Mock<IHandler>();

            var stub = CreateValidChaincodeStubWithHandler(handlerMock.Object);

            var dummyArgs = new List<ByteString>()
            {
                "arg1".ToByteString(),
                "arg2".ToByteString()
            };

            await stub.InvokeChaincode("mycc", dummyArgs);
            await stub.InvokeChaincode("mycc", dummyArgs, "myChannel");

            handlerMock.Verify(m => m.HandleInvokeChaincode("mycc", dummyArgs, "ChannelId", "TxId"), Times.Once);
            handlerMock.Verify(m => m.HandleInvokeChaincode("mycc/myChannel", dummyArgs, "ChannelId", "TxId"),
                Times.Once);
        }

        [Fact]
        public async void GetState_invokes_the_handlers_GetState_handler()
        {
            var handlerMock = new Mock<IHandler>();
            handlerMock.Setup(m =>
                    m.HandleGetState(string.Empty, "key1", "ChannelId", "TxId"))
                .ReturnsAsync("response".ToByteString);

            var stub = CreateValidChaincodeStubWithHandler(handlerMock.Object);

            var result = await stub.GetState("key1");

            handlerMock.VerifyAll();
            result.ToStringUtf8().Should().Be("response");
        }

        [Fact]
        public async void PutState_invokes_the_handlers_PutState_handler()
        {
            var handlerMock = new Mock<IHandler>();
            handlerMock.Setup(m =>
                    m.HandlePutState(string.Empty, "key1", "value1".ToByteString(), "ChannelId", "TxId"))
                .ReturnsAsync("response".ToByteString);

            var stub = CreateValidChaincodeStubWithHandler(handlerMock.Object);

            var result = await stub.PutState("key1", "value1".ToByteString());

            handlerMock.VerifyAll();
            result.ToStringUtf8().Should().Be("response");
        }

        [Fact]
        public async void DeleteState_invokes_the_handlers_DeleteState_handler()
        {
            var handlerMock = new Mock<IHandler>();
            handlerMock.Setup(m =>
                    m.HandleDeleteState(string.Empty, "key1", "ChannelId", "TxId"))
                .ReturnsAsync("response".ToByteString);

            var stub = CreateValidChaincodeStubWithHandler(handlerMock.Object);

            var result = await stub.DeleteState("key1");

            handlerMock.VerifyAll();
            result.ToStringUtf8().Should().Be("response");
        }

        [Fact]
        public async void GetHistoryForKey_invokes_the_handlers_GetHistoryForKey_handler()
        {
            var iteratorMock = new HistoryQueryIterator(null, string.Empty, string.Empty, null);
            var handlerMock = new Mock<IHandler>();
            handlerMock.Setup(m =>
                    m.HandleGetHistoryForKey("key1", "ChannelId", "TxId"))
                .ReturnsAsync(iteratorMock);

            var stub = CreateValidChaincodeStubWithHandler(handlerMock.Object);

            var result = await stub.GetHistoryForKey("key1");

            handlerMock.VerifyAll();
            result.Should().BeSameAs(iteratorMock);
        }

        [Fact]
        public async void GetQueryResult_invokes_the_handlers_GetQueryResult_handler()
        {
            var iteratorMock = new StateQueryIterator(null, string.Empty, string.Empty, null);
            var handlerMock = new Mock<IHandler>();
            handlerMock.Setup(m =>
                    m.HandleGetQueryResult(string.Empty, "query", "ChannelId", "TxId"))
                .ReturnsAsync(iteratorMock);

            var stub = CreateValidChaincodeStubWithHandler(handlerMock.Object);

            var result = await stub.GetQueryResult("query");

            handlerMock.VerifyAll();
            result.Should().BeSameAs(iteratorMock);
        }

        [Fact]
        public async void GetStateByRange_invokes_the_handlers_GetStateByRange_handler()
        {
            var iteratorMock = new StateQueryIterator(null, string.Empty, string.Empty, null);
            var handlerMock = new Mock<IHandler>();
            handlerMock.Setup(m =>
                    m.HandleGetStateByRange(string.Empty, "startKey", "endKey", "ChannelId", "TxId"))
                .ReturnsAsync(iteratorMock);

            handlerMock.Setup(m =>
                    m.HandleGetStateByRange(string.Empty, "\x01", "endKey", "ChannelId", "TxId"))
                .ReturnsAsync(iteratorMock);

            var stub = CreateValidChaincodeStubWithHandler(handlerMock.Object);

            var result = await stub.GetStateByRange("startKey", "endKey");
            result.Should().BeSameAs(iteratorMock);

            result = await stub.GetStateByRange("", "endKey");
            result.Should().BeSameAs(iteratorMock);

            result = await stub.GetStateByRange(null, "endKey");
            result.Should().BeSameAs(iteratorMock);

            handlerMock.Verify(m => m.HandleGetStateByRange(string.Empty, "startKey", "endKey", "ChannelId", "TxId"),
                Times.Once);
            handlerMock.Verify(m => m.HandleGetStateByRange(string.Empty, "\x01", "endKey", "ChannelId", "TxId"),
                Times.Exactly(2));
        }

        [Fact]
        public void SetEvent_throws_an_exception_when_event_name_is_null()
        {
            var sut = CreateValidChaincodeStub();
            Action act = () => sut.SetEvent(null, null);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void SetEvent_throws_an_exception_when_event_name_is_empty()
        {
            var sut = CreateValidChaincodeStub();
            Action act = () => sut.SetEvent(string.Empty, null);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void SetEvent_set_the_stubs_event_property()
        {
            var sut = CreateValidChaincodeStub();
            sut.SetEvent("event", "payload".ToByteString());

            sut.ChaincodeEvent.Should().NotBeNull();
            sut.ChaincodeEvent.EventName.Should().Be("event");
            sut.ChaincodeEvent.Payload.ToStringUtf8().Should().Be("payload");
        }

        [Fact]
        public void CreateCompositeKey_throws_an_exception_when_object_type_is_null()
        {
            var sut = CreateValidChaincodeStub();
            Action act = () => sut.CreateCompositeKey(null, null);
            act.Should().Throw<Exception>("objectType or attribute not a non-zero length string");
        }

        [Fact]
        public void CreateCompositeKey_throws_an_exception_when_object_type_is_empty()
        {
            var sut = CreateValidChaincodeStub();
            Action act = () => sut.CreateCompositeKey(string.Empty, null);
            act.Should().Throw<Exception>("objectType or attribute not a non-zero length string");
        }

        [Fact]
        public void CreateCompositeKey_returns_correct_key_when_no_attributes_are_set()
        {
            var sut = CreateValidChaincodeStub();
            sut.CreateCompositeKey("key", new List<string>()).Should().Be("\u0000key\u0000");
        }

        [Fact]
        public void CreateCompositeKey_returns_correct_key_when_one_attribute_is_set()
        {
            var sut = CreateValidChaincodeStub();
            sut.CreateCompositeKey("key", new[] {"attr1"}).Should().Be("\u0000key\u0000attr1\u0000");
        }

        [Fact]
        public void CreateCompositeKey_returns_correct_key_when_multiple_attribute_are_set()
        {
            var sut = CreateValidChaincodeStub();
            sut.CreateCompositeKey("key", new[] {"attr1", "attr2", "attr3"})
                .Should().Be("\u0000key\u0000attr1\u0000attr2\u0000attr3\u0000");
        }

        [Fact]
        public void SplitCompositeKey_should_return_empty_object_when_no_value_is_passed()
        {
            var sut = CreateValidChaincodeStub();
            var result = sut.SplitCompositeKey(null);

            result.ObjectType.Should().BeNull();
            result.Attributes.Should().BeEmpty();
        }

        [Fact]
        public void SplitCompositeKey_should_return_empty_object_when_empty_string_is_passed()
        {
            var sut = CreateValidChaincodeStub();
            var result = sut.SplitCompositeKey(string.Empty);

            result.ObjectType.Should().BeNull();
            result.Attributes.Should().BeEmpty();
        }

        [Fact]
        public void SplitCompositeKey_should_return_empty_object_when_no_delimited_string_is_passed()
        {
            var sut = CreateValidChaincodeStub();
            var result = sut.SplitCompositeKey("idonthaveadelimiter");

            result.ObjectType.Should().BeNull();
            result.Attributes.Should().BeEmpty();
        }

        [Fact]
        public void SplitCompositeKey_should_return_empty_object_when_a_incorrect_delimited_string_is_passed()
        {
            var sut = CreateValidChaincodeStub();
            var result = sut.SplitCompositeKey("something\u0000\u0101ello");

            result.ObjectType.Should().BeNull();
            result.Attributes.Should().BeEmpty();
        }

        [Fact]
        public void SplitCompositeKey_should_return_empty_object_when_only_a_delimiter_is_passed()
        {
            var sut = CreateValidChaincodeStub();
            var result = sut.SplitCompositeKey("\x00");

            result.ObjectType.Should().BeNull();
            result.Attributes.Should().BeEmpty();
        }

        [Fact]
        public void SplitCompositeKey_splits_correctly_when_only_object_type_is_passed()
        {
            var sut = CreateValidChaincodeStub();
            var result = sut.SplitCompositeKey(sut.CreateCompositeKey("key", new List<string>()));

            result.ObjectType.Should().Be("key");
            result.Attributes.Should().BeEmpty();
        }

        [Fact]
        public void SplitCompositeKey_splits_correctly_when_object_type_and_one_attribute_are_passed()
        {
            var sut = CreateValidChaincodeStub();
            var result = sut.SplitCompositeKey(sut.CreateCompositeKey("key", new[] {"attr1"}));

            result.ObjectType.Should().Be("key");
            result.Attributes.Should().Contain("attr1");
        }

        [Fact]
        public void SplitCompositeKey_splits_correctly_when_object_type_and_multiple_attributes_are_passed()
        {
            var sut = CreateValidChaincodeStub();
            var result = sut.SplitCompositeKey(sut.CreateCompositeKey("key", new[] {"attr1", "attr2", "attr3"}));

            result.ObjectType.Should().Be("key");
            result.Attributes.Should().Contain("attr1");
            result.Attributes.Should().Contain("attr2");
            result.Attributes.Should().Contain("attr3");
        }

        [Fact]
        public async void GetStateByPartialCompositeKey_returns_expected_result()
        {
            const string expectedKey = "\u0000key\u0000attr1\u0000attr2\u0000";

            var handlerMock = new Mock<IHandler>();
            handlerMock.Setup(m => m.HandleGetStateByRange(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult<StateQueryIterator>(null));

            var sut = CreateValidChaincodeStubWithHandler(handlerMock.Object);

            await sut.GetStateByPartialCompositeKey("key", new[] {"attr1", "attr2"});

            handlerMock.Verify(
                m => m.HandleGetStateByRange(string.Empty, expectedKey, expectedKey + _maxUnicodeRuneValue, "ChannelId",
                    "TxId"), Times.Once);
        }

        [Fact]
        public void Parses_arguments_correctly_when_function_name_and_parameters_are_set()
        {
            var input = new ChaincodeInput();
            input.Args.AddRange(new List<ByteString>()
            {
                "invoke".ToByteString(),
                "key".ToByteString(),
                "value".ToByteString()
            });

            var sut = CreateChaincodeStub(null, "ChannelId", "TxId", input, CreateValidSignedProposal());

            sut.Args.Should().HaveCount(3);
            sut.Args.Should().HaveElementAt(0, "invoke");
            sut.Args.Should().HaveElementAt(1, "key");
            sut.Args.Should().HaveElementAt(2, "value");

            var fap = sut.GetFunctionAndParameters();
            fap.Function.Should().Be("invoke");
            fap.Parameters.Should().HaveCount(2);
            fap.Parameters.Should().HaveElementAt(0, "key");
            fap.Parameters.Should().HaveElementAt(1, "value");
        }

        [Fact]
        public void Parses_arguments_correctly_when_function_name()
        {
            var input = new ChaincodeInput();
            input.Args.AddRange(new List<ByteString>() {"invoke".ToByteString()});

            var sut = CreateChaincodeStub(null, "ChannelId", "TxId", input, CreateValidSignedProposal());

            sut.Args.Should().HaveCount(1);
            sut.Args.Should().Contain("invoke");

            var fap = sut.GetFunctionAndParameters();
            fap.Function.Should().Be("invoke");
            fap.Parameters.Should().BeEmpty();
        }

        [Fact]
        public void Parses_arguments_correctly_no_arguments_are_set()
        {
            var sut = CreateValidChaincodeStub();

            sut.Args.Should().BeEmpty();

            var fap = sut.GetFunctionAndParameters();
            fap.Should().BeNull();
        }

        [Fact]
        public void Parses_creator_correctly()
        {
            var sut = CreateValidChaincodeStub();

            sut.Creator.Mspid.Should().Be("testMSP");
        }

        [Fact]
        public void Parses_transiert_map_correctly()
        {
            var sut = CreateValidChaincodeStub();

            sut.TransientMap.Should().ContainKey("testKey");
            sut.TransientMap["testKey"].ToStringUtf8().Should().Be("testValue");
        }

        [Fact]
        public void SignedProposal_has_correct_signature()
        {
            var signedProposal = CreateValidSignedProposal();
            signedProposal.Signature = "dummy".ToByteString();

            var sut = CreateChaincodeStub(null, "ChannelId", "TxId", new ChaincodeInput(), signedProposal);

            sut.DecodedSignedProposal.Signature.ToStringUtf8().Should().Be("dummy");
        }

        [Fact]
        public void SignedProposal_header_signature_header_nonce_is_set()
        {
            var sut = CreateValidChaincodeStub();

            sut.DecodedSignedProposal.Proposal.Header.SignatureHeader.Nonce.ToStringUtf8().Should().Be("nonce");
        }

        [Fact]
        public void SignedProposal_header_signature_header_creator_mspid_is_set()
        {
            var sut = CreateValidChaincodeStub();

            sut.DecodedSignedProposal.Proposal.Header.SignatureHeader.Creator.Mspid.Should().Be("testMSP");
        }

        [Fact]
        public void GetPrivateData_throws_an_exception_when_collection_is_not_set()
        {
            var sut = CreateValidChaincodeStub();

            sut.Invoking(s => s.GetPrivateData(string.Empty, "key"))
                .Should().Throw<Exception>("collection must be a valid string");
        }

        [Fact]
        public async void GetPrivateData_returns_a_response()
        {
            var handlerMock = new Mock<IHandler>();
            handlerMock.Setup(m => m.HandleGetState("Collection", "Key", "ChannelId", "TxId"))
                .ReturnsAsync("response".ToByteString());

            var sut = CreateValidChaincodeStubWithHandler(handlerMock.Object);

            var result = await sut.GetPrivateData("Collection", "Key");

            result.Should().NotBeNull();
            result.ToStringUtf8().Should().Be("response");

            handlerMock.VerifyAll();
        }

        [Fact]
        public void PutPrivateData_throws_an_exception_when_collection_is_not_set()
        {
            var sut = CreateValidChaincodeStub();

            sut.Invoking(s => s.PutPrivateData(string.Empty, "key", "value".ToByteString()))
                .Should().Throw<Exception>("collection must be a valid string");
        }

        [Fact]
        public async void PutPrivateData_returns_a_response()
        {
            var handlerMock = new Mock<IHandler>();
            handlerMock.Setup(m => m.HandlePutState("Collection", "Key", "Value".ToByteString(), "ChannelId", "TxId"))
                .ReturnsAsync("response".ToByteString());

            var sut = CreateValidChaincodeStubWithHandler(handlerMock.Object);

            var result = await sut.PutPrivateData("Collection", "Key", "Value".ToByteString());

            result.Should().NotBeNull();
            result.ToStringUtf8().Should().Be("response");

            handlerMock.VerifyAll();
        }

        [Fact]
        public void DeletePrivateData_throws_an_exception_when_collection_is_not_set()
        {
            var sut = CreateValidChaincodeStub();

            sut.Invoking(s => s.DeletePrivateData(string.Empty, "key"))
                .Should().Throw<Exception>("collection must be a valid string");
        }

        [Fact]
        public async void DeletePrivateData_returns_a_response()
        {
            var handlerMock = new Mock<IHandler>();
            handlerMock.Setup(m => m.HandleDeleteState("Collection", "Key", "ChannelId", "TxId"))
                .ReturnsAsync("response".ToByteString());

            var sut = CreateValidChaincodeStubWithHandler(handlerMock.Object);

            var result = await sut.DeletePrivateData("Collection", "Key");

            result.Should().NotBeNull();
            result.ToStringUtf8().Should().Be("response");

            handlerMock.VerifyAll();
        }

        [Fact]
        public async void GetPrivateDataByRange_invokes_the_handlers_GetStateByRange_handler()
        {
            var iteratorMock = new StateQueryIterator(null, string.Empty, string.Empty, null);
            var handlerMock = new Mock<IHandler>();
            handlerMock.Setup(m =>
                    m.HandleGetStateByRange(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(iteratorMock);

            var stub = CreateValidChaincodeStubWithHandler(handlerMock.Object);

            var result = await stub.GetPrivateDataByRange("collection", "startKey", "endKey");
            result.Should().BeSameAs(iteratorMock);

            result = await stub.GetPrivateDataByRange("collection", "", "endKey");
            result.Should().BeSameAs(iteratorMock);

            result = await stub.GetPrivateDataByRange("collection", null, "endKey");
            result.Should().BeSameAs(iteratorMock);

            handlerMock.Verify(m => m.HandleGetStateByRange("collection", "startKey", "endKey", "ChannelId", "TxId"),
                Times.Once);
            handlerMock.Verify(m => m.HandleGetStateByRange("collection", "\x01", "endKey", "ChannelId", "TxId"),
                Times.Exactly(2));
        }

        [Fact]
        public void GetPrivateDataByRange_throws_and_exception_when_collection_is_not_set()
        {
            var sut = CreateValidChaincodeStub();

            sut.Invoking(s => s.GetPrivateDataByRange(string.Empty, "start", "end"))
                .Should().Throw<Exception>()
                .WithMessage("collection must be a valid string");
        }

        [Fact]
        public void GetPrivateDataByPartialCompositeKey_throws_an_exception_when_collection_is_not_set()
        {
            var sut = CreateValidChaincodeStub();

            sut.Invoking(s => s.GetPrivateDataByPartialCompositeKey(string.Empty, "key", new List<string>()))
                .Should().Throw<Exception>()
                .WithMessage("collection must be a valid string");
        }

        [Fact]
        public async void GetPrivateDataByPartialCompositeKey_returns_expected_result()
        {
            const string expectedKey = "\u0000key\u0000attr1\u0000attr2\u0000";

            var handlerMock = new Mock<IHandler>();
            handlerMock.Setup(m => m.HandleGetStateByRange(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult<StateQueryIterator>(null));

            var sut = CreateValidChaincodeStubWithHandler(handlerMock.Object);

            await sut.GetPrivateDataByPartialCompositeKey("collection", "key", new[] {"attr1", "attr2"});

            handlerMock.Verify(
                m => m.HandleGetStateByRange("collection", expectedKey, expectedKey + _maxUnicodeRuneValue, "ChannelId",
                    "TxId"), Times.Once);
        }

        [Fact]
        public void GetPrivateDataQueryResult_throws_an_exception_when_collection_is_not_set()
        {
            var sut = CreateValidChaincodeStub();

            sut.Invoking(s => s.GetPrivateDataQueryResult(string.Empty, "query"))
                .Should().Throw<Exception>()
                .WithMessage("collection must be a valid string");
        }

        [Fact]
        public async void GetPrivateDataQueryResult_invokes_the_handlers_GetQueryResult_handler()
        {
            var iteratorMock = new StateQueryIterator(null, string.Empty, string.Empty, null);
            var handlerMock = new Mock<IHandler>();
            handlerMock.Setup(m =>
                    m.HandleGetQueryResult("collection", "query", "ChannelId", "TxId"))
                .ReturnsAsync(iteratorMock);

            var stub = CreateValidChaincodeStubWithHandler(handlerMock.Object);

            var result = await stub.GetPrivateDataQueryResult("collection", "query");

            handlerMock.VerifyAll();
            result.Should().BeSameAs(iteratorMock);
        }
    }
}
