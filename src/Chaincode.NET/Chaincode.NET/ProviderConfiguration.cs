using Chaincode.NET.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Chaincode.NET
{
    public static class ProviderConfiguration
    {
        public static ServiceProvider Configure<TChaincode>(string[] args)
            where TChaincode : class, IChaincode
        {
           var serviceCollection = new ServiceCollection();

            ConfigureLogging(serviceCollection);
            ConfigureSettings(serviceCollection, args);
            ConfigureServices<TChaincode>(serviceCollection);

            return serviceCollection.BuildServiceProvider();
        }

        private static void ConfigureServices<TChaincode>(ServiceCollection serviceCollection) 
            where TChaincode : class, IChaincode
        {
            serviceCollection.AddSingleton<Shim>();
            serviceCollection.AddSingleton<IChaincode, TChaincode>();
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
            serviceCollection.AddSingleton(new LoggerFactory()
                .AddDebug()
                .AddConsole()
            );
            serviceCollection.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace));
        }
    }
}
