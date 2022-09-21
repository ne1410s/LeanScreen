## Av

``` powershell
# Restore tools
dotnet tool restore

# Run unit tests and show coverage report (single test project, with threshold)
#dotnet build -c Release -o bin; dotnet coverlet bin/Av.Tests.dll -t dotnet -a "test bin/Av.Tests.dll -c Release --no-build" --threshold 100 -f cobertura -o TestResults/coverage; dotnet reportgenerator -targetdir:coveragereport -reports:**/coverage.cobertura.xml -reporttypes:"html"; start coveragereport/index.html;

# Run unit tests (multiple test projects, no threshold)
gci **/TestResults/ | ri -r; dotnet test -c Release --collect:"XPlat Code Coverage"; dotnet reportgenerator -targetdir:coveragereport -reports:**/coverage.cobertura.xml -reporttypes:"html"; start coveragereport/index.html;

# Run mutation tests and show report
if (Test-Path StrykerOutput) { rm -r StrykerOutput }; dotnet stryker -o

# Bundle up executable
dotnet publish AvCtl -p:PublishSingleFile=true -p:DebugType=None -r win-x64 -c Release --sc false
```

## Original Project Setup
```powershell
# check dotnet versions
dotnet --list-sdks

# set dotnet version (remember to tweak pre-release, and newer versions)
dotnet new globaljson --sdk-version 6.0.400

# add a tool manifest
dotnet new tool-manifest

# add some tools
dotnet tool install coverlet.console
dotnet tool install dotnet-reportgenerator-globaltool
dotnet tool install dotnet-stryker
```