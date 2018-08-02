using System.Threading.Tasks;
using Chaincode.NET.Handler;
using Microsoft.Extensions.DependencyInjection;

namespace Chaincode.NET.Sample
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
