using IniParser;
using IniParser.Model;
using OsuAchievedOverlay.Managers;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for LaunchWindow.xaml
    /// </summary>
    public partial class LaunchWindow : Window
    {
        public LaunchWindow(StartupEventArgs e)
        {
            StartupEventArgs t = e;
            InitializeComponent();
            Show();
            Focus();

            TestSettings();
        }

        public void TestSettings()
        {
            FileIniDataParser parser = new FileIniDataParser();
            if (File.Exists("Settings.ini"))
            {
                IniData data = parser.ReadFile("Settings.ini");
                string key = data["api"]["key"];
                OsuApiHelper.OsuApiKey.Key = key;
                if (!OsuApiHelper.OsuApi.IsKeyValid()){
                    OpenPopup();
                }else{
                    // API key is valid, lets head on further

                    GameManager.Instance.Start();

                    Close();
                }
            }
            else
            {
                IniData newData = new IniData();

                newData = SettingsManager.FixIniData(parser, newData);
                parser.WriteFile("Settings.ini", newData);

                OpenPopup();
            }
        }

        private void OpenPopup(){
            PopupSetAPI popup = new PopupSetAPI();
            popup.Show();
            popup.Focus();
        }
    }
}
