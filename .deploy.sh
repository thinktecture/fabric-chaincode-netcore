#!/bin/bash

if [ $1 = "prerelease" ]; then
	dotnet pack src/Chaincode.NET/Chaincode.NET/Chaincode.NET.csproj --configuration Release --output ../../../package --version-suffix "prerelease"
else
	dotnet pack src/Chaincode.NET/Chaincode.NET/Chaincode.NET.csproj --configuration Release --output ../../../package
fi

dotnet nuget push package/*.nupkg --api-key $NUGET_API_KEY
