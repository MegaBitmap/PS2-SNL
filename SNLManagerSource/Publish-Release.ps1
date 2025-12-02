
$ReleaseVersion = (Get-Content -Path ".\SNL-CLI\SNL-CLI.csproj" | Select-String -Pattern AssemblyVersion).ToString().Trim() -replace "<[^>]+>"
$ReleaseFolder = ".\SimpleNeutrinoLoaderGUI\bin\Release\net10.0-windows7.0\publish\release-$ReleaseVersion"
$SNLManagerFolder = "$ReleaseFolder\SNL Manager (UDPBD)"

dotnet publish ".\SNL-CLI.sln"
dotnet publish ".\SimpleNeutrinoLoaderGUI.sln"
dotnet publish ".\UDPBDTray.sln"

New-Item -ItemType Directory -Path $SNLManagerFolder

Get-ChildItem -File -Path ".\SNL-CLI\bin\Release\net10.0\publish\*" | Move-Item -Destination $SNLManagerFolder
Get-ChildItem -File -Path ".\SimpleNeutrinoLoaderGUI\bin\Release\net10.0-windows7.0\publish\*" | Move-Item -Destination $SNLManagerFolder
Get-ChildItem -File -Path ".\UDPBDTray\bin\Release\net10.0-windows7.0\publish\*" | Move-Item -Destination $SNLManagerFolder

Copy-Item -Path ".\NeededForRelease\*" -Destination $SNLManagerFolder -Recurse -Force
Copy-Item -Path "..\SNLLua\*" -Destination "$SNLManagerFolder\InstallFiles\SimpleNeutrinoLoader" -Recurse -Force

Copy-Item -Path "..\README.md" -Destination "$ReleaseFolder\README.txt" -Force
Copy-Item -Path "..\LICENSE.txt" -Destination $ReleaseFolder -Force
Copy-Item -Path "..\neutrino-LICENSE.txt" -Destination $ReleaseFolder -Force
Copy-Item -Path "..\ps2client-license.txt" -Destination $ReleaseFolder -Force

Compress-Archive -Path "$ReleaseFolder\*" -DestinationPath ".\SNL-Manager-UDPBD-v$ReleaseVersion.zip" -Force

Remove-Item -Path ".\SNL-CLI\bin\Release" -Recurse
Remove-Item -Path ".\SimpleNeutrinoLoaderGUI\bin\Release" -Recurse
Remove-Item -Path ".\UDPBDTray\bin\Release" -Recurse

