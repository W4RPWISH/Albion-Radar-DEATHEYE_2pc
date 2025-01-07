using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace X975.Pages
{
    [Obfuscation(Feature = "mutation", Exclude = false)]
    public partial class SupportPage : Page
    {
        public SupportPage()
        {
            InitializeComponent();
        }

        private void Links(object sender, RoutedEventArgs e)
        {
            switch ((e.Source as Button).Tag)
            {
                case "Discord":
                    System.Diagnostics.Process.Start("https://discord.gg/8byNr7TDma");
                    break;

                case "Website":
                    System.Diagnostics.Process.Start("https://deatheye.cc");
                    break;

                case "Youtube":
                    System.Diagnostics.Process.Start("https://www.youtube.com/@W4RPWISH");
                    break;
            }
        }
    }
}
