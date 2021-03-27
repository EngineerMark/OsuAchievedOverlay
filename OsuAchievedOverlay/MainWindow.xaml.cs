using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using IniParser;
using IniParser.Model;
using Microsoft.Win32;
using Newtonsoft.Json;
using TimeAgo;

namespace OsuAchievedOverlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        

        public MainWindow()
        {
            InitializeComponent();

            Closed += (object sender, EventArgs e) =>
            {
                GameManager.Instance.Stop();
                Environment.Exit(0);
            };

            Start();
        }

        public void Start()
        {
            GameManager.Instance.MainWin = this;
            GameManager.Instance.Start();
        }

        private void ButtonHandler_OpenDisplay(object sender, RoutedEventArgs e)
        {
            GameManager.Instance.OpenDisplay();
        }

        private void btnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            GameManager.Instance.SettingsSave();
        }

        private void btnResetSession_Click(object sender, RoutedEventArgs e)
        {
            GameManager.Instance.RefreshSession();
        }

        private void btnSaveSession_Click(object sender, RoutedEventArgs e)
        {
            string json = GameManager.Instance.CurrentSession.ConvertToJson();

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt";
            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, json);
        }

        private void btnLoadSession_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true){
                //File.WriteAllText(saveFileDialog.FileName, json);
                var fileStream = openFileDialog.OpenFile();

                using (StreamReader reader = new StreamReader(fileStream))
                {
                    string data = reader.ReadToEnd();
                    if(data.Length>0){
                        Session newSession = null;
                        bool validSession = true;
                        try{
                            newSession = JsonConvert.DeserializeObject<Session>(data);
                        }catch (Exception)
                        {
                            validSession = false;
                            MessageBox.Show("Seems like the opened file is an invalid session file.", "Error opening session", MessageBoxButton.OK);
                        }
                        if (validSession)
                        {
                            GameManager.Instance.CurrentSession = newSession;
                            GameManager.Instance.RefreshTimer(null, null);
                        }
                    }
                }
            }
        }
    }
}
