using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Protos;
using Thinktecture.HyperledgerFabric.Chaincode.Handler;

namespace Thinktecture.HyperledgerFabric.Chaincode.Chaincode
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public class ChaincodeStubFactory : IChaincodeStubFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ChaincodeStubFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IChaincodeStub Create(
            IHandler handler,
            string channelId,
            string txId,
            ChaincodeInput chaincodeInput,
            SignedProposal signedProposal
        )
        {
            using (var scope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                return new ChaincodeStub(handler, channelId, txId, chaincodeInput, signedProposal,
                    scope.ServiceProvider.GetRequiredService<ILogger<ChaincodeStub>>());
            }
        }
    }
}
