
$CLIDir = ".\SNL-CLI\bin\Release\net10.0\publish"
$GUIDir = ".\UDPBDG\bin\Release\net10.0-windows\publish"

$ReleaseVersion = (Get-Content -Path ".\SNL-CLI\SNL-CLI.csproj" | Select-String -Pattern AssemblyVersion).ToString().Trim() -replace "<[^>]+>"
$ReleaseFolder = "$GUIDir\release-$ReleaseVersion"
$SNLCLIFolder = "$ReleaseFolder\SNL-CLI"
$UDPBDGFolder = "$ReleaseFolder\UDPBDG"

dotnet publish ".\SNL-CLI.sln"
dotnet publish ".\UDPBDG.slnx"

# Preserve the current working directory
$env:CHERE_INVOKING = "yes"
# Start a 64 bit Mingw environment
$env:MSYSTEM = "UCRT64"
# Run for the first time
& "C:\msys64\usr\bin\bash" "-lc" " "
# Update MSYS2 Core (in case any core packages are outdated)
& "C:\msys64\usr\bin\bash" "-lc" "pacman --noconfirm -Syuu"
& "C:\msys64\usr\bin\bash" "-lc" "pacman --noconfirm -Syuu"
& "C:\msys64\usr\bin\bash" "-lc" "pacman --noconfirm --needed -S git make mingw-w64-ucrt-x86_64-gcc"
& "C:\msys64\usr\bin\bash" "-lc" "curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh -s -- -y --no-modify-path"
# Build udpbd_vexfat.dll
& "C:\msys64\usr\bin\bash" "-lc" "git clone --recurse-submodules -b windows_dll https://github.com/MegaBitmap/udpbd-vexfat.git"
& "C:\msys64\usr\bin\bash" "-lc" "export PATH=`"/c/Users/`$USER/.cargo/bin:`$PATH`"
cd udpbd-vexfat/vexfatbd/
cargo update
cd ..
cargo update
cargo build --release --target x86_64-pc-windows-gnu"
# Build udpbd_server.dll
& "C:\msys64\usr\bin\bash" "-lc" "git clone -b windows_dll https://github.com/MegaBitmap/udpbd-server.git"
& "C:\msys64\usr\bin\bash" "-lc" "cd udpbd-server/
make"

New-Item -ItemType Directory -Path $SNLCLIFolder
New-Item -ItemType Directory -Path $UDPBDGFolder

Copy-Item -Path .\udpbd-vexfat\target\x86_64-pc-windows-gnu\release\udpbd_vexfat.dll -Destination $UDPBDGFolder -Force
Copy-Item -Path .\udpbd-server\udpbd_server.dll -Destination $UDPBDGFolder -Force

Copy-Item -Path ..\ListBuilder\vmc_groups.list -Destination $SNLCLIFolder -Force
Get-ChildItem -File -Path "$CLIDir\*" | Move-Item -Destination $SNLCLIFolder -Force
Get-ChildItem -File -Path "$GUIDir\*" | Move-Item -Destination $UDPBDGFolder -Force

Copy-Item -Path ".\NeededForRelease\*" -Destination $SNLCLIFolder -Recurse -Force
Copy-Item -Path ".\NeededForRelease\InstallFiles\" -Destination $UDPBDGFolder -Recurse -Force
Copy-Item -Path "..\udpfs_server\" -Destination $GUIDir -Recurse -Force

Copy-Item -Path "..\SNLLua\*" -Destination "$SNLCLIFolder\InstallFiles\SimpleNeutrinoLoader" -Recurse -Force
Copy-Item -Path "..\SNLLua\*" -Destination "$UDPBDGFolder\InstallFiles\SimpleNeutrinoLoader" -Recurse -Force

Copy-Item -Path "..\README.md" -Destination "$ReleaseFolder\README.txt" -Force
Copy-Item -Path "..\LICENSE.txt" -Destination $ReleaseFolder -Force
Copy-Item -Path "..\neutrino-LICENSE.txt" -Destination $ReleaseFolder -Force

Compress-Archive -Path "$ReleaseFolder\*" -DestinationPath ".\SNL-UDPBD-v$ReleaseVersion.zip" -Force

