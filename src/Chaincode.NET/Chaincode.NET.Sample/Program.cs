using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.HyperledgerFabric.Chaincode.NET.Handler;

namespace Thinktecture.HyperledgerFabric.Chaincode.NET.Sample
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            using (var providerConfiguration = ProviderConfiguration.Configure<FabCar>(args))
            {
                var shim = providerConfiguration.GetRequiredService<Shim>();
                await shim.Start();
            }
        }
    }
}
