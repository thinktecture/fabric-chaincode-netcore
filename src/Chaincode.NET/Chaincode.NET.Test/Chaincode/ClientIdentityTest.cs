using System;
using Chaincode.NET.Chaincode;
using Chaincode.NET.Extensions;
using FluentAssertions;
using Moq;
using Msp;
using Xunit;

namespace Chaincode.NET.Test.Chaincode
{
    public class ClientIdentityTest
    {
        // Original certificates from Node.js tests
        private const string CertificateWithoutAttributes =
            "-----BEGIN CERTIFICATE-----" +
            "MIICXTCCAgSgAwIBAgIUeLy6uQnq8wwyElU/jCKRYz3tJiQwCgYIKoZIzj0EAwIw" +
            "eTELMAkGA1UEBhMCVVMxEzARBgNVBAgTCkNhbGlmb3JuaWExFjAUBgNVBAcTDVNh" +
            "biBGcmFuY2lzY28xGTAXBgNVBAoTEEludGVybmV0IFdpZGdldHMxDDAKBgNVBAsT" +
            "A1dXVzEUMBIGA1UEAxMLZXhhbXBsZS5jb20wHhcNMTcwOTA4MDAxNTAwWhcNMTgw" +
            "OTA4MDAxNTAwWjBdMQswCQYDVQQGEwJVUzEXMBUGA1UECBMOTm9ydGggQ2Fyb2xp" +
            "bmExFDASBgNVBAoTC0h5cGVybGVkZ2VyMQ8wDQYDVQQLEwZGYWJyaWMxDjAMBgNV" +
            "BAMTBWFkbWluMFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEFq/90YMuH4tWugHa" +
            "oyZtt4Mbwgv6CkBSDfYulVO1CVInw1i/k16DocQ/KSDTeTfgJxrX1Ree1tjpaodG" +
            "1wWyM6OBhTCBgjAOBgNVHQ8BAf8EBAMCB4AwDAYDVR0TAQH/BAIwADAdBgNVHQ4E" +
            "FgQUhKs/VJ9IWJd+wer6sgsgtZmxZNwwHwYDVR0jBBgwFoAUIUd4i/sLTwYWvpVr" +
            "TApzcT8zv/kwIgYDVR0RBBswGYIXQW5pbHMtTWFjQm9vay1Qcm8ubG9jYWwwCgYI" +
            "KoZIzj0EAwIDRwAwRAIgCoXaCdU8ZiRKkai0QiXJM/GL5fysLnmG2oZ6XOIdwtsC" +
            "IEmCsI8Mhrvx1doTbEOm7kmIrhQwUVDBNXCWX1t3kJVN" +
            "-----END CERTIFICATE-----";

        private const string CertificateWithAttributes =
            "-----BEGIN CERTIFICATE-----" +
            "MIIB6TCCAY+gAwIBAgIUHkmY6fRP0ANTvzaBwKCkMZZPUnUwCgYIKoZIzj0EAwIw" +
            "GzEZMBcGA1UEAxMQZmFicmljLWNhLXNlcnZlcjAeFw0xNzA5MDgwMzQyMDBaFw0x" +
            "ODA5MDgwMzQyMDBaMB4xHDAaBgNVBAMTE015VGVzdFVzZXJXaXRoQXR0cnMwWTAT" +
            "BgcqhkjOPQIBBggqhkjOPQMBBwNCAATmB1r3CdWvOOP3opB3DjJnW3CnN8q1ydiR" +
            "dzmuA6A2rXKzPIltHvYbbSqISZJubsy8gVL6GYgYXNdu69RzzFF5o4GtMIGqMA4G" +
            "A1UdDwEB/wQEAwICBDAMBgNVHRMBAf8EAjAAMB0GA1UdDgQWBBTYKLTAvJJK08OM" +
            "VGwIhjMQpo2DrjAfBgNVHSMEGDAWgBTEs/52DeLePPx1+65VhgTwu3/2ATAiBgNV" +
            "HREEGzAZghdBbmlscy1NYWNCb29rLVByby5sb2NhbDAmBggqAwQFBgcIAQQaeyJh" +
            "dHRycyI6eyJhdHRyMSI6InZhbDEifX0wCgYIKoZIzj0EAwIDSAAwRQIhAPuEqWUp" +
            "svTTvBqLR5JeQSctJuz3zaqGRqSs2iW+QB3FAiAIP0mGWKcgSGRMMBvaqaLytBYo" +
            "9v3hRt1r8j8vN0pMcg==" +
            "-----END CERTIFICATE-----";

        private const string CertificateWithLongDNs =
            "-----BEGIN CERTIFICATE-----" +
            "MIICGjCCAcCgAwIBAgIRAIPRwJHVLhHK47XK0BbFZJswCgYIKoZIzj0EAwIwczEL" +
            "MAkGA1UEBhMCVVMxEzARBgNVBAgTCkNhbGlmb3JuaWExFjAUBgNVBAcTDVNhbiBG" +
            "cmFuY2lzY28xGTAXBgNVBAoTEG9yZzIuZXhhbXBsZS5jb20xHDAaBgNVBAMTE2Nh" +
            "Lm9yZzIuZXhhbXBsZS5jb20wHhcNMTcwNjIzMTIzMzE5WhcNMjcwNjIxMTIzMzE5" +
            "WjBbMQswCQYDVQQGEwJVUzETMBEGA1UECBMKQ2FsaWZvcm5pYTEWMBQGA1UEBxMN" +
            "U2FuIEZyYW5jaXNjbzEfMB0GA1UEAwwWVXNlcjFAb3JnMi5leGFtcGxlLmNvbTBZ" +
            "MBMGByqGSM49AgEGCCqGSM49AwEHA0IABBd9SsEiFH1/JIb3qMEPLR2dygokFVKW" +
            "eINcB0Ni4TBRkfIWWUJeCANTUY11Pm/+5gs+fBTqBz8M2UzpJDVX7+2jTTBLMA4G" +
            "A1UdDwEB/wQEAwIHgDAMBgNVHRMBAf8EAjAAMCsGA1UdIwQkMCKAIKfUfvpGproH" +
            "cwyFD+0sE3XfJzYNcif0jNwvgOUFZ4AFMAoGCCqGSM49BAMCA0gAMEUCIQC8NIMw" +
            "e4ym/QRwCJb5umbONNLSVQuEpnPsJrM/ssBPvgIgQpe2oYa3yO3USro9nBHjpM3L" +
            "KsFQrpVnF8O6hoHOYZQ=" +
            "-----END CERTIFICATE-----";

        private Mock<IChaincodeStub> CreateChaincodeStubMock(string certificate)
        {
            var mock = new Mock<IChaincodeStub>();
            mock.Setup(m => m.Creator)
                .Returns(new SerializedIdentity()
                {
                    Mspid = "dummyId",
                    IdBytes = certificate.ToByteString()
                });

            return mock;
        }

        [Fact]
        public void ClientIdentity_with_valid_certificate_and_attributes_is_loaded_correctly()
        {
            var stubMock = CreateChaincodeStubMock(CertificateWithAttributes);

            var sut = new ClientIdentity(stubMock.Object);

            sut.Mspid.Should().Be("dummyId");
            sut.Id.Should().Be("x509::/CN=MyTestUserWithAttrs::/CN=fabric-ca-server");
            sut.X509Certificate.SerialNumber.Should().Be("1E4998E9F44FD00353BF3681C0A0A431964F5275");
            sut.Attributes["attr1"].Should().Be("val1");
            sut.GetAttributeValue("attr1").Should().Be("val1");
            sut.GetAttributeValue("unknown").Should().BeNullOrEmpty();
            sut.AssertAttributeValue("attr1", "val1").Should().BeTrue();
            sut.AssertAttributeValue("unknown", "val1").Should().BeFalse();
            sut.AssertAttributeValue("attr1", "wrongValue").Should().BeFalse();
        }

        [Fact]
        public void ClientIdentity_with_valid_certificate_without_attributes_is_loaded_correctly()
        {
            var stubMock = CreateChaincodeStubMock(CertificateWithoutAttributes);

            var sut = new ClientIdentity(stubMock.Object);

            sut.Mspid.Should().Be("dummyId");
            sut.Attributes.Count.Should().Be(0);
        }

        [Fact]
        public void ClientIdentity_with_valid_certificate_and_long_dns_is_loaded_correctly()
        {
            var stubMock = CreateChaincodeStubMock(CertificateWithLongDNs);

            var sut = new ClientIdentity(stubMock.Object);

            // TODO: Check if this is ok: The format of the subject string differs from Node.js
            sut.Mspid.Should().Be("dummyId");
            sut.Id.Should()
                .Be(
                    "x509::/CN=User1@org2.example.com, L=San Francisco, S=California, C=US::/CN=ca.org2.example.com, O=org2.example.com, L=San Francisco, S=California, C=US");
        }

        [Fact]
        public void ClientIdentity_throws_an_error_when_certificate_is_empty()
        {
            var stubMock = CreateChaincodeStubMock(string.Empty);

            Action action = () => new ClientIdentity(stubMock.Object);

            action.Should().Throw<Exception>()
                .WithMessage("Failed to find start line or end line of the certificate.");
        }

        [Fact]
        public void ClientIdentity_throws_an_error_when_certificate_begin_line_is_missing()
        {
            var stubMock = CreateChaincodeStubMock("e4ym/QRwCJb5umbONNLSVQuEpnPsJrM/ssBPvgIgQpe2oYa3yO3USro9nBHjpM3L" +
                                                   "KsFQrpVnF8O6hoHOYZQ=" +
                                                   "-----END CERTIFICATE-----");

            Action action = () => new ClientIdentity(stubMock.Object);

            action.Should().Throw<Exception>()
                .WithMessage("Failed to find start line or end line of the certificate.");
        }

        [Fact]
        public void ClientIdentity_throws_an_error_when_certificate_end_line_is_missing()
        {
            var stubMock = CreateChaincodeStubMock("-----BEGIN CERTIFICATE-----" +
                                                   "e4ym/QRwCJb5umbONNLSVQuEpnPsJrM/ssBPvgIgQpe2oYa3yO3USro9nBHjpM3L" +
                                                   "KsFQrpVnF8O6hoHOYZQ=");

            Action action = () => new ClientIdentity(stubMock.Object);

            action.Should().Throw<Exception>()
                .WithMessage("Failed to find start line or end line of the certificate.");
        }
    }
}
