
[![Master Build](https://img.shields.io/travis/thinktecture/fabric-chaincode-netcore.svg?label=master)](https://travis-ci.org/thinktecture/fabric-chaincode-netcore)
[![NuGet Release](https://img.shields.io/nuget/v/Thinktecture.HyperledgerFabric.Chaincode.svg?label=nuget%20release)](https://www.nuget.org/packages/Thinktecture.HyperledgerFabric.Chaincode/)
[![Coverage Status](https://coveralls.io/repos/github/thinktecture/fabric-chaincode-netcore/badge.svg?branch=)](https://coveralls.io/github/thinktecture/fabric-chaincode-netcore?branch=master)

[![Develop Build](https://img.shields.io/travis/thinktecture/fabric-chaincode-netcore/develop.svg?label=develop)](https://travis-ci.org/thinktecture/fabric-chaincode-netcore)
[![NuGet Pre Release](https://img.shields.io/nuget/vpre/Thinktecture.HyperledgerFabric.Chaincode.svg?label=nuget%20pre-release)](https://www.nuget.org/packages/Thinktecture.HyperledgerFabric.Chaincode/)
[![Coverage Status](https://coveralls.io/repos/github/thinktecture/fabric-chaincode-netcore/badge.svg?branch=)](https://coveralls.io/github/thinktecture/fabric-chaincode-netcore?branch=develop)


# Thinktecture Hyperledger Fabric Chaincode .NET Adapter

With this package you are able to build chaincode (aka "Smart Contracts") for [Hyperledger Fabric](https://hyperledger.org/projects/fabric) using .NET Core. 

> Please have in mind, that this code and it's NuGet package is heavily work in progress.

## Usage

1. Install the [NuGet Package](https://www.nuget.org/packages/Thinktecture.HyperledgerFabric.Chaincode): `Thinktecture.HyperledgerFabric.Chaincode` 
2. Create a new console application 
3. Decide, if you want to use the low level API or the higher level API

### IChaincode (Low Level API)

If you implemented `IChaincode` you are using the low level API giving you full access to chaincode development.

1. Create a new class implementing `IChaincode`
2. Take a look at the following section for the implementation of `static Main()`.

For more samples, please take a look at [examples](#samples).

#### Startup (C# Language Level >= 7.0)

```csharp
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

#### Startup (C# Language Level < 7.1)

```csharp
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

### `IContract`/`ContractBase` (High Level API)

If you want to use a higher level API, please consider implementing `IContract` instead of `IChaincode`.
For your convenience, you can use `ContractBase` to extend from to get some basic functionality, like namespaces.
The difference between `IContract` and `IChaincode` is, that within `IChaincode` you have to do the routing to your methods by yourself given a key string identifier and map the parameters to your method. 
Depending on your usecase, you get a lot of bootstrap/infrastructure code.

With `IContract` the library does that for you by using reflection to get all exectuble methods within your contract. 
Executable means:

* is public accessible
* has `Task<ByteString>` as a return value

The signature of the method is: 

```
public [async] Task<ByteString> MethodName(IContractContext context [, string param1]*)
```

The first parameter is always of type `IContractContext`. 
Then optional, string parameter only, paramters can follow.

If no method with those specification can be found, booting the Chaincode will result in an exception.

It's heavily recommended to take a look at the [samples](#samples) to see the difference between those two implementation methods. 

#### Startup

For startup, it's basically the same as with the `IChaincode` method, but the line with `using` will be different:

```csharp
using (var provider = ProviderConfiguration.ConfigureWithContracts<YourContractImplementation>(args))
```

The generic version of `ConfigureWithContracts` supports up to four contracts.
After that, please use the non-generic version:

```csharp
using (var provider = ProviderConfiguration.ConfigureWithContracts(args, new [] { typeof(YourContractImplementation) }))
```

## Samples

The samples contain both `IChaincode` and `IContract` implementations.

* [FabCar](https://github.com/thinktecture/hlf-sample-fabcar-netcore)
* [Asset Holding](https://github.com/thinktecture/hlf-sample-asset-holding-netcore)
* [Number Porting](https://github.com/thinktecture/hlf-sample-number-porting-netcore)

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

The library uses xUnit to unit test most of the code. 
A coverage of 80 % should tried to be achieved. 
For the coverage, [coverlet](https://github.com/tonerdo/coverlet) is used.
To test for coverage locally, execute:

```bash
dotnet test src/Chaincode/Chaincode.Test/Chaincode.Test.csproj --configuration Release /p:CollectCoverage=true /p:Exclude="[Chaincode.Protos]*" /p:CoverletOutputFormat=opencover
```
