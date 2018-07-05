namespace Chaincode.NET.Settings
{
    public class ChaincodeSettings
    {
        public string SslTargetNameOverride { get; set; }
        public GrpcSettings Grpc { get; set; }
        public PeerSettings Peer { get; set; }
    }
}
