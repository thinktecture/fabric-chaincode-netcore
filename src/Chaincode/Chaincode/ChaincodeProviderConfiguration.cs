using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Thinktecture.HyperledgerFabric.Chaincode.Chaincode;
using Thinktecture.HyperledgerFabric.Chaincode.Contract;
using Thinktecture.HyperledgerFabric.Chaincode.Handler;
using Thinktecture.HyperledgerFabric.Chaincode.Messaging;
using Thinktecture.HyperledgerFabric.Chaincode.Settings;
using Thinktecture.IO;
using Thinktecture.IO.Adapters;

namespace Thinktecture.HyperledgerFabric.Chaincode
{
    public static class ChaincodeProviderConfiguration
    {
        public static ServiceProvider ConfigureWithContracts<TContract>(string[] args)
            where TContract : class, IContract
        {
            return ConfigureWithContracts(args, new[] {typeof(TContract)});
        }

        public static ServiceProvider ConfigureWithContracts<TContract, TContract2>(string[] args)
            where TContract : class, IContract
            where TContract2 : class, IContract
        {
            return ConfigureWithContracts(args, new[] {typeof(TContract), typeof(TContract2)});
        }

        public static ServiceProvider ConfigureWithContracts<TContract, TContract2, TContract3>(string[] args)
            where TContract : class, IContract
            where TContract2 : class, IContract
            where TContract3 : class, IContract
        {
            return ConfigureWithContracts(args, new[] {typeof(TContract), typeof(TContract2), typeof(TContract3)});
        }

        public static ServiceProvider ConfigureWithContracts<TContract, TContract2, TContract3, TContract4>(
            string[] args
        )
            where TContract : class, IContract
            where TContract2 : class, IContract
            where TContract3 : class, IContract
            where TContract4 : class, IContract
        {
            return ConfigureWithContracts(args,
                new[] {typeof(TContract), typeof(TContract2), typeof(TContract3), typeof(TContract4)});
        }

        public static ServiceProvider ConfigureWithContracts(string[] args, IEnumerable<Type> contracts)
        {
            return Configure<ChaincodeFromContracts>(args, serviceCollection =>
            {
                serviceCollection.AddSingleton<IContractContextFactory, ContractContextFactory>();

                foreach (var contract in contracts)
                {
                    if (!contract.GetInterfaces().Contains(typeof(IContract)))
                    {
                        throw new Exception($"{contract.Name} does not implement Interface IContract");
                    }

                    serviceCollection.AddSingleton(typeof(IContract), contract);
                }
            });
        }

        public static ServiceProvider Configure<TChaincode>(string[] args)
            where TChaincode : class, IChaincode
        {
            return Configure<TChaincode>(args, null);
        }

        /// <summary>
        /// Configures the .NET Core dependency injection to use the given <see cref="TChaincode"/> as chaincode
        /// for starting up the Chaincode shim.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <param name="setup">Optional setup action to add own services to the dependency collection to be used in
        /// the chaincode implementation.</param>
        /// <typeparam name="TChaincode">An implementation of <see cref="IChaincode"/>.</typeparam>
        /// <returns></returns>
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
            serviceCollection.AddSingleton<IClientIdentityFactory, ClientIdentityFactory>();
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
