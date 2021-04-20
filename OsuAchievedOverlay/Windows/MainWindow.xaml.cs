using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;

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
                SessionManager.Instance.Stop();
                Environment.Exit(0);
            };

            Start();
        }

        public void Start()
        {
            WindowManager.Instance.MainWin = this;
            GameManager.Instance.Start();
        }

        private void ButtonHandler_OpenDisplay(object sender, RoutedEventArgs e)
        {
            GameManager.Instance.FocusDisplay();
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
            {
                File.WriteAllText(saveFileDialog.FileName, json);
                SessionManager.Instance.AddFile(saveFileDialog.FileName);
            }
        }

        private void btnLoadSession_Click(object sender, RoutedEventArgs e)
        {
            if(WindowManager.Instance.SessionWin==null){
                WindowManager.Instance.SessionWin = new LoadSessionWindow();
                WindowManager.Instance.SessionWin.Show();
                WindowManager.Instance.SessionWin.Focus();
            }
        }

        private void btnOpenApiManager_Click(object sender, RoutedEventArgs e)
        {
            if (WindowManager.Instance.ApiWin == null)
            {
                WindowManager.Instance.ApiWin = new LocalApiWindow();
                WindowManager.Instance.ApiWin.Show();
            }
            WindowManager.Instance.ApiWin.Focus();
        }

        private void InputNumericOnly(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
