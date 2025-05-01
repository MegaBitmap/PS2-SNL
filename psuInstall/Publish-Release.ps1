

Copy-Item -Path "..\SNLManagerSource\NeededForRelease\BlankVMC8.bin" -Destination ".\temp.bin"

& .\ps2vmc-tool.exe ".\temp.bin" --make-directory "/Enceladus"

& .\ps2vmc-tool.exe ".\temp.bin" --make-directory "/SimpleNeutrinoLoader"


$ListEnceladus = Get-ChildItem -Path ..\includeManualInstall\Enceladus

foreach ($file in $ListEnceladus)
{
    & .\ps2vmc-tool.exe ".\temp.bin" --inject-file $file.FullName "/Enceladus/$file"
}

$ListSNLIncude = Get-ChildItem -Path ..\includeManualInstall\SimpleNeutrinoLoader

foreach ($file in $ListSNLIncude)
{
    & .\ps2vmc-tool.exe ".\temp.bin" --inject-file $file.FullName "/SimpleNeutrinoLoader/$file"
}

$ListSNLLua = Get-ChildItem -Path ..\SNLLua

foreach ($file in $ListSNLLua)
{
    & .\ps2vmc-tool.exe ".\temp.bin" --inject-file $file.FullName "/SimpleNeutrinoLoader/$file"
}

$ReleaseVersion = (Get-Content ..\SNLLua\version.txt)[1]

$ReleaseFolder = ".\release-$ReleaseVersion"

New-Item -ItemType Directory -Path "$ReleaseFolder\psuInstall"

& .\ps2vmc-tool.exe ".\temp.bin" --psu-export "/Enceladus" "$ReleaseFolder\psuInstall\EnceladusSNL-$ReleaseVersion.psu"

& .\ps2vmc-tool.exe ".\temp.bin" --psu-export "/SimpleNeutrinoLoader" "$ReleaseFolder\psuInstall\SimpleNeutrinoLoader-$ReleaseVersion.psu"

Remove-Item -Path ".\temp.bin"

Copy-Item -Path "..\includeManualInstall\*" -Destination $ReleaseFolder -Recurse -Force
Copy-Item -Path "..\SNLLua\*" -Destination "$ReleaseFolder\SimpleNeutrinoLoader" -Recurse -Force
Copy-Item -Path "..\ListBuilder" -Destination $ReleaseFolder -Recurse -Force

Copy-Item -Path "..\README.md" -Destination "$ReleaseFolder\README.txt" -Force
Copy-Item -Path "..\LICENSE.txt" -Destination $ReleaseFolder -Force
Copy-Item -Path "..\neutrino-LICENSE.txt" -Destination $ReleaseFolder -Force

Compress-Archive -Path "$ReleaseFolder\*" -DestinationPath ".\SNL-Full-$ReleaseVersion.zip" -Force

Remove-Item -Path $ReleaseFolder -Recurse

