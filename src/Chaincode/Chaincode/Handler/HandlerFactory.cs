using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Thinktecture.HyperledgerFabric.Chaincode.Chaincode;
using Thinktecture.HyperledgerFabric.Chaincode.Messaging;

namespace Thinktecture.HyperledgerFabric.Chaincode.Handler
{
    public class HandlerFactory : IHandlerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public HandlerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IHandler Create(string host, int port)
        {
            using (var scope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                return new Handler(
                    scope.ServiceProvider.GetRequiredService<IChaincode>(),
                    host,
                    port,
                    scope.ServiceProvider.GetRequiredService<IChaincodeStubFactory>(),
                    scope.ServiceProvider.GetRequiredService<ILogger<Handler>>(),
                    scope.ServiceProvider.GetRequiredService<IMessageQueueFactory>(),
                    scope.ServiceProvider.GetRequiredService<IChaincodeSupportClientFactory>()
                );
            }
        }
    }
}