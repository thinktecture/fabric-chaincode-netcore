using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.HyperledgerFabric.Chaincode;
using Thinktecture.HyperledgerFabric.Chaincode.Handler;

namespace FabCar
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var provider = ProviderConfiguration.Configure<FabCar>(args))
            {
                var shim = provider.GetRequiredService<Shim>();
                await shim.Start();
            }
        }
    }
}
