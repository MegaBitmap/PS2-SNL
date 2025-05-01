
$ReleaseVersion = (Get-Content -Path ".\SNL-CLI\SNL-CLI.csproj" | Select-String -Pattern AssemblyVersion).ToString().Trim() -replace "<[^>]+>"
$ReleaseFolder = ".\SimpleNeutrinoLoaderGUI\bin\Release\net8.0-windows\publish\release-$ReleaseVersion"
$SNLManagerFolder = "$ReleaseFolder\SNL Manager (UDPBD)"

dotnet publish ".\SNL-CLI.sln"
dotnet publish ".\SimpleNeutrinoLoaderGUI.sln"

New-Item -ItemType Directory -Path $SNLManagerFolder

Get-ChildItem -File -Path ".\SNL-CLI\bin\Release\net8.0\publish\*" | Move-Item -Destination $SNLManagerFolder
Get-ChildItem -File -Path ".\SimpleNeutrinoLoaderGUI\bin\Release\net8.0-windows\publish\*" | Move-Item -Destination $SNLManagerFolder

Copy-Item -Path ".\NeededForRelease\*" -Destination $SNLManagerFolder -Recurse -Force
Copy-Item -Path "..\SNLLua\*" -Destination "$SNLManagerFolder\InstallFiles\SimpleNeutrinoLoader" -Recurse -Force

Copy-Item -Path "..\README.md" -Destination "$ReleaseFolder\README.txt" -Force
Copy-Item -Path "..\LICENSE.txt" -Destination $ReleaseFolder -Force
Copy-Item -Path "..\neutrino-LICENSE.txt" -Destination $ReleaseFolder -Force

Compress-Archive -Path "$ReleaseFolder\*" -DestinationPath ".\SNL-Manager-UDPBD-v$ReleaseVersion.zip" -Force

Remove-Item -Path ".\SNL-CLI\bin\Release" -Recurse
Remove-Item -Path ".\SimpleNeutrinoLoaderGUI\bin\Release" -Recurse

