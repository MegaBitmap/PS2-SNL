using System.Windows;

namespace SimpleNeutrinoLoaderGUI
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            TextBoxAbout.Text = "" +
                "Big Thanks to these Developers!\n\n" +
                "Alex Parrado & Matías Israelson & Rick Gaiser - udpbd-server\n" +
                "github.com/israpps/udpbd-server\n\n" +
                "awaken1ng - udpbd-vexfat\n" +
                "github.com/awaken1ng/udpbd-vexfat\n\n" +
                "Daniel Santos - Enceladus\n" +
                "github.com/DanielSant0s/Enceladus\n\n" +
                "Matías Israelson - PS2-Basic-Bootloader\n" +
                "github.com/israpps/PlayStation2-Basic-BootLoader\n\n" +
                "Rick Gaiser - neutrino\n" +
                "github.com/rickgaiser/neutrino\n\n" +
                "sync-on-luma - XEB+ neutrino Launcher Plugin\n" +
                "github.com/sync-on-luma/xebplus-neutrino-loader-plugin";
        }
    }
}
