using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using FluentFTP;
using Microsoft.Win32;

namespace SimpleNeutrinoLoaderGUI
{
    public partial class MainWindow : Window
    {
        readonly string version = $"Version {Assembly.GetExecutingAssembly().GetName().Version} by MegaBitmap";
        const string helpUrl = "https://github.com/MegaBitmap/PS2-SNL?tab=readme-ov-file#udpbd-setup";
        readonly List<string> gameList = [];
        string gamePath = "";

        public MainWindow()
        {
            InitializeComponent();
            TextBlockVersion.Text = version;
            KillServer();
            LoadIPSetting();
            LoadGamePathSetting();
            CheckFiles();
        }

        private async void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            ButtonConnect.IsEnabled = false;
            TextBlockConnection.Text = "Please Wait . . .";
            string tempIP = TextBoxPS2IP.Text;
            await PS2ConnectAsync(tempIP);
        }

        private async void ButtonInstall_Click(object sender, RoutedEventArgs e)
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
                Thread.Sleep(200);
                await client.GetListing();
                Thread.Sleep(200);
                locations = await Install.GetStorageDevices(client);
                Thread.Sleep(200);
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

        private void ComboBoxServer_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            gameList.Clear();
            gamePath = "";
            if (TextBlockGamesLoaded == null) return;
            ComboBoxGameVolume.Items.Clear();
            TextBlockGamesLoaded.Text = "";
            if (ComboBoxServer.SelectedIndex == 0)
            {
                ButtonGamePath.Visibility = Visibility.Visible;
                TextBlockGamesLoaded.Visibility = Visibility.Visible;
                ServerNote.Visibility = Visibility.Hidden;
                ComboBoxGameVolume.Visibility = Visibility.Hidden;
                CheckBoxVMC.Visibility = Visibility.Hidden;
            }
            else
            {
                if (!CheckForExFat())
                {
                    ComboBoxServer.SelectedIndex = 0;
                    ButtonGamePath.Visibility = Visibility.Visible;
                    TextBlockGamesLoaded.Visibility = Visibility.Visible;
                    ServerNote.Visibility = Visibility.Hidden;
                    ComboBoxGameVolume.Visibility = Visibility.Hidden;
                    CheckBoxVMC.Visibility = Visibility.Hidden;
                    return;
                }
                ButtonGamePath.Visibility = Visibility.Hidden;
                TextBlockGamesLoaded.Visibility = Visibility.Hidden;
                ServerNote.Visibility = Visibility.Visible;
                ComboBoxGameVolume.Visibility = Visibility.Visible;
                CheckBoxVMC.Visibility = Visibility.Visible;
            }
        }

        private void ButtonGamePath_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                Filter = "PS2 ISO Files | *.iso",
                Title = "Select a game from the DVD folder..."
            };
            bool? result = dialog.ShowDialog();
            if (result != true) return;
            if (!dialog.FileName.Contains(@"\DVD\" + dialog.SafeFileName) && !dialog.FileName.Contains(@"\CD\" + dialog.SafeFileName))
            {
                MessageBox.Show("Game ISOs need to be in a folder named DVD or CD", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (ComboBoxServer.SelectedIndex == 1 && CheckBoxVMC.IsChecked == true)
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

        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new();
            aboutWindow.ShowDialog();
        }

        private void CheckBoxConsole_Checked(object sender, RoutedEventArgs e)
        {
            string? serverStatus = ButtonStart.Content.ToString();
            if (serverStatus == null) return;
            if (serverStatus.Contains("Stop"))
            {
                MessageBox.Show("Please restart the server to show the console.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            string? currentState = ButtonStart.Content.ToString();
            if (string.IsNullOrEmpty(currentState)) return;
            if (currentState.Contains("Stop"))
            {
                QuickKillServer();
                ButtonStart.Content = "Start Server";
                return;
            }
            string serverName;
            if (ComboBoxServer.SelectedIndex == 0)
            {
                serverName = "udpbd-vexfat";
                if (CheckServer(serverName)) return;
            }
            else
            {
                serverName = "udpbd-server";
                if (CheckServer(serverName)) return;
                string? tempGameDrive = ComboBoxGameVolume.SelectedItem.ToString();
                if (tempGameDrive == null) return;
                gamePath = SelectedVolume().Replace(tempGameDrive, "");
                GetGameList(gamePath);
            }
            if (gameList.Count == 0)
            {
                MessageBox.Show("Please first select the game folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Process process = new();
            process.StartInfo.FileName = "cmd.exe";
            if (serverName.Contains("vexfat"))
            {
                process.StartInfo.Arguments = $"/K {serverName} \"{gamePath}\"";
                if (CheckBoxConsole.IsChecked != true)
                {
                    process.StartInfo.FileName = serverName;
                    process.StartInfo.Arguments = $"\"{gamePath}\"";
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                }
            }
            else
            {
                process.StartInfo.Arguments = $"/K \"{Path.GetFullPath(serverName)}\" \\\\.\\{gamePath}";
                if (CheckBoxConsole.IsChecked != true)
                {
                    process.StartInfo.FileName = serverName;
                    process.StartInfo.Arguments = $"\\\\.\\{gamePath}";
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                }
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.Verb = "runas";
            }
            process.Start();
            if (CheckBoxConsole.IsChecked != true)
            {
                CheckServerStart(serverName);
            }
            ButtonStart.Content = "Stop Server";
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
                Ping pingSender = new();
                PingReply reply = await pingSender.SendPingAsync(address, 6000);
                if (!(reply.Status == IPStatus.Success))
                {
                    TextBlockConnection.Text = "Disconnected";
                    ButtonConnect.IsEnabled = true;
                    MessageBox.Show("Failed to receive a ping reply:\n\n" +
                        "Please verify that your network settings are configured properly and all cables are connected. " +
                        "Try adjusting the IP address settings in launchELF.\n\n" +
                        $"{reply.Status}", "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
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
                Thread.Sleep(200);
                ftpList = await client.GetListing();
                Thread.Sleep(200);
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

        private async Task<bool> ValidateSyncAsync()
        {
            if (ComboBoxServer.SelectedIndex == 1)
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
                MessageBox.Show("Please first select the game folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void SaveIPSetting()
        {
            TextWriter settings = new StreamWriter("IPSetting.cfg");
            settings.WriteLine(TextBoxPS2IP.Text);
            settings.Close();
        }

        private void LoadIPSetting()
        {
            if (!File.Exists("IPSetting.cfg")) return;
            TextReader settings = new StreamReader("IPSetting.cfg");
            string? tempIP = settings.ReadLine();
            settings.Close();
            if (!string.IsNullOrEmpty(tempIP)) TextBoxPS2IP.Text = tempIP;
        }

        private void SaveGamePathSetting()
        {
            TextWriter settings = new StreamWriter("GamePathSetting.cfg");
            settings.WriteLine(gamePath);
            if (CheckBoxVMC.IsChecked == true && ComboBoxServer.SelectedIndex == 1)
            {
                settings.WriteLine("VMCServer");
            }
            settings.Close();
        }

        private void LoadGamePathSetting()
        {
            if (!File.Exists("GamePathSetting.cfg")) return;
            TextReader settings = new StreamReader("GamePathSetting.cfg");
            string? tempPath = settings.ReadLine();
            string? serveVMC = settings.ReadLine();
            settings.Close();
            if (tempPath != null && Directory.Exists(tempPath))
            {
                GetGameList(tempPath);
                if (gameList.Count > 0) gamePath = tempPath;

                if (!string.IsNullOrEmpty(serveVMC) && serveVMC.Contains("VMCServer"))
                {
                    ComboBoxServer.SelectedIndex = 1;
                    CheckBoxVMC.IsChecked = true;
                    int itemNum = 0;
                    foreach (var item in ComboBoxGameVolume.Items)
                    {
                        string? tempItem = item.ToString();
                        if (tempItem != null && tempItem.Contains(tempPath))
                        {
                            ComboBoxGameVolume.SelectedIndex = itemNum;
                            return;
                        }
                        itemNum++;
                    }
                }
            }
        }

        private void GetGameList(string testPath)
        {
            KillServer();
            TextBlockGamesLoaded.Text = "";
            gameList.Clear();
            string[] scanFolders = [$"{testPath}/CD", $"{testPath}/DVD"];
            foreach (var item in scanFolders)
            {
                if (Directory.Exists(item))
                {
                    IEnumerable<string> ISOFiles = Directory.EnumerateFiles(item, "*.iso", SearchOption.TopDirectoryOnly);
                    foreach (string file in ISOFiles) gameList.Add(file.Replace(testPath + @"\", ""));
                }
            }
            if (gameList.Count == 0) return;
            else if (gameList.Count == 1) TextBlockGamesLoaded.Text = gameList.Count + " Game Loaded";
            else TextBlockGamesLoaded.Text = gameList.Count + " Games Loaded";
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
                MessageBox.Show("The program was unable to find an exFAT volume or partition.\n" +
                    "The exFAT partition needs to be created in Linux.\n" +
                    "See README for more details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void KillServer()
        {
            string[] serverNames = ["udpbd-server", "udpbd-vexfat"];
            foreach (var server in serverNames)
            {
                Process[] processes = Process.GetProcessesByName(server);
                if (!(processes.Length == 0))
                {
                    MessageBoxResult response = MessageBox.Show("The server is currently running.\n" +
                        "Click OK to stop the server and sync.", "The server is running", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                    if (response == MessageBoxResult.OK)
                    {
                        foreach (var item in processes) item.Kill();
                        ButtonStart.Content = "Start Server";
                    }
                    else Environment.Exit(-1);
                }
            }
        }

        private static void QuickKillServer()
        {
            bool hasKilled = false;
            string[] serverNames = ["udpbd-server", "udpbd-vexfat"];
            foreach (var server in serverNames)
            {
                Process[] processes = Process.GetProcessesByName(server);
                if (!(processes.Length == 0))
                {
                    hasKilled = true;
                    foreach (var item in processes) item.Kill();
                }
            }
            if (!hasKilled)
            {
                MessageBox.Show("The server was not running.", "Server is stopped", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else MessageBox.Show("The server was stopped.", "Server is stopped", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private static void CheckServerStart(string serverName)
        {
            Thread.Sleep(1000); //wait 1 second for the server to start before checking if it failed
            Process[] processesStarted = Process.GetProcessesByName(serverName);
            if (processesStarted.Length != 0)
            {
                MessageBox.Show("The server is now running and ready to Play!", "Server is running", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else MessageBox.Show("Failed to start the server.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static void CheckFiles()
        {
            string[] files = ["SNL-CLI.exe", "udpbd-server.exe", "udpbd-vexfat.exe"];
            foreach (var file in files)
            {
                if (!File.Exists(file))
                {
                    MessageBox.Show($"The file {file} is missing.", "File Missing", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(-1);
                }
            }
        }

        [GeneratedRegex(@"\\.*")]
        private static partial Regex SelectedVolume();
    }
}
