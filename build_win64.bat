@echo off

dotnet publish drpc.csproj -c Release -r win-x64 -p:PublishSingleFile=true --self-contained true

pause
