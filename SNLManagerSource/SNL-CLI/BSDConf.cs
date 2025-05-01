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
                "env = [\"LE\", \"EE\"]\n\n" +
                "# Faking strategy\n" +
                "# ---------------\n" +
                "# To prevent games from trying to use networing:\n" +
                "# - we try to simulate that there is no dev9 hardware present:\n" +
                "#   - dev9 returns NO_RESIDENT_END, module is hidden\n" +
                "#   - all modules depending on dev9 fail to load becouse dev9 is not resident\n" +
                "[[fake]]\n" +
                "file = \"ENT_SMAP.IRX\"\n" +
                "name = \"ent_smap\"\n" +
                "version = 0x021f\n" +
                "loadrv = -200 # KE_LINKERR becouse dev9 does not exist\n" +
                "startrv = 1    # 0=RESIDENT_END, 1=NO_RESIDENT_END, 2=REMOVABLE_END\n" +
                "[[fake]]\n" +
                "file = \"SMAP.IRX\"\n" +
                "name = \"INET_SMAP_driver\"\n" +
                "version = 0x0219\n" +
                "loadrv = -200 # KE_LINKERR becouse dev9 does not exist\n" +
                "startrv = 1    # 0=RESIDENT_END, 1=NO_RESIDENT_END, 2=REMOVABLE_END\n";
        }
    }
}
