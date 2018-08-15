
[![Master Build](https://img.shields.io/travis/thinktecture/fabric-chaincode-netcore.svg?label=master)](https://travis-ci.org/thinktecture/fabric-chaincode-netcore)
[![NuGet Release](https://img.shields.io/nuget/Thinktecture.HyperledgerFabric.Chaincode.svg?label=nuget%20release)](https://www.nuget.org/packages/Thinktecture.HyperledgerFabric.Chaincode/)

[![Develop Build](https://img.shields.io/travis/thinktecture/fabric-chaincode-netcore/develop.svg?label=develop)](https://travis-ci.org/thinktecture/fabric-chaincode-netcore)
[![NuGet Pre Release](https://img.shields.io/nuget/vpre/Thinktecture.HyperledgerFabric.Chaincode.svg?label=nuget%20pre-release)](https://www.nuget.org/packages/Thinktecture.HyperledgerFabric.Chaincode/)


# Thinktecture Hyperledger Fabric Chaincode .NET Adapter

With this package you are able to build chaincode (aka "Smart Contracts") for [Hyperledger Fabric](https://hyperledger.org/projects/fabric) using .NET Core. 

> Please have in mind, that this code and it's NuGet package is heavily work in progress.

## Usage

1. Install the [NuGet Package](https://www.nuget.org/packages/Thinktecture.HyperledgerFabric.Chaincode): `Thinktecture.HyperledgerFabric.Chaincode` 
2. Create a new console application 
3. Create a new class implementing `IChaincode`
4. Take a look at the following section for the implementation of `static Main()`.

For more samples, please take a look at [examples](examples).

### Startup (C# Language Level >= 7.3)

```
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
            using (var provider = ProviderConfiguration.Configure<YourChaincodeImplementation>(args))
            {
                var shim = provider.GetRequiredService<Shim>();
                await shim.Start();
            }
        }
    }
}
```

### Startup (C# Language Level <= 7.3)

```
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.HyperledgerFabric.Chaincode;
using Thinktecture.HyperledgerFabric.Chaincode.Handler;

namespace AssetHolding
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var provider = ProviderConfiguration.Configure<YourChaincodeImplementation>(args))
            {
                var shim = provider.GetRequiredService<Shim>();
                shim.Start().Wait();
            }
        }
    }
}  
```
 

## Development

The following instructions are meant for developers of the Chaincode package.

### Folder structure

#### fabric-ccenv-netcore

The `fabric-ccenv-netcore` folder contains an adoption of the original [fabric-ccenv](https://hub.docker.com/r/hyperledger/fabric-ccenv/) which additionally installs .NET Core to run the chaincode within a Docker environment.

#### src

The `src` folder contains the code for the NuGet package. 

### Building

#### Requirements

* [.NET Core 2.1 SDK](https://www.microsoft.com/net/download)
* [Golang](https://golang.org/dl/) 

#### Build Steps

In order to build the source folder, please follow the steps:

1. Open a terminal within the `Chaincode` folder and run `dotnet restore` to restore all the packages
2. If you want to regenerate the proto files:
	1. Make sure, you have Golang and the proto files for Hyperledger Fabric installed: 
		* `go get -d github.com/hyperledger/fabric/protos` 
		* `go get -d github.com/gogo/protobuf/protobuf` 
	2. Run `generate_protos.sh` within the `src` folder. It will generate the C# classes for Hyperledger Fabric's Protofiles.
4. Open the Project with JetBrains Rider (preferred) or Visual Studio
5. Build Chaincode

### Testing

TODO
