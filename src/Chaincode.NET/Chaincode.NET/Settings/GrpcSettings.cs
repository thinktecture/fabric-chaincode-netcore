namespace Chaincode.NET.Settings
{
    public class GrpcSettings
    {
        public int MaxSendMessageLength { get; set; } = -1;
        public int MaxReceiveMessageLength { get; set; } = -1;
    }
}
