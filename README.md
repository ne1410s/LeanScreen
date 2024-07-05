# LeanScreen

[![Coverage Status](https://coveralls.io/repos/github/ne1410s/LeanScreen/badge.svg?branch=main)](https://coveralls.io/github/ne1410s/LeanScreen?branch=main)

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/d229dd8acd714205a1473f7406f46a28)](https://app.codacy.com/gh/ne1410s/LeanScreen/dashboard)

[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2Fne1410s%2FLeanScreen%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/ne1410s/LeanScreen/main)

## Overview
Provides a set of tools for working with multimedia. Some key features:
- Extract video data and stills, directly from stream with managed .NET code!
- Compiling a collection of stills into a single file
- Some basic media organisation capability

The **Extensions** package provides default implementation for video and image handling (ffmpeg and sixlabors respectively)..
However the abstractions package is designed so that you can roll your own if you so choose.

The calling assembly needs to ship the ffmpeg binaries in the folder `ffmpeg` in its build output.
The x64 versions have already been sourced for both linux and windows and are shipped in the Rendering.Ffmpeg project.

In the case of linux, files in the format `..lib<BINARY>.so.<VERSION>` were obtained from the ffmpeg package and have just been renamed `<BINARY>.<VERSION>`.

In the case of windows, the dlls were obtained from here: https://www.gyan.dev/ffmpeg/builds/

The extensions are exposed to the command line in the form of a Cli tool named `leanctl`.
(NB: This tool is currently not published anywhere, and needs to be built from source - see the `dotnet publish ...` commands below for reference).
## Further Notes
### Handy Commandies
```powershell
# Restore tools
dotnet tool restore

# General clean up
rd -r **/bin/; rd -r **/obj/;

# Run unit tests
rd -r ../**/TestResults/; dotnet test -c Release -s .runsettings; dotnet reportgenerator -targetdir:coveragereport -reports:**/coverage.cobertura.xml -reporttypes:"html;jsonsummary"; start coveragereport/index.html;

# Run mutation tests
rd -r ../**/StrykerOutput/; dotnet stryker -o;

# Pack and publish a pre-release to a local feed
$suffix="alpha001"; dotnet pack -c Release -o nu --version-suffix $suffix; dotnet nuget push "nu\*.*$suffix.nupkg" --source localdev; rd -r ../**/nu/;

# Publish examples
dotnet publish LeanScreen.CliTool -p:PublishSingleFile=true -p:DebugType=Embedded -r win-x64 -c Release --sc false
dotnet publish LeanScreen.CliTool -p:PublishSingleFile=true -p:DebugType=Embedded -r linux-x64 -c Release --self-contained

# Allow execute (Linux)
sudo chmod +x leanctl
```
