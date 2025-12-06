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

        void StartCLIInstall(string target)
        {
            Process process = new();
            process.StartInfo.FileName = "SNL-CLI.exe";
            string bootFlag = GetBoot();
            process.StartInfo.Arguments = $"-install {target} -ps2ip \"{ps2ip}\"{bootFlag}";
            process.Start();
            Close();
        }

        string GetBoot()
        {
            bool? isChecked = CheckBoxAutorun.IsChecked;
            if ( isChecked != null && (bool)isChecked)
            {
                return " -boot";
            }
            return "";
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
            StartCLIInstall("mc0");
        }

        private void ButtonMC1_Click(object sender, RoutedEventArgs e)
        {
            StartCLIInstall("mc1");
        }

        private void ButtonMass_Click(object sender, RoutedEventArgs e)
        {
            StartCLIInstall("mass");
        }
    }
}
