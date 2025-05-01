using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using DiscUtils.Iso9660;
using FluentFTP;

namespace SNL_CLI
{
    internal partial class MiscMethods
    {
        public static void ValidateList(string fileName)
        {
            string combinedList = File.ReadAllText(fileName);
            if (combinedList.Length < 20)
            {
                Console.WriteLine($"Failed to save game list to {fileName}");
                Console.WriteLine("The sync was not able to be completed.");
                PauseExit(9);
            }
        }

        public static bool CheckSpace(string source, string destination)
        {
            FileInfo fileInfo = new(source);
            long fileSize = fileInfo.Length;
            string? dest = Path.GetPathRoot(destination);
            if (dest == null) return false;
            DriveInfo driveInfo = new(dest);
            long availableSpace = driveInfo.AvailableFreeSpace;
            if (availableSpace > fileSize)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void UpdateUDPConfig(FtpClient client, IPAddress ps2ip, string syncTarget)
        {
            string udpConf = BSDConf.Config(ps2ip);
            File.WriteAllText("temp-bsd-udpbd.toml", udpConf);
            Thread.Sleep(200);
            FTP.UploadFile(client, "temp-bsd-udpbd.toml", $"{syncTarget}/SimpleNeutrinoLoader/", "bsd-udpbd.toml");
            Console.WriteLine($"Updated {syncTarget}/SimpleNeutrinoLoader/bsd-udpbd.toml with the IP address {ps2ip}");
        }

        public static string GetSerialID(string fullGamePath)
        {
            try
            {
                string content;
                using (FileStream isoStream = File.Open(fullGamePath, FileMode.Open))
                {
                    CDReader cd = new(isoStream, true);
                    if (!cd.FileExists(@"SYSTEM.CNF"))
                    {
                        Console.WriteLine($"{fullGamePath} Is not a valid PS2 game ISO. The SYSTEM.CNF file is missing.");
                        return "";
                    }
                    using Stream fileStream = cd.OpenFile(@"SYSTEM.CNF", FileMode.Open);
                    using StreamReader reader = new(fileStream);
                    content = reader.ReadToEnd();
                }
                if (!content.Contains("BOOT2"))
                {
                    Console.WriteLine($"{fullGamePath} Is not a valid PS2 game ISO.\nThe SYSTEM.CNF file does not contain BOOT2.");
                    return "";
                }
                string serialID = SerialMask().Replace(content.Split("\n")[0], "");
                return serialID;
            }
            catch (Exception)
            {
                Console.WriteLine($"{fullGamePath} was unable to be read. The ISO file may be corrupt.");
                return "";
            }
        }

        public static bool KillServer()
        {
            string[] serverNames = ["udpbd-server", "udpbd-vexfat"];
            foreach (var server in serverNames)
            {
                Process[] processes = Process.GetProcessesByName(server);
                if (!(processes.Length == 0))
                {
                    Console.Write("The server is currently running, do you want to stop the server and sync? (y/n): ");
                    char response = Console.ReadKey().KeyChar;
                    Console.WriteLine();
                    if (response == 'y' || response == 'Y')
                    {
                        foreach (var item in processes) item.Kill();
                        return true;
                    }
                    else return false;
                }
            }
            return true;
        }

        public static void PauseExit(int number)
        {
            Console.Write("Press any key to continue . . . ");
            Console.ReadKey();
            Console.WriteLine();
            Environment.Exit(number);
        }

        [GeneratedRegex(@".*\\|;.*")]
        private static partial Regex SerialMask();
    }
}
