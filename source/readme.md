## Av

``` powershell
# Restore tools
dotnet tool restore

# Run unit tests (multiple test projects, no threshold)
gci **/TestResults/ | ri -r; dotnet test -c Release -s .runsettings; dotnet reportgenerator -targetdir:coveragereport -reports:**/coverage.cobertura.xml -reporttypes:"html;jsonsummary"; start coveragereport/index.html;

# Run mutation tests and show report (per project)
if (Test-Path StrykerOutput) { rm -r StrykerOutput }; dotnet stryker -o [-tp TEST_PROJECT_NAME]

# Bundle up executable
dotnet publish AvCtl -p:PublishSingleFile=true -p:DebugType=None -r win-x64 -c Release --sc false
dotnet publish AvCtl -p:PublishSingleFile=true -p:DebugType=None -r ubuntu.20.04-x64 -c Release --self-contained

# Allow execute (Linux)
sudo chmod +x AvCtl
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