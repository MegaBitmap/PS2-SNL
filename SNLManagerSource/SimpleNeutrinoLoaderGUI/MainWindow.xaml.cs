using FluentFTP;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;

namespace SimpleNeutrinoLoaderGUI
{
    public partial class MainWindow : Window
    {
        readonly string version = $"Version {Assembly.GetExecutingAssembly().GetName().Version} by MegaBitmap";
        const string helpUrl = "https://github.com/MegaBitmap/PS2-SNL?tab=readme-ov-file#udpbd-setup";
        readonly List<string> gameList = [];
        string gamePath = "";
        readonly string VHDXNameZip = "PS2-Games-exFAT-udpbd.zip";
        readonly string VHDXName = "PS2-Games-exFAT-udpbd.vhdx";
        readonly string traySettingsFile = "UDPBDTraySettings.txt";
        string VHDXLetter = "";

        public MainWindow()
        {
            InitializeComponent();
            CheckAlreadyRunning();
            CheckFiles();
            TextBlockVersion.Text = version;
            KillServer();
            LoadIPSetting();
            if (File.Exists(VHDXName))
            {
                VHDXLetter = InitVHDX(VHDXName);
            }
            if (!LoadGamePathSetting())
            {
                if (!CheckForExFat())
                {
                    ComboBoxServer.SelectedIndex = 1;
                }
            }
        }

        private async void ButtonInstall_ClickAsync(object sender, RoutedEventArgs e)
        {
            string locations;
            if (!TextBlockConnection.Text.Contains("Connected"))
            {
                MessageBox.Show("Please first connect to the PS2.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            try
            {
                AsyncFtpClient client = new(TextBoxPS2IP.Text);
                await client.Connect();
                await client.GetListing(); // for compatibility with ps2ftpd, reconnect every time FtpDataStream is used
                await client.Disconnect();
                await client.Connect();
                locations = await Install.GetStorageDevices(client);
                await client.Disconnect();
            }
            catch (Exception ex)
            {
                TextBlockConnection.Text = "Disconnected";
                ButtonConnect.IsEnabled = true;
                MessageBox.Show("Failed to connect to the PS2's FTP server.\n\n" +
                    $"{ex.Message}", "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(locations))
            {
                MessageBox.Show("Failed to find any memory cards or USB devices on the PS2's FTP server.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            InstallWindow installWindow = new(locations, TextBoxPS2IP.Text);
            installWindow.ShowDialog();
        }

        private static void CheckAlreadyRunning()
        {
            string pName = Process.GetCurrentProcess().ProcessName;
            int pCount = Process.GetProcessesByName(pName).Length;
            if (pCount > 1)
            {
                MessageBox.Show("This program is already running.", "Already Running", MessageBoxButton.OK, MessageBoxImage.Information);
                Environment.Exit(-1);
            }
        }

        private async void ButtonConnect_ClickAsync(object sender, RoutedEventArgs e)
        {
            ButtonConnect.IsEnabled = false;
            TextBlockConnection.Text = "Please Wait . . .";
            string tempIP = TextBoxPS2IP.Text;
            await PS2ConnectAsync(tempIP);
        }

        private void ButtonGamePath_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                Filter = "PS2 Games(*.iso;*.bin)|*.iso;*.bin",
                Title = "Select a game from the DVD or CD folder..."
            };
            bool? result = dialog.ShowDialog();
            if (result != true) return;
            if (!dialog.FileName.Contains(@"\DVD\" + dialog.SafeFileName) && !dialog.FileName.Contains(@"\CD\" + dialog.SafeFileName))
            {
                MessageBox.Show("Game files need to be in a folder named DVD or CD", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            gamePath = dialog.FileName.Replace(@"\DVD\" + dialog.SafeFileName, "").Replace(@"\CD\" + dialog.SafeFileName, "");
            GetGameList(gamePath);
        }

        private async void ButtonSync_Click(object sender, RoutedEventArgs e)
        {
            KillServer();
            if (await ValidateSyncAsync() != true) return;
            SaveGamePathSetting();
            string extraArgs = "";
            if (CheckBoxBinConvert.IsChecked == true)
            {
                extraArgs += " -bin2iso";
            }
            if (ComboBoxServer.SelectedIndex == 0 && CheckBoxVMC.IsChecked == true)
            {
                extraArgs += " -enablevmc";
            }
            Process process = new();
            process.StartInfo.FileName = "SNL-CLI.exe";
            process.StartInfo.Arguments = $"-path \"{gamePath}\" -ps2ip \"{TextBoxPS2IP.Text}\"{extraArgs}";
            process.Start();
        }

        private void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = helpUrl, UseShellExecute = true });
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            string? currentState = ButtonStart.Content.ToString();
            ButtonStart.Content = "Please Wait . . .";
            if (string.IsNullOrEmpty(currentState)) return;
            if (currentState.Contains("Stop"))
            {
                QuickKillServer();
                ButtonStart.Content = "Start Server";
                return;
            }
            string serverName;
            if (ComboBoxServer.SelectedIndex == 1)
            {
                serverName = "udpbd-vexfat";
                if (CheckServer(serverName))
                {
                    ButtonStart.Content = "Stop Server";
                    return;
                }
            }
            else
            {
                serverName = "udpbd-server";
                if (CheckServer(serverName))
                {
                    ButtonStart.Content = "Stop Server";
                    return;
                }
                string? tempGameDrive = ComboBoxGameVolume.SelectedItem.ToString();
                if (tempGameDrive == null) return;
                gamePath = SelectedVolume().Replace(tempGameDrive, "");
                GetGameList(gamePath);
            }
            if (gameList.Count == 0)
            {
                MessageBox.Show("The sync app was unable to find any games.\r\nPlease first select the game path.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ButtonStart.Content = "Start Server";
                return;
            }
            SaveTraySettings(serverName);

            Process process = new();
            process.StartInfo.FileName = "UDPBDTray.exe";
            process.Start();
            ButtonStart.Content = "Stop Server";
        }

        private void SaveTraySettings(string serverName)
        {
            string trayMountPath = gamePath;
            if (File.Exists(VHDXName))
            {
                if (!string.IsNullOrEmpty(VHDXLetter) && gamePath.Contains($"{VHDXLetter}:"))
                {
                    trayMountPath = VHDXName;
                }
            }
            using TextWriter traySettings = new StreamWriter(traySettingsFile);
            traySettings.WriteLine(trayMountPath);
            traySettings.WriteLine(serverName);
        }

        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new();
            aboutWindow.ShowDialog();
        }

        private bool LoadGamePathSetting()
        {
            if (!File.Exists("GamePathSetting.cfg")) return false;
            using TextReader settings = new StreamReader("GamePathSetting.cfg");
            string? tempPath = settings.ReadLine();
            string? server = settings.ReadLine();
            if (tempPath != null && Directory.Exists(tempPath))
            {
                GetGameList(tempPath);
                if (gameList.Count > 0)
                {
                    gamePath = tempPath;
                    if (!string.IsNullOrEmpty(server) && (server.Contains("VMCServer") || server.Contains("udpbd-server")))
                    {
                        ComboBoxServer.SelectedIndex = 0;
                        string? enableVMC = settings.ReadLine();
                        if ((!string.IsNullOrEmpty(enableVMC) && enableVMC.Contains("VMCServer")) || server.Contains("VMCServer"))
                        {
                            CheckBoxVMC.IsChecked = true;
                        }
                        CheckForExFat();
                        int itemNum = 0;
                        foreach (var item in ComboBoxGameVolume.Items)
                        {
                            string? tempItem = item.ToString();
                            if (tempItem != null && tempItem.Contains(tempPath))
                            {
                                ComboBoxGameVolume.SelectedIndex = itemNum;
                                return true;
                            }
                            itemNum++;
                        }
                    }
                    else
                    {
                        ComboBoxServer.SelectedIndex = 1;
                        gamePath = tempPath;
                        GetGameList(gamePath);
                    }
                    return true;
                }
            }
            return false;
        }

        private void SaveGamePathSetting()
        {
            using TextWriter settings = new StreamWriter("GamePathSetting.cfg");
            settings.WriteLine(gamePath);
            if (ComboBoxServer.SelectedIndex == 0)
            {
                settings.WriteLine("udpbd-server");
            }
            else
            {
                settings.WriteLine("udpbd-vexfat");
            }
            if (CheckBoxVMC.IsChecked == true && ComboBoxServer.SelectedIndex == 0)
            {
                settings.WriteLine("VMCServer");
            }
        }

        private void LoadIPSetting()
        {
            if (!File.Exists("IPSetting.cfg")) return;
            using TextReader settings = new StreamReader("IPSetting.cfg");
            string? tempIP = settings.ReadLine();
            if (!string.IsNullOrEmpty(tempIP)) TextBoxPS2IP.Text = tempIP;
        }

        private void SaveIPSetting()
        {
            using TextWriter settings = new StreamWriter("IPSetting.cfg");
            settings.WriteLine(TextBoxPS2IP.Text);
        }

        private async Task<bool> ValidateSyncAsync()
        {
            if (ComboBoxServer.SelectedIndex == 0)
            {
                string? tempGameDrive = ComboBoxGameVolume.SelectedItem.ToString();
                if (tempGameDrive == null) return false;
                gamePath = SelectedVolume().Replace(tempGameDrive, "");
                GetGameList(gamePath);
            }
            if (!TextBlockConnection.Text.Contains("Connected"))
            {
                MessageBox.Show("Please first connect to the PS2.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (gameList.Count == 0)
            {
                MessageBox.Show("The sync app was unable to find any games.\r\n" +
                    "Please first select the game path.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (!await PS2ConnectAsync(TextBoxPS2IP.Text))
            {
                TextBlockConnection.Text = "Disconnected";
                TextBoxPS2IP.IsEnabled = true;
                ButtonConnect.IsEnabled = true;
                return false;
            }
            return true;
        }

        private void GetGameList(string testPath)
        {
            KillServer();
            TextBlockGamesLoaded.Text = "";
            gameList.Clear();
            string[] scanFolders = [$"{testPath}/CD", $"{testPath}/DVD"];
            foreach (string folder in scanFolders)
            {
                if (Directory.Exists(folder))
                {
                    IEnumerable<string> ISOFiles = Directory.EnumerateFiles(folder, "*.iso", SearchOption.TopDirectoryOnly);
                    foreach (string ISOFile in ISOFiles) gameList.Add(ISOFile.Replace(testPath + @"\", ""));
                    IEnumerable<string> BINFiles = Directory.EnumerateFiles(folder, "*.bin", SearchOption.TopDirectoryOnly);
                    foreach (string BINFile in BINFiles)
                    {
                        string alreadyScanned = string.Join(" ", ISOFiles);
                        if (!alreadyScanned.Contains(Path.GetFileNameWithoutExtension(BINFile)))
                        {
                            gameList.Add(BINFile.Replace(testPath + @"\", ""));
                        }
                    }
                }
            }
            if (gameList.Count == 0) return;
            else if (gameList.Count == 1) TextBlockGamesLoaded.Text = gameList.Count + " Game Loaded";
            else TextBlockGamesLoaded.Text = gameList.Count + " Games Loaded";
        }

        private async Task<bool> PS2ConnectAsync(string ps2ip)
        {
            if (!IPAddress.TryParse(ps2ip, out IPAddress? address))
            {
                TextBlockConnection.Text = "Disconnected";
                ButtonConnect.IsEnabled = true;
                MessageBox.Show($"{ps2ip} is not a valid IP address.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            try
            {
                bool pingSuccess = false;
                for (int i = 0; i < 2; i++)
                {
                    Ping pingSender = new();
                    PingReply reply = await pingSender.SendPingAsync(address, 3000);
                    if (reply.Status == IPStatus.Success)
                    {
                        pingSuccess = true;
                    }
                }
                if (!pingSuccess)
                {
                    TextBlockConnection.Text = "Disconnected";
                    ButtonConnect.IsEnabled = true;
                    MessageBox.Show("Failed to receive a ping reply:\n\n" +
                        "Please verify that your network settings are configured properly and all cables are connected. " +
                        "Try adjusting the IP address settings in launchELF.\n\n",
                        "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                TextBlockConnection.Text = "Disconnected";
                ButtonConnect.IsEnabled = true;
                MessageBox.Show("The network location cannot be reached:\n\n" +
                    "Please verify that your network settings are configured properly and all cables are connected. " +
                    "Try manually assigning an IPv4 address and subnet mask to this PC.\n\n" +
                    $"{ex.Message}", "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            FtpListItem[] ftpList;
            try
            {
                AsyncFtpClient client = new(address.ToString());
                await client.Connect();
                ftpList = await client.GetListing();
                await client.Disconnect();
            }
            catch (Exception ex)
            {
                TextBlockConnection.Text = "Disconnected";
                ButtonConnect.IsEnabled = true;
                MessageBox.Show("Failed to connect to the PS2's FTP server.\n\n" +
                    $"{ex.Message}", "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            foreach (var item in ftpList)
            {
                if (item.Name.Contains("mc"))
                {
                    TextBlockConnection.Text = "Connected";
                    TextBoxPS2IP.IsEnabled = false;
                    ButtonConnect.IsEnabled = false;
                    SaveIPSetting();
                    return true;
                }
            }
            TextBlockConnection.Text = "Disconnected";
            ButtonConnect.IsEnabled = true;
            MessageBox.Show("Failed to connect to the PS2's FTP server.\n\n" +
                "No exceptions were raised.", "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        private bool CheckForExFat()
        {
            int numValidVolume = 0;
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveFormat.Equals("exFAT", StringComparison.OrdinalIgnoreCase))
                {
                    GetGameList(drive.ToString());
                    int numGames = gameList.Count;
                    ComboBoxGameVolume.Items.Add($"{drive}    {TextBlockGamesLoaded.Text}");
                    numValidVolume++;
                }
            }
            if (numValidVolume >= 1)
            {
                ComboBoxGameVolume.SelectedIndex = 0;
                return true;
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("The program was unable to find an exFAT volume or partition.\r\nDo you want to mount a Virtual Drive?", "exFAT not Found", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                {
                    return false;
                }
                if (!File.Exists(VHDXName))
                {
                    ZipFile.ExtractToDirectory(VHDXNameZip, Directory.GetCurrentDirectory());
                }
                VHDXLetter = InitVHDX(VHDXName);
                if (string.IsNullOrEmpty(VHDXLetter))
                {
                    MessageBox.Show($"Failed to mount the disk image '{VHDXName}'.", "Error Mounting VHDX", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(-1);
                }
                MessageBox.Show("The virtual drive has been mounted. Add your PS2 game ISOs to the DVD or CD folder then restart this sync app.", "Virtual Drive Mounted", MessageBoxButton.OK, MessageBoxImage.Information);
                Environment.Exit(0);
                return false;
            }
        }

        private async Task<bool> CheckForExFatAsync()
        {
            int numValidVolume = 0;
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveFormat.Equals("exFAT", StringComparison.OrdinalIgnoreCase))
                {
                    GetGameList(drive.ToString());
                    int numGames = gameList.Count;
                    ComboBoxGameVolume.Items.Add($"{drive}    {TextBlockGamesLoaded.Text}");
                    numValidVolume++;
                }
            }
            if (numValidVolume >= 1)
            {
                ComboBoxGameVolume.SelectedIndex = 0;
                return true;
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("The program was unable to find an exFAT volume or partition.\r\nDo you want to mount a Virtual Drive?", "exFAT not Found", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                {
                    return false;
                }
                if (!File.Exists(VHDXName))
                {
                    ZipFile.ExtractToDirectory(VHDXNameZip, Directory.GetCurrentDirectory());
                }
                VHDXLetter = await InitVHDXAsync(VHDXName);
                if (string.IsNullOrEmpty(VHDXLetter))
                {
                    MessageBox.Show($"Failed to mount the disk image '{VHDXName}'.", "Error Mounting VHDX", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(-1);
                }
                MessageBox.Show("The virtual drive has been mounted. Add your PS2 game ISOs to the DVD or CD folder then restart this sync app.", "Virtual Drive Mounted", MessageBoxButton.OK, MessageBoxImage.Information);
                Environment.Exit(0);
                return false;
            }
        }

        private static string InitVHDX(string fileName)
        {
            Process process = new();
            process.StartInfo.FileName = "powershell";
            process.StartInfo.Arguments = "-Command " +
                $"$p=Resolve-Path '{fileName}';" +
                "$d=Get-DiskImage $p;" +
                "if(-not$d.Attached){&$p;Start-Sleep .6;$d=Get-DiskImage $p}" +
                "(Get-Partition([string]$d.DevicePath[-1])).DriveLetter";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            process.WaitForExit();
            int testChar = process.StandardOutput.Peek();
            if (testChar == 0)
            {
                return "";
            }
            return process.StandardOutput.ReadLine() + "";
        }

        private static async Task<string> InitVHDXAsync(string fileName)
        {
            Process process = new();
            process.StartInfo.FileName = "powershell";
            process.StartInfo.Arguments = "-Command " +
                $"$p=Resolve-Path '{fileName}';" +
                "$d=Get-DiskImage $p;" +
                "if(-not$d.Attached){&$p;Start-Sleep .6;$d=Get-DiskImage $p}" +
                "(Get-Partition([string]$d.DevicePath[-1])).DriveLetter";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            await process.WaitForExitAsync();
            int testChar = process.StandardOutput.Peek();
            if (testChar == 0)
            {
                return "";
            }
            return process.StandardOutput.ReadLine() + "";
        }

        private static void CheckFiles()
        {
            string[] files = ["SNL-CLI.exe", "udpbd_server.dll", "udpbd_vexfat.dll", "PS2-Games-exFAT-udpbd.zip", "UDPBDTray.exe"];
            foreach (var file in files)
            {
                if (!File.Exists(file))
                {
                    MessageBox.Show($"The file {file} is missing.", "File Missing", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(-1);
                }
            }
        }

        private static bool CheckServer(string serverName)
        {
            Process[] processes = Process.GetProcessesByName(serverName);
            if (!(processes.Length == 0))
            {
                MessageBox.Show("The server is already running.", "Server is running", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            return false;
        }

        private void KillServer()
        {
            bool killAll = false;
            string[] serverNames = ["UDPBDTray", "udpbd-server", "udpbd-vexfat"];
            foreach (var server in serverNames)
            {
                Process[] processes = Process.GetProcessesByName(server);
                if (processes.Length != 0)
                {
                    if (killAll)
                    {
                        foreach (var item in processes) item.Kill();
                    }
                    else
                    {
                        MessageBoxResult response = MessageBox.Show("The server is currently running.\nClick OK to stop the server and sync.", "The server is running", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                        if (response == MessageBoxResult.OK)
                        {
                            killAll = true;
                            foreach (var item in processes) item.Kill(true);
                            ButtonStart.Content = "Start Server";
                        }
                        else
                        {
                            Environment.Exit(-1);
                        }
                    }
                }
            }
            if (killAll)
            {
                Thread.Sleep(200);
            }
        }

        private static void QuickKillServer()
        {
            string[] serverNames = ["UDPBDTray", "udpbd-server", "udpbd-vexfat"];
            foreach (var server in serverNames)
            {
                Process[] processes = Process.GetProcessesByName(server);
                if (processes.Length != 0)
                {
                    foreach (var item in processes) item.Kill();
                }
            }
        }

        private async void ComboBoxServer_SelectionChangedAsync(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            gameList.Clear();
            gamePath = "";
            if (TextBlockGamesLoaded == null) return;
            ComboBoxGameVolume.Items.Clear();
            TextBlockGamesLoaded.Text = "";
            if (ComboBoxServer.SelectedIndex == 1)
            {
                SelectVexfat();
            }
            else
            {
                if (!await CheckForExFatAsync())
                {
                    ComboBoxServer.SelectedIndex = 1;
                    return;
                }
                SelectUServer();
            }
        }

        private void SelectVexfat()
        {
            ButtonGamePath.Visibility = Visibility.Visible;
            TextBlockGamesLoaded.Visibility = Visibility.Visible;
            ServerNote.Visibility = Visibility.Hidden;
            ComboBoxGameVolume.Visibility = Visibility.Hidden;
            CheckBoxVMC.Visibility = Visibility.Hidden;
        }

        private void SelectUServer()
        {
            ButtonGamePath.Visibility = Visibility.Hidden;
            TextBlockGamesLoaded.Visibility = Visibility.Hidden;
            ServerNote.Visibility = Visibility.Visible;
            ComboBoxGameVolume.Visibility = Visibility.Visible;
            CheckBoxVMC.Visibility = Visibility.Visible;
        }

        [GeneratedRegex(@"\\.*")]
        private static partial Regex SelectedVolume();
    }
}
