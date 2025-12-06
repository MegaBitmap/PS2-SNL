using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace UDPBDTray
{
    internal partial class TrayNotifyIcon : ApplicationContext
    {
        private readonly NotifyIcon notifyIcon;
        private readonly ContextMenuStrip contextMenu;
        private readonly ToolStripMenuItem menuItemOpenSync;
        private readonly ToolStripMenuItem menuItemConsoleToggle;
        private readonly ToolStripMenuItem menuItemKill;
        private readonly IContainer components;

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool AllocConsole();
        [LibraryImport("kernel32.dll", SetLastError = true)]
        private static partial nint GetConsoleWindow();
        [LibraryImport("kernel32.dll", SetLastError = true)]
        private static partial nint GetStdHandle(int nStdHandle);
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetConsoleMode(nint hConsoleHandle, uint dwMode);
        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ShowWindow(nint hWnd, int nCmdShow);

        private bool showConsole = false;
        private string serverName = "udpbd-server";
        private string gamePath = "FAILED TO SET GAMEPATH";
        private bool isActive = false;
        private readonly string syncApp = "SimpleNeutrinoLoaderGUI";
        private readonly int listenPort = 0x4712;

        [LibraryImport("udpbd_server.dll", StringMarshalling = StringMarshalling.Utf8, SetLastError = true), ]
        private static partial int Run_udpbd_server(string path);
        [LibraryImport("udpbd_vexfat.dll", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
        private static partial int run_vexfat_server(string path);

        public TrayNotifyIcon()
        {
            components = new Container();
            contextMenu = new();
            menuItemOpenSync = new();
            menuItemConsoleToggle = new();
            menuItemKill = new();
            notifyIcon = new(components);

            CheckAlreadyRunning();
            SilentKillServer();
            CheckFiles();
            LoadSettings("UDPBDTraySettings.txt");
            GetConsole();
            InitNotifyIcon();
            isActive = true;
        }

        private void CheckAlreadyRunning()
        {
            string pName = Process.GetCurrentProcess().ProcessName;
            int pCount = Process.GetProcessesByName(pName).Length;
            if (pCount > 1)
            {
                isActive = false;
                MessageBox.Show("This program is already running.", "Already Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Environment.Exit(0);
            }
        }

        private void InitNotifyIcon()
        {
            contextMenu.Items.Add(menuItemOpenSync);
            contextMenu.Items.Add(menuItemConsoleToggle);
            contextMenu.Items.Add(menuItemKill);

            menuItemOpenSync.Text = "Open Sync App/Change Server Settings";
            menuItemOpenSync.Click += new EventHandler(MenuItemOpenSync_Click);
            menuItemConsoleToggle.Text = "Show Server Console";
            menuItemConsoleToggle.Click += new EventHandler(MenuItemConsoleToggle_Click);
            menuItemConsoleToggle.CheckOnClick = true;

            // use menuItem to create events
            menuItemKill.TextChanged += new EventHandler(StartServerAsync);
            menuItemKill.TextChanged += new EventHandler(PS2ListenAsync);
            menuItemKill.TextChanged += new EventHandler(ServerStartBaloonAsync);

            // update menuItemText to start running async events
            menuItemKill.Text = "Stop Server and Exit";
            menuItemKill.Click += new EventHandler(MenuItemKill_Click);

            notifyIcon.Icon = Properties.Resources.Icon;
            notifyIcon.ContextMenuStrip = contextMenu;
            notifyIcon.Text = $"{serverName} is Running";
            notifyIcon.Visible = true;
            notifyIcon.MouseUp += new MouseEventHandler(NotifyIcon_Click);
        }

        private async void PS2ListenAsync(object? sender, EventArgs e)
        {
            await Task.Delay(1000);
            if (!isActive) return;
            IPEndPoint ipEndPoint = new(IPAddress.Any, listenPort);
            using Socket ps2sock = new(ipEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                ps2sock.Bind(ipEndPoint);
                Console.WriteLine($"Listening to PS2 console output on port {listenPort} (0x{listenPort:X})");
                while (true)
                {
                    byte[] buffer = new byte[512];
                    int numBytes = await ps2sock.ReceiveAsync(buffer);
                    Console.Write(Encoding.UTF8.GetString(buffer, 0, numBytes));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error has occured in PS2ListenAsync.\n\n" +
                    $"{ex.Message}\n\n{ex}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void StartServerAsync(object? sender, EventArgs e)
        {
            Process[] UCLIProcess = Process.GetProcessesByName("UDPBD-for-XEB+-CLI");
            Process[] SCLIProcess = Process.GetProcessesByName("SNL-CLI");
            if (UCLIProcess.Length != 0 || SCLIProcess.Length != 0)
            {
                MessageBox.Show("Please close SNL-CLI and UDPBD-for-XEB+-CLI while UDPBDTray is running.",
                    "CLI is Running", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(-1);
            }
            Console.WriteLine("Starting Server . . .");
            Func<int> serverFunc;
            if (serverName.Contains("vexfat"))
            {
                serverFunc = new Func<int>(() => run_vexfat_server(gamePath));
            }
            else
            {
                serverFunc = new Func<int>(() => Run_udpbd_server($"\\\\.\\{gamePath}"));
            }
            // server starts here v
            int rValue = await Task.Run(serverFunc);            
            isActive = false;
            if (rValue == 5)
            {
                RestartAdmin();
            }
            EditConsole();
            string errorMessage = "";
            if ( rValue > 0)
            {
                Win32Exception ex = new(rValue);
                errorMessage = $"This might be caused by the following:\n\n{ex.Message}";
            }
            MessageBox.Show($"Server stopped unexpectedly with a return value of {rValue}\n\n" + errorMessage,
                "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(rValue);
        }

        private void MenuItemOpenSync_Click(object? sender, EventArgs e)
        {
            if (!File.Exists($"{syncApp}.exe"))
            {
                MessageBox.Show($"Unable to locate {syncApp}", "Sync app missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (Process.GetProcessesByName(syncApp).Length != 0)
            {
                MessageBox.Show("The sync app is already running.", "Already Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                SilentKillServer();
                Process syncProcess = new();
                syncProcess.StartInfo.FileName = syncApp;
                syncProcess.Start();
                Environment.Exit(0);
            }
        }

        private void CheckFiles()
        {
            string[] files = ["udpbd_server.dll", "udpbd_vexfat.dll"];
            foreach (var file in files)
            {
                if (!File.Exists(file))
                {
                    isActive = false;
                    MessageBox.Show($"The file {file} is missing.", "File Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(-1);
                }
            }
        }

        private void LoadSettings(string settingsFile)
        {
            if (!File.Exists(settingsFile))
            {
                isActive = false;
                MessageBox.Show("Error the settings file 'UDPBDTraySettings.txt' does not exist.",
                    "Error Reading Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
            using TextReader settingsReader = new StreamReader(settingsFile);
            string? tempPath = settingsReader.ReadLine();
            string? tempServer = settingsReader.ReadLine();
            if (string.IsNullOrEmpty(tempPath) || string.IsNullOrEmpty(tempServer))
            {
                isActive = false;
                MessageBox.Show("Failed to read the settings file 'UDPBDTraySettings.txt'",
                    "Error Reading Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
            serverName = tempServer;
            if (tempPath.Contains(".vhdx") && File.Exists(tempPath))
            {
                string driveLetter = InitVHDX(tempPath);
                if (string.IsNullOrEmpty(driveLetter))
                {
                    MessageBox.Show($"Failed to mount the disk image '{tempPath}'",
                        "Error Mounting VHDX", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(-1);
                }
                gamePath = $"{driveLetter}:";
            }
            else
            {
                if (!Path.Exists(tempPath))
                {
                    isActive = false;
                    MessageBox.Show($"Error the file path '{tempPath}' does not exist.",
                        "Error finding path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(-1);
                }
                gamePath = tempPath;
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

        private void NotifyIcon_Click(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MethodInfo? mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                mi?.Invoke(notifyIcon, null);
            }
        }

        private void MenuItemConsoleToggle_Click(object? sender, EventArgs e)
        {
            showConsole = !showConsole;
            if (showConsole)
            {
                ShowWindow(GetConsoleWindow(), 5);
            }
            else
            {
                ShowWindow(GetConsoleWindow(), 0);
            }
        }

        private void MenuItemKill_Click(object? sender, EventArgs e)
        {
            isActive = false;
            QuickKillServer();
            Environment.Exit(0);
        }

        private static void QuickKillServer()
        {
            string[] serverNames = ["udpbd-server", "udpbd-vexfat"];
            foreach (var server in serverNames)
            {
                Process[] processes = Process.GetProcessesByName(server);
                if (processes.Length != 0)
                {
                    foreach (var item in processes) item.Kill();
                }
            }
        }

        private static void SilentKillServer()
        {
            string[] serverNames = ["udpbd-server", "udpbd-vexfat"];
            foreach (var server in serverNames)
            {
                Process[] processes = Process.GetProcessesByName(server);
                if (processes.Length != 0)
                {
                    foreach (var item in processes) item.Kill();
                }
            }
            Thread.Sleep(200);
        }

        private async void ServerStartBaloonAsync(object? sender, EventArgs e)
        {
            await Task.Delay(4000); // wait for the server to start before checking if it failed
            if (isActive)
            {
                notifyIcon.ShowBalloonTip(10000, $"{serverName} is Active!", "The PS2 game server is ready to Play!", ToolTipIcon.None);
            }
        }

        private static void RestartAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal pricipal = new(identity);
            if (!pricipal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                try
                {
                    Process process = new();
                    process.StartInfo.Verb = "runas";
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = Environment.ProcessPath;
                    process.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error has occured while trying to run as Administrator.\n\n" +
                        $"{ex.Message}\n\n{ex}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Environment.Exit(5);
            }
        }

        private static void GetConsole()
        {
            const uint ENABLE_QUICK_EDIT = 0x0040;
            const int STD_INPUT_HANDLE = -10;
            AllocConsole();
            ShowWindow(GetConsoleWindow(), 0);
            nint consoleHandle = GetStdHandle(STD_INPUT_HANDLE);
            if (!GetConsoleMode(consoleHandle, out uint consoleMode))
            {
                return;
            }
            consoleMode &= ~ENABLE_QUICK_EDIT;
            if (!SetConsoleMode(consoleHandle, consoleMode))
            {
                return;
            }
        }

        private void EditConsole()
        {
            const uint ENABLE_QUICK_EDIT = 0x0040;
            const int STD_INPUT_HANDLE = -10;
            showConsole = true;
            menuItemConsoleToggle.Checked = true;
            ShowWindow(GetConsoleWindow(), 5);
            nint consoleHandle = GetStdHandle(STD_INPUT_HANDLE);
            if (!GetConsoleMode(consoleHandle, out uint consoleMode))
            {
                return;
            }
            consoleMode |= ENABLE_QUICK_EDIT;
            if (!SetConsoleMode(consoleHandle, consoleMode))
            {
                return;
            }
        }
    }
}
