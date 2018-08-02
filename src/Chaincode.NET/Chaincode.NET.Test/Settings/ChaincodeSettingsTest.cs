using Chaincode.NET.Settings;
using FluentAssertions;
using Xunit;

namespace Chaincode.NET.Test.Settings
{
    public class ChaincodeSettingsTest
    {
        [Fact]
        public void ChaincodeIdName_maps_to_external_CORE_CHAINCODE_ID_NAME()
        {
            var sut = new ChaincodeSettings {CORE_CHAINCODE_ID_NAME = "foobar"};
            sut.ChaincodeIdName.Should().Be("foobar");
        }

        [Fact]
        public void PeerAddress_maps_to_external_CORE_PEER_ADDRESS()
        {
            var sut = new ChaincodeSettings {CORE_PEER_ADDRESS = "foobar"};
            sut.PeerAddress.Should().Be("foobar");
        }

        [Fact]
        public void PeerId_maps_to_external_CORE_PEER_ID()
        {
            var sut = new ChaincodeSettings {CORE_PEER_ID = "foobar"};
            sut.PeerId.Should().Be("foobar");
        }
    }
}