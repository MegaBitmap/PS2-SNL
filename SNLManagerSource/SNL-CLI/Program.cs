using System.Net;
using System.Reflection;
using FluentFTP;

namespace SNL_CLI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Simple Neutrino Loader Manager CLI {Assembly.GetExecutingAssembly().GetName().Version} by MegaBitmap");

            string gamePath = "";
            IPAddress ps2ip = IPAddress.Parse("192.168.0.10");
            bool enableBin2ISO = false;
            string installTarget = "";
            bool enableVMC = false;
            bool modifyBootloader = false;

            if (args.Length < 2 || !args.Contains("-ps2ip"))
            {
                PrintHelp();
                MiscMethods.PauseExit(0);
            }
            int argIndex = 0;
            foreach (var arg in args)
            {
                if (arg.Contains("-path"))
                {
                    gamePath = args[argIndex + 1];
                }
                else if (arg.Contains("-ps2ip"))
                {
                    ps2ip = IPAddress.Parse(args[argIndex + 1]);
                }
                else if (arg.Contains("-install"))
                {
                    installTarget = ParseInstallLocation(args[argIndex + 1]);
                }
                else if (arg.Contains("-boot"))
                {
                    modifyBootloader = true;
                }
                else if (arg.Contains("-bin2iso"))
                {
                    enableBin2ISO = true;
                }
                else if (arg.Contains("-enablevmc"))
                {
                    enableVMC = true;
                }
                argIndex++;
            }
            if (!string.IsNullOrEmpty(installTarget))
            {
                Install install = new();
                install.SNL(installTarget, ps2ip, modifyBootloader);
            }
            if (!MiscMethods.KillServer())
            {
                MiscMethods.PauseExit(2);
            }
            if (!Path.Exists(gamePath))
            {
                Console.WriteLine("The path does not exist.");
                MiscMethods.PauseExit(3);
            }
            if (!Path.Exists($"{gamePath}/CD") && !Path.Exists($"{gamePath}/DVD"))
            {
                Console.WriteLine($"There must be a DVD or CD folder inside '{gamePath}'.");
                MiscMethods.PauseExit(4);
            }
            if (enableBin2ISO)
            {
                CDBin.ConvertFolder(gamePath);
            }
            List<string> gameList = ScanFolder(gamePath);
            if (gameList.Count < 1)
            {
                Console.WriteLine($"No games found in {gamePath}/CD or {gamePath}/DVD");
                MiscMethods.PauseExit(5);
            }
            Console.WriteLine($"{gameList.Count} games loaded");
            CreateGameList(gamePath, gameList);

            FtpClient client = new(ps2ip.ToString());

            if (!FTP.TestConnection(client, ps2ip))
            {
                MiscMethods.PauseExit(6);
            }
            string syncTarget = GetInstallLocation(client);
            if (string.IsNullOrEmpty(syncTarget))
            {
                Console.WriteLine($"Unable to detect a valid Enceladus and SimpleNeutrinoLoader installation.");
                MiscMethods.PauseExit(26);
            }
            MiscMethods.UpdateUDPConfig(client, ps2ip, syncTarget);

            if (enableVMC)
            {
                if (VMC.Sync(gamePath, gameList))
                {
                    Console.WriteLine("Virtual Memory Cards are now enabled.");
                }
                else Console.WriteLine("Virtual Memory Cards are now disabled.");
            }
            else Console.WriteLine("Virtual Memory Cards are now disabled.");

            MiscMethods.ValidateList("UDPBDList.txt");
            FTP.UploadFile(client, "UDPBDList.txt", $"{syncTarget}/SimpleNeutrinoLoader/", "UDPBDList.txt");
            Console.WriteLine($"Updated game list at {syncTarget}/SimpleNeutrinoLoader/UDPBDList.txt");
            client.Disconnect();
            Console.WriteLine(
@" ________       ___    ___ ________   ________  _______   ________     
|\   ____\     |\  \  /  /|\   ___  \|\   ____\|\  ___ \ |\   ___ \    
\ \  \___|_    \ \  \/  / | \  \\ \  \ \  \___|\ \   __/|\ \  \_|\ \   
 \ \_____  \    \ \    / / \ \  \\ \  \ \  \    \ \  \_|/_\ \  \ \\ \  
  \|____|\  \    \/  /  /   \ \  \\ \  \ \  \____\ \  \_|\ \ \  \_\\ \ 
    ____\_\  \ __/  / /      \ \__\\ \__\ \_______\ \_______\ \_______\
   |\_________\\___/ /        \|__| \|__|\|_______|\|_______|\|_______|
   \|_________\|___|/");
            Console.WriteLine("Synchronization with the PS2 is now complete!");
            Console.WriteLine("Please make sure to start the server before launching a game.");
            MiscMethods.PauseExit(10);
        }

        static void PrintHelp()
        {
            Console.WriteLine("\n" +
                "Usage Example:\n" +
                @"dotnet SNL-CLI.dll -install mc0 -ps2ip 192.168.0.10 -boot" +
                "\n-install '?' will install Enceladus, neutrino, and SNL to the specified device (mc0, mc1, or mass).\n" +
                "-ps2ip '?' is the ip address for connecting to the PS2 with PS2Net.\n" +
                "-boot will modify basic-boot-loader to load network drivers on boot and auto-run SNL.\n\n" +
                @"dotnet SNL-CLI.dll -path 'C:\PS2\' -ps2ip 192.168.0.10 -bin2iso -enablevmc" +
                "\n-path '?' is the file path to the CD and DVD folder that contain game ISOs.\n" +
                "-bin2iso enables automatic CD-ROM Bin to ISO conversion.\n" +
                "-enablevmc will assign a virtual memory card for each game or group of games in 'vmc_groups.list'.\n");
        }

        static string GetInstallLocation(FtpClient client)
        {
            Install install = new();
            if (install.VerifyInstallation(client, "/mc/0"))
            {
                return "/mc/0";
            }
            else if (install.VerifyInstallation(client, "/mc/1"))
            {
                return "/mc/1";
            }
            else if (install.VerifyInstallation(client, "/mass/0"))
            {
                return "/mass/0";
            }
            return "";
        }

        static string ParseInstallLocation(string target)
        {
            if (target.Contains("mc0")) return "mc0";
            else if (target.Contains("mc1")) return "mc1";
            else if (target.Contains("mass")) return "mass";
            else
            {
                Console.WriteLine($"{target} is an invalid install location.");
                MiscMethods.PauseExit(34);
                return "";
            }
        }

        static List<string> ScanFolder(string scanPath)
        {
            List<string> tempList = [];
            string[] scanFolders = [$"{scanPath}/CD", $"{scanPath}/DVD"];
            foreach (var folder in scanFolders)
            {
                if (Directory.Exists(folder))
                {
                    string[] ISOFiles = Directory.GetFiles(folder, "*.iso", SearchOption.TopDirectoryOnly);
                    foreach (string file in ISOFiles)
                    {
                        tempList.Add(file.Replace(scanPath, "").Replace(@"\", "/"));
                    }
                }
            }
            return tempList;
        }

        static void CreateGameList(string gamePath, List<string> gameList)
        {
            List<string> gameListWithID = [];
            foreach (var game in gameList)
            {
                string serialGameID = MiscMethods.GetSerialID(gamePath + game);
                string friendlyName = Path.GetFileNameWithoutExtension(gamePath + game);
                if (!string.IsNullOrEmpty(serialGameID))
                {
                    gameListWithID.Add($"{friendlyName}|{serialGameID}|-bsd=udpbd|-dvd=mass:{game}");
                    Console.WriteLine($"Loaded {game}");
                }
                else
                {
                    Console.WriteLine($"Unable to find a serial Game ID for {game}");
                }
            }
            File.WriteAllLines("UDPBDList.txt", gameListWithID);
            Thread.Sleep(200);
        }
    }
}
