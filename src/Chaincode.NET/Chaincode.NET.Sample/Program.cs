using System.Threading.Tasks;
using Chaincode.NET.Handler;
using Chaincode.NET.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chaincode.NET.Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var providerConfiguration = ProviderConfiguration.Configure<AssetHolding>(args))
            {
                var config = providerConfiguration.GetRequiredService<IOptions<ChaincodeSettings>>();
                var logger = providerConfiguration.GetRequiredService<ILogger<Program>>();
                logger.LogDebug($"Peer Address: {config.Value.PeerAddress}");

                var shim = providerConfiguration.GetRequiredService<Shim>();
                await shim.Start();
            }
        }
    }
}
