#!/bin/bash

if [ $1 = "prerelease" ]; then
	dotnet pack src/Chaincode/Chaincode/Chaincode.csproj --configuration Release --output ../../../package --include-symbols --version-suffix "prerelease.$TRAVIS_BUILD_NUMBER"
else
	dotnet pack src/Chaincode/Chaincode/Chaincode.csproj --configuration Release --output ../../../package --include-symbols
fi

dotnet nuget push package/*.nupkg --source https://www.nuget.org/ --api-key $NUGET_API_KEY
