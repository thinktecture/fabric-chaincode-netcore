using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Thinktecture.HyperledgerFabric.Chaincode.Chaincode;
using Thinktecture.HyperledgerFabric.Chaincode.Handler;
using Thinktecture.HyperledgerFabric.Chaincode.Messaging;
using Thinktecture.HyperledgerFabric.Chaincode.Settings;
using Thinktecture.IO;
using Thinktecture.IO.Adapters;

namespace Thinktecture.HyperledgerFabric.Chaincode
{
    public static class ChaincodeProviderConfiguration
    {
        public static ServiceProvider Configure<TChaincode>(string[] args)
            where TChaincode : class, IChaincode
        {
            return Configure<TChaincode>(args, null);
        }

        public static ServiceProvider Configure<TChaincode>(string[] args, Action<ServiceCollection> setup)
            where TChaincode : class, IChaincode
        {
            var serviceCollection = new ServiceCollection();

            ConfigureLogging(serviceCollection);
            ConfigureSettings(serviceCollection, args);
            ConfigureServices<TChaincode>(serviceCollection);

            setup?.Invoke(serviceCollection);

            return serviceCollection.BuildServiceProvider();
        }

        private static void ConfigureServices<TChaincode>(ServiceCollection serviceCollection)
            where TChaincode : class, IChaincode
        {
            serviceCollection.AddSingleton<Shim>();
            serviceCollection.AddSingleton<IChaincode, TChaincode>();
            serviceCollection.AddSingleton<IMessageQueue, MessageQueue>();
            serviceCollection.AddSingleton<IChaincodeStubFactory, ChaincodeStubFactory>();
            serviceCollection.AddSingleton<IHandlerFactory, HandlerFactory>();
            serviceCollection.AddSingleton<IMessageQueueFactory, MessageQueueFactory>();
            serviceCollection.AddSingleton<IChaincodeSupportClientFactory, ChaincodeSupportClientFactory>();
            serviceCollection.AddSingleton<IFile, FileAdapter>();
        }

        private static void ConfigureSettings(ServiceCollection serviceCollection, string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            serviceCollection.AddOptions();
            serviceCollection.Configure<ChaincodeSettings>(configuration);
        }

        private static void ConfigureLogging(ServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging(builder =>
                builder.SetMinimumLevel(LogLevel.Trace)
                    .AddDebug()
                    .AddConsole()
            );
        }
    }
}
