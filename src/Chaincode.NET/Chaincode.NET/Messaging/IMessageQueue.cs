using System.Threading.Tasks;
using Protos;

namespace Chaincode.NET.Messaging
{
    public interface IMessageQueue
    {
        Task QueueMessage(QueueMessage queueMessage);
        void HandleMessageResponse(ChaincodeMessage response);
    }
}
