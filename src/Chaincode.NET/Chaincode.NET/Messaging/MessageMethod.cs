namespace Chaincode.NET.Messaging
{
    public enum MessageMethod
    {
        GetState,
        GetStateByRange,
        GetQueryResult,
        GetHistoryForKey,
        QueryStateNext,
        QueryStateClose,
        InvokeChaincode,
        PutState,
        DelState
    }
}