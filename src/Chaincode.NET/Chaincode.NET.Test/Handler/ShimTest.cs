using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chaincode.NET.Chaincode;
using Chaincode.NET.Handler;
using Chaincode.NET.Messaging;
using Chaincode.NET.Protos.Extensions;
using Chaincode.NET.Settings;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32.SafeHandles;
using Moq;
using Protos;
using Xunit;

namespace Chaincode.NET.Test.Handler
{
    public class ShimTest
    {
        [Fact]
        public void Creates_an_empty_success_response()
        {
            var sut = Shim.Success();

            sut.Payload.Should().BeEmpty();
            sut.Status.Should().Be((int) ResponseCodes.Ok);
        }

        [Fact]
        public void Creates_a_success_response_with_payload()
        {
            var sut = Shim.Success("payload".ToByteString());

            sut.Payload.ToStringUtf8().Should().Be("payload");
            sut.Status.Should().Be((int) ResponseCodes.Ok);
        }

        [Fact]
        public void Creates_an_error_response()
        {
            var sut = Shim.Error("foobar");

            sut.Payload.ToStringUtf8().Should().Be("foobar");
            sut.Status.Should().Be((int) ResponseCodes.Error);
        }

        [Fact]
        public void Start_throws_an_error_when_peer_address_contains_a_protocol()
        {
            var options = Options.Create(new ChaincodeSettings()
            {
                CORE_PEER_ADDRESS = "grpcs://example.test"
            });
            
            var shim = new Shim(new Mock<IChaincode>().Object, options, new Mock<IChaincodeStubFactory>().Object,
                new Mock<ILogger<Shim>>().Object, new Mock<ILogger<NET.Handler.Handler>>().Object,
                new Mock<ILogger<MessageQueue>>().Object);

            shim.Awaiting(m => m.Start())
                .Should().Throw<Exception>("Peer Address should not contain any protocol information.");
        }
        
        [Fact]
        public void Start_throws_an_error_when_peer_address_port_is_missing()
        {
            var options = Options.Create(new ChaincodeSettings()
            {
                CORE_PEER_ADDRESS = "example.test"
            });
            
            var shim = new Shim(new Mock<IChaincode>().Object, options, new Mock<IChaincodeStubFactory>().Object,
                new Mock<ILogger<Shim>>().Object, new Mock<ILogger<NET.Handler.Handler>>().Object,
                new Mock<ILogger<MessageQueue>>().Object);

            shim.Awaiting(m => m.Start())
                .Should().Throw<Exception>("Please provide peer address in the format of host:port");
        }
    }
}
