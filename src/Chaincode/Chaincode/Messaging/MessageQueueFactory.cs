using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Thinktecture.HyperledgerFabric.Chaincode.Handler;

namespace Thinktecture.HyperledgerFabric.Chaincode.Messaging
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
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
