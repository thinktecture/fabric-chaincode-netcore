using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.HyperledgerFabric.Chaincode;
using Thinktecture.HyperledgerFabric.Chaincode.Handler;

namespace AssetHolding
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var provider = ChaincodeProviderConfiguration.Configure<AssetHolding>(args))
            {
                var shim = provider.GetRequiredService<Shim>();
                await shim.Start();
            }
        }
    }
}
