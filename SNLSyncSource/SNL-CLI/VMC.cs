namespace SNL_CLI
{
    internal class VMC
    {
        public static bool Sync(string gamePath, List<string> gameList)
        {
            List<string> gameListVMC = [];
            if (!Path.Exists($"{gamePath}/VMC"))
            {
                Directory.CreateDirectory($"{gamePath}/VMC");
            }
            string[] neededFiles = ["vmc_groups.list", "BlankVMC8.bin", "BlankVMC32.bin"];
            foreach (string file in neededFiles)
            {
                if (!File.Exists(file))
                {
                    Console.WriteLine($"Missing file {file}");
                    return false;
                }
            }
            string[] groupsVMC = File.ReadAllLines("vmc_groups.list");
            string crossSaveIDs = string.Join("", groupsVMC);
            foreach (var game in gameList)
            {
                string serialID = MiscMethods.GetSerialID(gamePath + game);
                if (string.IsNullOrEmpty(serialID))
                {
                    Console.WriteLine($"Failed to get serial ID for {gamePath + game}");
                    continue;
                }
                string friendlyName = Path.GetFileNameWithoutExtension(gamePath + game);
                string vmcRelativePath;
                string vmcFullPath;
                int currentVmcSize = 8;
                if (crossSaveIDs.Contains(serialID))
                {
                    string vmcFile = "";
                    bool checkSize = false;
                    string currentGroup = "";

                    foreach (string line in groupsVMC)
                    {
                        if (checkSize)
                        {
                            checkSize = false;
                            if (line == "32") currentVmcSize = 32;
                            else currentVmcSize = 8;
                        }
                        if (line.Contains("XEBP"))
                        {
                            currentGroup = line;
                            checkSize = true;
                        }
                        else if (line == serialID && !string.IsNullOrEmpty(currentGroup))
                        {
                            vmcFile = $"{currentGroup}_0.bin";
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(vmcFile))
                    {
                        Console.Write($"Failed to find a group for {serialID}");
                        return false;
                    }
                    vmcRelativePath = $"/VMC/{vmcFile}";
                    vmcFullPath = $"{gamePath}{vmcRelativePath}";
                }
                else
                {
                    vmcRelativePath = $"/VMC/{serialID}_0.bin";
                    vmcFullPath = $"{gamePath}{vmcRelativePath}";
                }
                gameListVMC.Add($"{friendlyName}|{serialID}|-bsd=udpbd|-dvd=mass:{game}|-mc0=mass:{vmcRelativePath}");
                if (!File.Exists(vmcFullPath))
                {
                    if (MiscMethods.CheckSpace($"BlankVMC{currentVmcSize}.bin", vmcFullPath))
                    {
                        File.Copy($"BlankVMC{currentVmcSize}.bin", vmcFullPath);
                        Thread.Sleep(200);
                        Console.WriteLine($"Created {vmcRelativePath} for {game}");
                    }
                    else
                    {
                        Console.WriteLine("Not enough space to create a new VMC file.");
                        return false;
                    }
                }
            }
            File.WriteAllLines("UDPBDList.txt", gameListVMC);
            Thread.Sleep(200);
            return true;
        }
    }
}
