
$DelPaths = @(
".\SimpleNeutrinoLoaderGUI\bin"
".\SimpleNeutrinoLoaderGUI\obj"
".\SNL-CLI\bin"
".\SNL-CLI\obj"
".\UDPBDTray\bin"
".\UDPBDTray\obj"
".\udpbd-vexfat"
".\udpbd-server"
)

foreach ($delPath in $DelPaths)
{
    if (Test-Path $delPath)
    {
        Remove-Item -Path $delPath -Recurse -Force
    }
}

