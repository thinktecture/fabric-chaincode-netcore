namespace Chaincode.NET.Settings
{
    public class ChaincodeSettings
    {
        public string CORE_PEER_ADDRESS { get; set; }
        public string CORE_PEER_ID { get; set; }
        public string CORE_CHAINCODE_ID_NAME { get; set; }
        public string PeerAddress => CORE_PEER_ADDRESS;
        public string ChaincodeIdName => CORE_CHAINCODE_ID_NAME;
        public string PeerId => CORE_PEER_ID;
    }
}