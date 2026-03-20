using System.Net;

namespace SNL_CLI;

internal class BSDConf
{
    public static string Config(IPAddress ps2ip)
    {
        return "" +
            "# Name of loaded config, to show to user\n" +
            "name = \"UDPBD BDM driver\"\n\n" +
            "# Drivers this driver depends on (config file must exist)\n" +
            "depends = [\"i_bdm\", \"i_dev9_hidden\"]\n\n" +
            "# Modules to load\n" +
            "[[module]]\n" +
            "file = \"smap.irx\"\n" +
            "env = [\"LE\", \"EE\"]\n" +
            "[[module]]\n" +
            "file = \"ministack.irx\"\n" +
            $"args = [\"ip={ps2ip}\"]\n" +
            "env = [\"LE\", \"EE\"]\n" +
            "[[module]]\n" +
            "file = \"udpbd.irx\"\n" +
            "#file = \"udpfs_bd.irx\" # Alternative driver based on UDPRDMA\n" +
            "env = [\"LE\", \"EE\"]\n";
    }
}
