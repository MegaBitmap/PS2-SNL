using System.Net;

namespace SNL_CLI
{
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
                "file = \"smap_udpbd.irx\"\n" +
                $"args = [\"ip={ps2ip}\"]\n" +
                "env = [\"LE\", \"EE\"]\n";
        }
    }
}
