
dotnet build .\SimpleNeutrinoLoaderGUI.sln
dotnet build .\SNL-CLI.sln
dotnet build .\UDPBDTray.sln

Copy-Item -Path .\NeededForRelease\* -Destination .\SNL-CLI\bin\Debug\net8.0 -Recurse -Force
Copy-Item -Path ..\SNLLua\* -Destination .\SNL-CLI\bin\Debug\net8.0\InstallFiles\SimpleNeutrinoLoader -Recurse -Force
Copy-Item -Path .\SNL-CLI\bin\Debug\net8.0\* -Destination .\SimpleNeutrinoLoaderGUI\bin\Debug\net8.0-windows -Recurse -Force
Copy-Item -Path .\UDPBDTray\bin\Debug\net8.0-windows\* -Destination .\SimpleNeutrinoLoaderGUI\bin\Debug\net8.0-windows -Recurse -Force
Copy-Item -Path .\SimpleNeutrinoLoaderGUI\bin\Debug\net8.0-windows\* -Destination .\UDPBDTray\bin\Debug\net8.0-windows -Recurse -Force

