using FluentFTP;
using System.Net;

namespace SNL_CLI
{
    internal class Install
    {
        readonly List<string> enceladusFiles = ["enceladus_pkd.elf", "helloworld.lua", "icon.icn", "icon.sys", "index.lua"];

        public void SNL(string installTarget, IPAddress ps2ip, bool modifyBootloader)
        {
            FtpClient client = new(ps2ip.ToString());
            client.Config.LogToConsole = false; // Set to true when debugging FTP commands
            client.Config.DataConnectionType = FtpDataConnectionType.PASVEX;
            client.Config.CheckCapabilities = false;
            string rootFolder = "mc";
            string childFolder = "0";

            if (!FTP.TestConnection(client, ps2ip))
            {
                MiscMethods.PauseExit(46);
            }
            if (!VerifyLocalFiles(enceladusFiles, "InstallFiles/Enceladus") ||
                !VerifyLocalFiles(SNLFiles.Names(), "InstallFiles/SimpleNeutrinoLoader"))
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
            else if (!VerifyFTPFiles(client, SNLFiles.Names(), $"/{rootFolder}/{childFolder}/SimpleNeutrinoLoader", "InstallFiles/SimpleNeutrinoLoader"))
            {
                InstallSNL(client, $"/{rootFolder}/{childFolder}/SimpleNeutrinoLoader/");
            }
            Console.WriteLine("Verifying Installation . . .");
            if (!VerifyFTPFiles(client, enceladusFiles, $"/{rootFolder}/{childFolder}/Enceladus", "InstallFiles/Enceladus"))
            {
                Console.WriteLine($"Failed to install Enceladus to {rootFolder}");
                MiscMethods.PauseExit(23);
            }
            else if (!VerifyFTPFiles(client, SNLFiles.Names(), $"/{rootFolder}/{childFolder}/SimpleNeutrinoLoader", "InstallFiles/SimpleNeutrinoLoader"))
            {
                Console.WriteLine($"Failed to install Simple Neutrino Loader to {rootFolder}");
                MiscMethods.PauseExit(24);
            }
            if (modifyBootloader)
            {
                if (IsPS2BBLInstalled(client))
                {
                    string configTarget = "mc?";
                    if (rootFolder.Contains("mass"))
                    {
                        configTarget = "mass";
                    }
                    UpdateBLConfig(client, configTarget);
                }
                else
                {
                    Console.WriteLine("Skipping PS2BBL configuration update because no installation was found.");
                }
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
            foreach (string file in SNLFiles.Names())
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

        static void InstallSNL(FtpClient client, string folder)
        {
            Console.WriteLine("Starting installation of Simple Neutrino Loader . . .");
            foreach (string file in SNLFiles.Names())
            {
                Console.WriteLine($"Installing {file} to {folder}{file} . . .");
                FTP.UploadFile(client, $"InstallFiles/SimpleNeutrinoLoader/{file}", folder, file);
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

        public static void UpdateBLConfig(FtpClient client, string target)
        {
            string configPath = GetBLConfig(client);
            string configFile = "";
            if (configPath.Contains("SYS-CONF"))
            {
                configFile = "PS2BBL.INI";
            }
            else
            {
                configFile = "CONFIG.INI";
            }
            string configContents = PS2BBL.Config(target);
            File.WriteAllText( "temp-BL-CFG.txt", configContents);
            Thread.Sleep(200);
            string readContent = File.ReadAllText("temp-BL-CFG.txt");
            if (readContent.Length < 300)
            {
                Console.WriteLine($"Error: Failed to save/load contents of 'temp-BL-CFG.txt' in this folder: {Directory.GetCurrentDirectory}");
                MiscMethods.PauseExit(82);
            }
            FTP.UploadFile(client, "temp-BL-CFG.txt", configPath, configFile);
            Console.WriteLine($"The configuration for PS2BBL has been updated in {configPath}{configFile}");
        }

        static string GetBLConfig(FtpClient client)
        {
            List<string> folders = ["SYS-CONF", "PS2BBL"];
            List<string> configFiles = ["PS2BBL.INI", "CONFIG.INI"];
            foreach (string folder in folders)
            {
                for (int i = 0; i < 2; i++)
                {
                    string testFolder = $"/mc/{i}/{folder}";
                    foreach (string configFile in configFiles)
                    {
                        if (FTP.FileExists(client, testFolder, configFile))
                        {
                            return testFolder + "/";
                        }
                    }
                }
            }
            return "";
        }
    }
}
