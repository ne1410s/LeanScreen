## Av

``` powershell
# Restore tools
dotnet tool restore

# Run unit tests
gci **/TestResults/ | ri -r; dotnet test -c Release -s .runsettings; dotnet reportgenerator -targetdir:coveragereport -reports:**/coverage.cobertura.xml -reporttypes:"html;jsonsummary"; start coveragereport/index.html;

# Run mutation tests and show report
gci **/StrykerOutput/ | ri -r; dotnet stryker -o;

# Bundle up executable
dotnet publish Av.CliTool -p:PublishSingleFile=true -p:DebugType=Embedded -r win-x64 -c Release --sc false
dotnet publish Av.CliTool -p:PublishSingleFile=true -p:DebugType=Embedded -r linux-x64 -c Release --self-contained

# Allow execute (Linux)
sudo chmod +x AvCtl
```

## Original Project Setup
```powershell
# check dotnet versions
dotnet --list-sdks

# set dotnet version (remember to tweak pre-release, and newer versions)
dotnet new globaljson --sdk-version 8.0.202

# add a tool manifest
dotnet new tool-manifest

# add some tools
dotnet tool install dotnet-reportgenerator-globaltool
dotnet tool install dotnet-stryker
```