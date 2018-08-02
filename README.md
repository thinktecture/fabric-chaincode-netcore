[![Build Status](https://travis-ci.org/thinktecture/fabric-chaincode-netcore.svg?branch=master)](https://travis-ci.org/thinktecture/fabric-chaincode-netcore)
[![Build Status](https://travis-ci.org/thinktecture/fabric-chaincode-netcore.svg?branch=develop)](https://travis-ci.org/thinktecture/fabric-chaincode-netcore)



# Thinktecture Hyperledger Fabric Chaincode .NET Adapter

With this package you are able to build chaincode (aka "Smart Contracts") for [Hyperledger Fabric](https://hyperledger.org/projects/fabric) using .NET Core. 

## Usage

## Development

The following instructions are meant for developers of the Chaincode.NET package.

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

1. Open a terminal within the `Chaincode.NET` folder and run `dotnet restore` to restore all the packages
2. Make sure, you have Golang and the proto files for Hyperledger Fabric installed: `go get github.com/hyperledger/fabric/protos` 
3. Run `generate_protos.sh` within the `src` folder. It will generate the C# classes for Hyperledger Fabric's Protofiles.
4. Open the Project with JetBrains Rider (preferred) or Visual Studio
5. Build Chaincode.NET

### Testing

TODO
