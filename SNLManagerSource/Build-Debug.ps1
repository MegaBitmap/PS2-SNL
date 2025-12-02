
dotnet build .\SimpleNeutrinoLoaderGUI.sln
dotnet build .\SNL-CLI.sln
dotnet build .\UDPBDTray.sln

Copy-Item -Path .\NeededForRelease\* -Destination .\SNL-CLI\bin\Debug\net10.0 -Recurse -Force
Copy-Item -Path ..\SNLLua\* -Destination .\SNL-CLI\bin\Debug\net10.0\InstallFiles\SimpleNeutrinoLoader -Recurse -Force
Copy-Item -Path .\SNL-CLI\bin\Debug\net10.0\* -Destination .\SimpleNeutrinoLoaderGUI\bin\Debug\net10.0-windows7.0 -Recurse -Force
Copy-Item -Path .\UDPBDTray\bin\Debug\net10.0-windows7.0\* -Destination .\SimpleNeutrinoLoaderGUI\bin\Debug\net10.0-windows7.0 -Recurse -Force
Copy-Item -Path .\SimpleNeutrinoLoaderGUI\bin\Debug\net10.0-windows7.0\* -Destination .\UDPBDTray\bin\Debug\net10.0-windows7.0 -Recurse -Force

