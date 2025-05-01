using System.Diagnostics;
using System.Windows;

namespace SimpleNeutrinoLoaderGUI
{
    public partial class InstallWindow : Window
    {
        readonly string ps2ip = "";

        public InstallWindow(string locations, string tempIP)
        {
            InitializeComponent();
            ps2ip = tempIP;
            PopulateInstallLocations(locations);
        }

        void PopulateInstallLocations(string locations)
        {
            if (locations.Contains("mc0"))
            {
                ButtonMC0.IsEnabled = true;
            }
            if (locations.Contains("mc1"))
            {
                ButtonMC1.IsEnabled = true;
            }
            if (locations.Contains("mass"))
            {
                ButtonMass.IsEnabled = true;
            }
        }

        private void ButtonMC0_Click(object sender, RoutedEventArgs e)
        {
            Process process = new();
            process.StartInfo.FileName = "SNL-CLI.exe";
            process.StartInfo.Arguments = $"-install mc0 -ps2ip \"{ps2ip}\" -boot";
            process.Start();
            Close();
        }

        private void ButtonMC1_Click(object sender, RoutedEventArgs e)
        {
            Process process = new();
            process.StartInfo.FileName = "SNL-CLI.exe";
            process.StartInfo.Arguments = $"-install mc1 -ps2ip \"{ps2ip}\" -boot";
            process.Start();
            Close();
        }

        private void ButtonMass_Click(object sender, RoutedEventArgs e)
        {
            Process process = new();
            process.StartInfo.FileName = "SNL-CLI.exe";
            process.StartInfo.Arguments = $"-install mass -ps2ip \"{ps2ip}\" -boot";
            process.Start();
            Close();
        }
    }
}
