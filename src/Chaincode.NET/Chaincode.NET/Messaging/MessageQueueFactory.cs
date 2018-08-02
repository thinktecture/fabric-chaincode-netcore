using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Thinktecture.HyperledgerFabric.Chaincode.NET.Handler;

namespace Thinktecture.HyperledgerFabric.Chaincode.NET.Messaging
{
    public class MessageQueueFactory : IMessageQueueFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public MessageQueueFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IMessageQueue Create(IHandler handler)
        {
            using (var scope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                return new MessageQueue(handler, scope.ServiceProvider.GetRequiredService<ILogger<MessageQueue>>());
            }
        }
    }
}