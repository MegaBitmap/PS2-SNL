
$DelPaths = @(
".\SNL-CLI\bin"
".\SNL-CLI\obj"
".\UDPBDG\bin"
".\UDPBDG\obj"
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

