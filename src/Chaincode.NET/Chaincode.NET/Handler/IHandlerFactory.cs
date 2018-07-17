namespace Chaincode.NET.Handler
{
    public interface IHandlerFactory
    {
        IHandler Create(string host, int port);
    }
}
