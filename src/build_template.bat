@echo off

set version=ENTER_VERSION_HERE 
:: example 1.0.0

set name=Aco228.SimpleHttpClient

set api_key=ENTER_KEY_HERE
:: from nudget


:: dotnet
dotnet pack --output ./build -p:PackageID=%name% -p:PackageVersion=%version%
dotnet nuget push build/%name%.%version%.nupkg --api-key %api_key% --source https://api.nuget.org/v3/index.json
 