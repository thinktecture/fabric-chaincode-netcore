using Microsoft.Extensions.DependencyInjection;

namespace Chaincode.NET.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var providerConfiguration = ProviderConfiguration.Configure<FabCar>(args))
            {
                var shim = providerConfiguration.GetRequiredService<Shim>();
                shim.Start();    
            }
        }
    }
}
