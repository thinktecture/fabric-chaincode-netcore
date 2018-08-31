using FluentAssertions;
using Thinktecture.HyperledgerFabric.Chaincode.Settings;
using Xunit;

namespace Thinktecture.HyperledgerFabric.Chaincode.Test.Settings
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

        [Fact]
        public void PeerTlsRootCertificateFilePath_maps_to_external_CORE_PEER_TLS_ROOTCERT_FILE()
        {
            var sut = new ChaincodeSettings {CORE_PEER_TLS_ROOTCERT_FILE = "foobar"};
            sut.PeerTlsRootCertificateFilePath.Should().Be("foobar");
        }
        
        [Fact]
        public void TlsClientKeyFilePath_maps_to_external_CORE_TLS_CLIENT_KEY_PATH()
        {
            var sut = new ChaincodeSettings {CORE_TLS_CLIENT_KEY_PATH = "foobar"};
            sut.TlsClientKeyFilePath.Should().Be("foobar");
        }
        
        [Fact]
        public void TlsClientCertFilePath_maps_to_external_CORE_TLS_CLIENT_CERT_PATH()
        {
            var sut = new ChaincodeSettings {CORE_TLS_CLIENT_CERT_PATH = "foobar"};
            sut.TlsClientCertFilePath.Should().Be("foobar");
        }
    }
}
