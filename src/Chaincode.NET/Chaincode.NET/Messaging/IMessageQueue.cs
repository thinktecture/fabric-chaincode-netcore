using System.Threading.Tasks;
using Chaincode.NET.Protos;

namespace Thinktecture.HyperledgerFabric.Chaincode.NET.Messaging
{
    public interface IMessageQueue
    {
        Task QueueMessage(QueueMessage queueMessage);
        void HandleMessageResponse(ChaincodeMessage response);
    }
}