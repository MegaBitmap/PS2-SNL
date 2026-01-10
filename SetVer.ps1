
Write-Host "Enter a new version:"
$newVer = Read-Host

$lineVer = Get-Content -Path .\SNLLua\SNLMain.lua | Select-String -Pattern "\..\..\."
$oldVer = $lineVer -replace ".* v", "" -replace "`".*", ""
$newLine = $lineVer -replace $oldVer, $newVer
(Get-Content -Path .\SNLLua\SNLMain.lua) -replace $lineVer, $newLine | Set-Content -Path .\SNLLua\SNLMain.lua

$lineVer = (Get-Content -Path .\SNLLua\version.txt)[1]
$oldVer = $lineVer -replace "v", ""
$newLine = $lineVer -replace $oldVer, $newVer
(Get-Content -Raw -Path .\SNLLua\version.txt) -replace $lineVer, $newLine | Set-Content -NoNewline -Path .\SNLLua\version.txt

$lineVer = Get-Content -Path .\SNLSyncSource\SimpleNeutrinoLoaderGUI\SimpleNeutrinoLoaderGUI.csproj | Select-String -Pattern "<AssemblyVersion>"
$oldVer = $lineVer -replace ".*<AssemblyVersion>", "" -replace "</.*", ""
$newLine = $lineVer -replace $oldVer, $newVer
(Get-Content -Path .\SNLSyncSource\SimpleNeutrinoLoaderGUI\SimpleNeutrinoLoaderGUI.csproj) -replace $lineVer, $newLine | Set-Content -Encoding UTF8 -Path .\SNLSyncSource\SimpleNeutrinoLoaderGUI\SimpleNeutrinoLoaderGUI.csproj

$lineVer = Get-Content -Path .\SNLSyncSource\SNL-CLI\SNL-CLI.csproj | Select-String -Pattern "<AssemblyVersion>"
$oldVer = $lineVer -replace ".*<AssemblyVersion>", "" -replace "</.*", ""
$newLine = $lineVer -replace $oldVer, $newVer
(Get-Content -Path .\SNLSyncSource\SNL-CLI\SNL-CLI.csproj) -replace $lineVer, $newLine | Set-Content -Encoding UTF8 -Path .\SNLSyncSource\SNL-CLI\SNL-CLI.csproj

$lineVer = Get-Content -Path .\SNLSyncSource\UDPBDTray\UDPBDTray.csproj | Select-String -Pattern "<AssemblyVersion>"
$oldVer = $lineVer -replace ".*<AssemblyVersion>", "" -replace "</.*", ""
$newLine = $lineVer -replace $oldVer, $newVer
(Get-Content -Path .\SNLSyncSource\UDPBDTray\UDPBDTray.csproj) -replace $lineVer, $newLine | Set-Content -Encoding UTF8 -Path .\SNLSyncSource\UDPBDTray\UDPBDTray.csproj


