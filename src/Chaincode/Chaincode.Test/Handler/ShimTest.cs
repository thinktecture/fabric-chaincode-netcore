using System;
using System.Threading.Tasks;
using FluentAssertions;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Protos;
using Thinktecture.HyperledgerFabric.Chaincode.Chaincode;
using Thinktecture.HyperledgerFabric.Chaincode.Extensions;
using Thinktecture.HyperledgerFabric.Chaincode.Handler;
using Thinktecture.HyperledgerFabric.Chaincode.Settings;
using Thinktecture.IO;
using Xunit;

namespace Thinktecture.HyperledgerFabric.Chaincode.Sample.Handler
{
    public class ShimTest
    {
        [Fact]
        public void Creates_a_success_response_with_payload()
        {
            var sut = Shim.Success("payload".ToByteString());

            sut.Payload.ToStringUtf8().Should().Be("payload");
            sut.Status.Should().Be((int) ResponseCodes.Ok);
        }

        [Fact]
        public void Creates_an_empty_success_response()
        {
            var sut = Shim.Success();

            sut.Payload.Should().BeEmpty();
            sut.Status.Should().Be((int) ResponseCodes.Ok);
        }

        [Fact]
        public void Creates_an_error_response()
        {
            var sut = Shim.Error("foobar");

            sut.Message.Should().Be("foobar");
            sut.Status.Should().Be((int) ResponseCodes.Error);
        }

        [Fact]
        public void GrpcEnvironment_sets_logger_to_console_if_option_is_set()
        {
            var options = Options.Create(new ChaincodeSettings
            {
                CORE_PEER_ADDRESS = "example.test:9999",
                CORE_CHAINCODE_ID_NAME = "unittest",
                CORE_LOG_GRPC = true
            });

            var _ = new Shim(options, new Mock<ILogger<Shim>>().Object, new Mock<IHandlerFactory>().Object, null);

            GrpcEnvironment.Logger.Should().BeOfType<ConsoleLogger>();
        }

        [Fact]
        public async void Start_calls_the_handlers_chat_method()
        {
            var options = Options.Create(new ChaincodeSettings
            {
                CORE_PEER_ADDRESS = "example.test:9999",
                CORE_CHAINCODE_ID_NAME = "unittest"
            });

            var message = new ChaincodeMessage
            {
                Type = ChaincodeMessage.Types.Type.Register,
                Payload = new ChaincodeID {Name = "unittest"}.ToByteString()
            };

            var handlerMock = new Mock<IHandler>();
            handlerMock.Setup(m => m.Chat(message)).Returns(Task.CompletedTask);

            var handlerFactoryMock = new Mock<IHandlerFactory>();
            handlerFactoryMock.Setup(m => m.Create("example.test", 9999, ChannelCredentials.Insecure))
                .Returns(handlerMock.Object);

            var shim = new Shim(options, new Mock<ILogger<Shim>>().Object, handlerFactoryMock.Object, null);
            var result = await shim.Start();

            result.Should().BeSameAs(handlerMock.Object);

            handlerFactoryMock.VerifyAll();
            handlerMock.VerifyAll();
        }

        [Fact]
        public void Start_throws_an_error_when_peer_address_contains_a_protocol()
        {
            var options = Options.Create(new ChaincodeSettings
            {
                CORE_PEER_ADDRESS = "grpcs://example.test"
            });

            var shim = new Shim(options, new Mock<ILogger<Shim>>().Object, new Mock<IHandlerFactory>().Object, null);

            shim.Awaiting(m => m.Start())
                .Should().Throw<Exception>("Peer Address should not contain any protocol information.");
        }

        [Fact]
        public void Start_throws_an_error_when_peer_address_port_is_missing()
        {
            var options = Options.Create(new ChaincodeSettings
            {
                CORE_PEER_ADDRESS = "example.test"
            });

            var shim = new Shim(options, new Mock<ILogger<Shim>>().Object, new Mock<IHandlerFactory>().Object, null);

            shim.Awaiting(m => m.Start())
                .Should().Throw<Exception>("Please provide peer address in the format of host:port");
        }

        [Fact]
        public void Start_throws_an_error_when_PeerTlsRootCertificateFilePath_points_to_a_non_existing_file()
        {
            var options = Options.Create(new ChaincodeSettings()
            {
                CORE_PEER_TLS_ROOTCERT_FILE = "foobar"
            });

            var fileMock = new Mock<IFile>();
            fileMock.Setup(m => m.Exists(It.IsAny<string>())).Returns(false);
            
            var shim = new Shim(options, null, null, fileMock.Object);
            shim.Awaiting(m => m.Start())
                .Should().Throw<Exception>("Could not locate file for environment variable CORE_PEER_TLS_ROOTCERT_FILE");
        }
        
        [Fact]
        public void Start_throws_an_error_when_TlsClientKeyFilePath_points_to_a_non_existing_file()
        {
            var options = Options.Create(new ChaincodeSettings()
            {
                CORE_TLS_CLIENT_KEY_PATH = "foobar"
            });

            var fileMock = new Mock<IFile>();
            fileMock.SetupSequence(m => m.Exists(It.IsAny<string>()))
                .Returns(true)
                .Returns(false);
            
            var shim = new Shim(options, null, null, fileMock.Object);
            shim.Awaiting(m => m.Start())
                .Should().Throw<Exception>("Could not locate file for environment variable CORE_TLS_CLIENT_KEY_PATH");
        }
        
        [Fact]
        public void Start_throws_an_error_when_TlsClientCertFilePath_points_to_a_non_existing_file()
        {
            var options = Options.Create(new ChaincodeSettings()
            {
                CORE_TLS_CLIENT_CERT_PATH = "foobar"
            });

            var fileMock = new Mock<IFile>();
            fileMock.SetupSequence(m => m.Exists(It.IsAny<string>()))
                .Returns(true)
                .Returns(true)
                .Returns(false);
            
            var shim = new Shim(options, null, null, fileMock.Object);
            shim.Awaiting(m => m.Start())
                .Should().Throw<Exception>("Could not locate file for environment variable CORE_TLS_CLIENT_CERT_PATH");
        }
    }
}
