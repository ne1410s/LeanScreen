# Av

# Bundle up executable
dotnet publish Av.CliTool -p:PublishSingleFile=true -p:DebugType=Embedded -r win-x64 -c Release --sc false
dotnet publish Av.CliTool -p:PublishSingleFile=true -p:DebugType=Embedded -r linux-x64 -c Release --self-contained

# Allow execute (Linux)
sudo chmod +x AvCtl