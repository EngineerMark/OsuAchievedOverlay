using IniParser;
using IniParser.Model;
using OsuAchievedOverlay.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OsuAchievedOverlay
{
    /// <summary>
    /// Interaction logic for PopupSetAPI.xaml
    /// </summary>
    public partial class PopupSetAPI : Window
    {
        public PopupSetAPI()
        {
            InitializeComponent();

            Closed += (object sender, EventArgs e) =>
            {
                Application.Current.Shutdown();
            };
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Btn_Close(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Btn_Minimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Btn_Apply(object sender, RoutedEventArgs e)
        {
            FileIniDataParser parser = new FileIniDataParser();
            if (File.Exists("Settings.ini"))
            {
                IniData data = parser.ReadFile("Settings.ini");
                data["api"]["key"] = InputApiKey.Password;
                data["api"]["user"] = InputUsername.Text;

                parser.WriteFile("Settings.ini", data);
            }
            else
            {
                IniData newData = new IniData();

                newData = SettingsManager.FixIniData(parser, newData);

                newData["api"]["key"] = InputApiKey.Password;
                newData["api"]["user"] = InputUsername.Text;

                parser.WriteFile("Settings.ini", newData);
            }

            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void Link_OpenURL(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
