using FluentFTP;
using System.Diagnostics;
using System.Net;

namespace SNL_CLI;

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

    public static bool KillServer()
    {
        string[] serverNames = ["UDPBDTray", "udpbd-server", "udpbd-vexfat"];
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
}
