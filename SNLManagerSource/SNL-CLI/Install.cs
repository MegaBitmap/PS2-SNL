using System.Net;
using FluentFTP;

namespace SNL_CLI
{
    internal class Install
    {
        readonly List<string> enceladusFiles = ["enceladus_pkd.elf", "helloworld.lua", "icon.icn", "icon.sys", "index.lua"];
        readonly List<string> SNLFiles = ["bdm.irx", "bdmfs_fatfs.irx", "bsd-udpbd.toml", "bsdfs-exfat.toml", "cdvdfsv.irx",
		"cdvdman_emu.irx", "dev9_hidden.irx", "eesync.irx", "ee_core.elf", "emu-dvd-file.toml", "emu-mc-file.toml", "fakemod.irx",
		"fhi_bd_defrag.irx", "fileXio.irx", "Gudea-Bold.ttf", "icon.icn", "icon.sys", "imgdrv.irx", "iomanX.irx", "i_bdm.toml", "i_dev9_hidden.toml",
		"mc_emu.irx", "neutrino.elf", "smap_udpbd.irx", "SNLFunctions.lua", "SNLMain.lua",
		"system.toml", "udnl.irx", "version.txt"];
        readonly List<string> networkDrivers = ["ps2dev9.irx", "netman.irx", "smap.irx"];

        public void SNL(string installTarget, IPAddress ps2ip, bool modifyBootloader)
        {
            FtpClient client = new(ps2ip.ToString());
            string rootFolder = "mc";
            string childFolder = "0";

            if (!FTP.TestConnection(client, ps2ip))
            {
                MiscMethods.PauseExit(46);
            }
            if (!VerifyLocalFiles(enceladusFiles, "InstallFiles/Enceladus") ||
                !VerifyLocalFiles(SNLFiles, "InstallFiles/SimpleNeutrinoLoader") ||
                !VerifyLocalFiles(networkDrivers, "InstallFiles/NetworkDrivers"))
            {
                Console.WriteLine("ERROR: One or more files from 'InstallFiles' are missing.");
                MiscMethods.PauseExit(71);
            }
            if (installTarget.Contains("mc0"))
            {
                rootFolder = "mc";
                childFolder = "0";
            }
            else if (installTarget.Contains("mc1"))
            {
                rootFolder = "mc";
                childFolder = "1";
            }
            else if (installTarget.Contains("mass"))
            {
                rootFolder = "mass";
                childFolder = "0";
            }
            if (!FTP.DirectoryExists(client, $"/{rootFolder}/{childFolder}/Enceladus"))
            {
                FTP.CreateDirectory(client, $"/{rootFolder}/{childFolder}/Enceladus");
                InstallEnceladus(client, $"/{rootFolder}/{childFolder}/Enceladus/");
            }
            else if (!VerifyFTPFiles(client, enceladusFiles, $"/{rootFolder}/{childFolder}/Enceladus", "InstallFiles/Enceladus"))
            {
                InstallEnceladus(client, $"/{rootFolder}/{childFolder}/Enceladus/");
            }
            if (!FTP.DirectoryExists(client, $"/{rootFolder}/{childFolder}/SimpleNeutrinoLoader"))
            {
                FTP.CreateDirectory(client, $"/{rootFolder}/{childFolder}/SimpleNeutrinoLoader");
                InstallSNL(client, $"/{rootFolder}/{childFolder}/SimpleNeutrinoLoader/");
            }
            else if (!VerifyFTPFiles(client, SNLFiles, $"/{rootFolder}/{childFolder}/SimpleNeutrinoLoader", "InstallFiles/SimpleNeutrinoLoader"))
            {
                InstallSNL(client, $"/{rootFolder}/{childFolder}/SimpleNeutrinoLoader/");
            }
            Console.WriteLine("Verifying Installation . . .");
            if (!VerifyFTPFiles(client, enceladusFiles, $"/{rootFolder}/{childFolder}/Enceladus", "InstallFiles/Enceladus"))
            {
                Console.WriteLine($"Failed to install Enceladus to {rootFolder}");
                MiscMethods.PauseExit(23);
            }
            else if (!VerifyFTPFiles(client, SNLFiles, $"/{rootFolder}/{childFolder}/SimpleNeutrinoLoader", "InstallFiles/SimpleNeutrinoLoader"))
            {
                Console.WriteLine($"Failed to install Simple Neutrino Loader to {rootFolder}");
                MiscMethods.PauseExit(24);
            }
            if (modifyBootloader)
            {
                if (!IsPS2BBLInstalled(client))
                {
                    Console.WriteLine("ERROR: Failed to find an existing PS2BBL installation.");
                    MiscMethods.PauseExit(27);
                }
                string configTarget = "mc?";
                if (rootFolder.Contains("mass"))
                {
                    configTarget = "mass";
                }
                UpdateBLConfig(client, configTarget);
            }
            Console.WriteLine($"\nEnceladus and SimpleNeutrinoLoader have been installed to {rootFolder}\n\n" +
                "Please remember to sync your game list then start the server.");
            MiscMethods.PauseExit(62);
        }

        public bool VerifyInstallation(FtpClient client, string FTPPath)
        {
            string tempDir = FTP.GetDir(client, $"{FTPPath}/Enceladus");
            foreach (string file in enceladusFiles)
            {
                if (!tempDir.Contains(file))
                {
                    return false;
                }
            }
            tempDir = FTP.GetDir(client, $"{FTPPath}/SimpleNeutrinoLoader");
            foreach (string file in SNLFiles)
            {
                if (!tempDir.Contains(file))
                {
                    return false;
                }
            }
            return true;
        }

        static bool VerifyFTPFiles(FtpClient client, List<string> files, string FTPPath, string folder)
        {
            string tempDir = FTP.GetDir(client, FTPPath);
            foreach (string file in files)
            {
                FileInfo fileInfo = new($"{folder}/{file}");
                if (!tempDir.Contains(file))
                {
                    return false;
                }
                if (FTP.GetSize(client, FTPPath, file) != fileInfo.Length)
                {
                    return false;
                }
            }
            return true;
        }

        static bool VerifyLocalFiles(List<string> files, string folder)
        {
            foreach (string file in files)
            {
                if (!File.Exists($"{folder}/{file}"))
                {
                    return false;
                }
            }
            return true;
        }

        void InstallEnceladus(FtpClient client, string folder)
        {
            Console.WriteLine("Starting installation of Enceladus . . .");
            foreach (string file in enceladusFiles)
            {
                Console.WriteLine($"Installing {file} to {folder}{file} . . .");
                FTP.UploadFile(client, $"InstallFiles/Enceladus/{file}", folder, file);
            }
        }

        void InstallSNL(FtpClient client, string folder)
        {
            Console.WriteLine("Starting installation of Simple Neutrino Loader . . .");
            foreach (string file in SNLFiles)
            {
                Console.WriteLine($"Installing {file} to {folder}{file} . . .");
                FTP.UploadFile(client, $"InstallFiles/SimpleNeutrinoLoader/{file}", folder, file);
            }
        }

        void InstallNetworkDrivers(FtpClient client, string folder)
        {
            Console.WriteLine("Starting installation of Network Drivers . . .");
            foreach (string file in networkDrivers)
            {
                Console.WriteLine($"Installing {file} to {folder}{file} . . .");
                FTP.UploadFile(client, $"InstallFiles/NetworkDrivers/{file}", folder, file);
            }
        }

        public static bool IsPS2BBLInstalled(FtpClient client)
        {
            string configType = GetBLConfig(client);
            if (string.IsNullOrEmpty(configType))
            {
                return false;
            }
            return true;
        }

        public void UpdateBLConfig(FtpClient client, string target)
        {
            string configPath = GetBLConfig(client);
            string configFolder = "";
            string configFile = "";
            if (configPath.Contains("SYS-CONF"))
            {
                configFolder = "SYS-CONF";
                configFile = "PS2BBL.INI";
            }
            else
            {
                configFolder = "PS2BBL";
                configFile = "CONFIG.INI";
            }
            string configContents = PS2BBL.Config(target, configFolder);
            File.WriteAllText( "temp-BL-CFG.txt", configContents);
            Thread.Sleep(200);
            string readContent = File.ReadAllText("temp-BL-CFG.txt");
            if (readContent.Length < 300)
            {
                Console.WriteLine($"Error: Failed to save/load contents of 'temp-BL-CFG.txt' in this folder: {Directory.GetCurrentDirectory}");
                MiscMethods.PauseExit(82);
            }
            FTP.UploadFile(client, "temp-BL-CFG.txt", configPath, configFile);

            if (!VerifyFTPFiles(client, networkDrivers, configPath, "InstallFiles/NetworkDrivers"))
            {
                InstallNetworkDrivers(client, configPath);
                if (!VerifyFTPFiles(client, networkDrivers, configPath, "InstallFiles/NetworkDrivers"))
                {
                    Console.WriteLine("Error: Failed to install network drivers.");
                    MiscMethods.PauseExit(83);
                }
            }
            Console.WriteLine($"The configuration for PS2BBL has been updated in {configPath}{configFile}");
        }

        static string GetBLConfig(FtpClient client)
        {
            if (FTP.DirectoryExists(client, "/mc/0/SYS-CONF"))
            {
                if (FTP.FileExists(client, "/mc/0/SYS-CONF", "PS2BBL.INI"))
                {
                    return "/mc/0/SYS-CONF/";
                }
            }
            if (FTP.DirectoryExists(client, "/mc/1/SYS-CONF"))
            {
                if (FTP.FileExists(client, "/mc/1/SYS-CONF", "PS2BBL.INI"))
                {
                    return "/mc/1/SYS-CONF/";
                }
            }
            if (FTP.DirectoryExists(client, "/mc/0/PS2BBL"))
            {
                if (FTP.FileExists(client, "/mc/0/PS2BBL", "CONFIG.INI"))
                {
                    return "/mc/0/PS2BBL/";
                }
            }
            if (FTP.DirectoryExists(client, "/mc/1/PS2BBL"))
            {
                if (FTP.FileExists(client, "/mc/1/PS2BBL", "CONFIG.INI"))
                {
                    return "/mc/1/PS2BBL/";
                }
            }
            return "";
        }
    }
}
