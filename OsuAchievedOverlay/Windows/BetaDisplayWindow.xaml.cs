using Humanizer;
using Microsoft.Win32;
using OsuAchievedOverlay.Managers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace OsuAchievedOverlay
{
    /// <summary>
    /// Interaction logic for BetaDisplayWindow.xaml
    /// </summary>
    public partial class BetaDisplayWindow : Window
    {
        public BetaDisplayWindow()
        {
            InitializeComponent();

            Closed += (object sender, EventArgs e) =>
            {
                GameManager.Instance.Stop();
            };

            GridBackdrop.Visibility = Visibility.Hidden;
            SidepanelGrid.Visibility = Visibility.Hidden;
        }

        private void Update()
        {
            //if (LastSessionUpdate != UpdateSession.Key)
            //{
            //    ApplySession(UpdateSession.Value);
            //    LastSessionUpdate = UpdateSession.Key;
            //}
        }

        private void Btn_Close(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Btn_Minimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Btn_OpenSettings(object sender, RoutedEventArgs e)
        {
            //if (WindowManager.Instance.SettingsWin == null)
            //{
            //    WindowManager.Instance.SettingsWin = new SettingsWindow();
            //    WindowManager.Instance.SettingsWin.Show();
            //}
            //WindowManager.Instance.SettingsWin.Focus();
            GridSettings.PopulateData();
            InterfaceManager.Instance.AnimateOpacity(GridBackdrop, 0, 0.3, 0.4);
            InterfaceManager.Instance.AnimateOpacity(SidepanelGrid, 0, 1, 0.3);
            GridSettings.Visibility = Visibility.Visible;
        }

        private void Btn_SettingsClosed(object sender, EventArgs e)
        {
            CloseSidepanel();
        }

        private void CloseSidepanel(){
            InterfaceManager.Instance.AnimateOpacity(GridBackdrop, 0.3, 0, 0.4, new Action(() =>
            {
                GridBackdrop.Visibility = Visibility.Collapsed;
            }));
            Storyboard sb = InterfaceManager.Instance.AnimateOpacity(SidepanelGrid, 1, 0, 0.3, new Action(() =>
            {
                SidepanelGrid.Visibility = Visibility.Collapsed;
            }));
            sb.Completed += delegate (object s, EventArgs e)
            {
                GridSettings.Visibility = Visibility.Hidden;
            };
        }

        private void BackdropGrid_ClickOutside(object sender, MouseButtonEventArgs e)
        {
            if (GridBackdrop.IsMouseOver)
            {
                CloseSidepanel();
            }
        }

        private void btnSaveSession_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("Do you want to save this session as read-only?", "Read-only mode", MessageBoxButton.YesNo);

            Session clonedSession = (Session)SessionManager.Instance.CurrentSession.Clone();
            if (res == MessageBoxResult.Yes)
            {
                clonedSession.ReadOnly = true;
            }
            clonedSession.Username = GameManager.Instance.OsuUser.Name;
            if (clonedSession.ReadOnly && clonedSession.SessionEndDate == -1)
            {
                clonedSession.SessionEndDate = DateTimeOffset.Now.ToUnixTimeSeconds();
            }

            string json = clonedSession.ConvertToJson();

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt";
            if (saveFileDialog.ShowDialog() == true)
            {
                FileManager.Instance.WriteAllText(saveFileDialog.FileName, json);
                SessionManager.Instance.AddFile(saveFileDialog.FileName);
            }
        }

        private void btnLoadSession_Click(object sender, RoutedEventArgs e)
        {
            if (WindowManager.Instance.SessionWin == null)
            {
                WindowManager.Instance.SessionWin = new LoadSessionWindow();
                WindowManager.Instance.SessionWin.Show();
            }
            WindowManager.Instance.SessionWin.Focus();
        }

        private void btnResetSession_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("Are you sure you want to start a new session?", "New Session", MessageBoxButton.YesNo);

            if (res == MessageBoxResult.Yes)
                GameManager.Instance.RefreshSession();
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
    }
}
