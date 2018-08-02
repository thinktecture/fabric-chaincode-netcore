using System.Threading.Tasks;
using Protos;

namespace Thinktecture.HyperledgerFabric.Chaincode.Messaging
{
    public interface IMessageQueue
    {
        Task QueueMessage(QueueMessage queueMessage);
        void HandleMessageResponse(ChaincodeMessage response);
    }
}
