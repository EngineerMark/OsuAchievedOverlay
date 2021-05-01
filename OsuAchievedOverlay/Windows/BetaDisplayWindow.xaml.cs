﻿using Humanizer;
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

        public void ApplyUser(OsuApiHelper.OsuUser user)
        {
            if (!DisplaySession.LabelUserName.Content.Equals(user.Name))
            {
                DisplaySession.LabelUserName.Content = user.Name;

                string t = ApiHelper.GetOsuUserHeaderUrl(@"https://osu.ppy.sh/users/" + user.ID);

                ThreadPool.QueueUserWorkItem((Object stateInfo) =>
                {
                    BitmapImage img = InterfaceManager.Instance.LoadImage(@"https://a.ppy.sh/" + user.ID);
                    Dispatcher.Invoke(new Action(() =>
                    {
                        DisplaySession.ImageProfilePicture.ImageSource = img;
                    }));
                });

                ThreadPool.QueueUserWorkItem((Object stateInfo) =>
                {
                    BitmapImage img = InterfaceManager.Instance.LoadImage(ApiHelper.GetOsuUserHeaderUrl(@"https://osu.ppy.sh/users/" + user.ID));
                    Dispatcher.Invoke(new Action(() =>
                    {
                        DisplaySession.ImageProfileHeader.ImageSource = img;
                    }));
                });

                try
                {
                    DisplaySession.ImageCountryFlag.Source = new BitmapImage(new Uri("pack://application:,,,/OsuAchievedOverlay;component/Assets/Images/Flags/" + user.CountryCode + ".png"));
                }
                catch (Exception)
                {
                    //Its fine to go here without any issue, try-catch is pretty much for non-existing flag files
                    DisplaySession.ImageCountryFlag.Source = new BitmapImage(new Uri("pack://application:,,,/OsuAchievedOverlay;component/Assets/Images/Flags/__.png"));
                }
                RegionInfo countryInfo = new RegionInfo(user.CountryCode);
                DisplaySession.LabelCountryName.Content = countryInfo.DisplayName;
            }
        }

        public void ApplySession(Session session)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (!session.ReadOnly)
                {
                    if (WindowManager.Instance.BetaDisplayWin.ButtonWarning.Visibility != Visibility.Hidden)
                        WindowManager.Instance.BetaDisplayWin.ButtonWarning.Visibility = Visibility.Hidden;

                    if (WindowManager.Instance.BetaDisplayWin.DisplaySession.GridNonReadonly.Visibility != Visibility.Visible)
                        WindowManager.Instance.BetaDisplayWin.DisplaySession.GridNonReadonly.Visibility = Visibility.Visible;
                    if (WindowManager.Instance.BetaDisplayWin.DisplaySession.GridReadonly.Visibility != Visibility.Hidden)
                        WindowManager.Instance.BetaDisplayWin.DisplaySession.GridReadonly.Visibility = Visibility.Hidden;
                }
                else
                {
                    if (WindowManager.Instance.BetaDisplayWin.ButtonWarning.Visibility != Visibility.Visible)
                        WindowManager.Instance.BetaDisplayWin.ButtonWarning.Visibility = Visibility.Visible;

                    if (WindowManager.Instance.BetaDisplayWin.DisplaySession.GridNonReadonly.Visibility != Visibility.Hidden)
                        WindowManager.Instance.BetaDisplayWin.DisplaySession.GridNonReadonly.Visibility = Visibility.Hidden;
                    if (WindowManager.Instance.BetaDisplayWin.DisplaySession.GridReadonly.Visibility != Visibility.Visible)
                        WindowManager.Instance.BetaDisplayWin.DisplaySession.GridReadonly.Visibility = Visibility.Visible;

                    DateTime sessionStart = DateTimeOffset.FromUnixTimeSeconds(session.SessionDate).UtcDateTime.ToLocalTime();
                    DateTime sessionEnd = DateTimeOffset.FromUnixTimeSeconds(session.SessionEndDate).UtcDateTime.ToLocalTime();

                    WindowManager.Instance.BetaDisplayWin.DisplaySession.LabelReadonlySessionDate.Content = sessionStart.ToString("g") + " - " + sessionEnd.ToString("g");
                }

                WindowManager.Instance.BetaDisplayWin.ApplyUser(GameManager.Instance.OsuUser);

                DisplaySession.LabelSSHCount.Content = session.CurrentData.RankSilverSS;
                DisplaySession.LabelSSCount.Content = session.CurrentData.RankGoldSS;
                DisplaySession.LabelSHCount.Content = session.CurrentData.RankSilverS;
                DisplaySession.LabelSCount.Content = session.CurrentData.RankGoldS;
                DisplaySession.LabelACount.Content = session.CurrentData.RankA;

                SetLabelStat(DisplaySession.LabelGainedSSHCount, session.DifferenceData.RankSilverSS);
                SetLabelStat(DisplaySession.LabelGainedSSCount, session.DifferenceData.RankGoldSS);
                SetLabelStat(DisplaySession.LabelGainedSHCount, session.DifferenceData.RankSilverS);
                SetLabelStat(DisplaySession.LabelGainedSCount, session.DifferenceData.RankGoldS);
                SetLabelStat(DisplaySession.LabelGainedACount, session.DifferenceData.RankA);

                SetLabelStat(DisplaySession.LabelTotalLevel, session.CurrentData.Level, false, false);
                SetLabelStat(DisplaySession.LabelGainedLevel, session.DifferenceData.Level);

                SetLabelStat(DisplaySession.LabelTotalScore, session.CurrentData.TotalScore, false, false);
                SetLabelStat(DisplaySession.LabelGainedScore, session.DifferenceData.TotalScore);

                SetLabelStat(DisplaySession.LabelTotalRankedScore, session.CurrentData.RankedScore, false, false);
                SetLabelStat(DisplaySession.LabelGainedRankedScore, session.DifferenceData.RankedScore);

                SetLabelStat(DisplaySession.LabelTotalRank, session.CurrentData.WorldRank, false, false);
                SetLabelStat(DisplaySession.LabelGainedRank, -session.DifferenceData.WorldRank);

                SetLabelStat(DisplaySession.LabelTotalCountryRank, session.CurrentData.CountryRank, false, false);
                SetLabelStat(DisplaySession.LabelGainedCountryRank, -session.DifferenceData.CountryRank);

                SetLabelStat(DisplaySession.LabelTotalPlaycount, session.CurrentData.Playcount, false, false);
                SetLabelStat(DisplaySession.LabelGainedPlaycount, session.DifferenceData.Playcount);

                SetLabelStat(DisplaySession.LabelTotalAccuracy, session.CurrentData.Accuracy, false, false);
                SetLabelStat(DisplaySession.LabelGainedAccuracy, session.DifferenceData.Accuracy);

                SetLabelStat(DisplaySession.LabelTotalPerformance, session.CurrentData.Performance, false, false);
                SetLabelStat(DisplaySession.LabelGainedPerformance, session.DifferenceData.Performance);

                TimeSpan totalPlayTime = TimeSpan.FromSeconds(session.CurrentData.Playtime);
                TimeSpan gainedPlayTime = TimeSpan.FromSeconds(session.DifferenceData.Playtime);
                DisplaySession.LabelTotalPlaytime.Content = totalPlayTime.Humanize(1, new System.Globalization.CultureInfo("en-US"), Humanizer.Localisation.TimeUnit.Hour);

                DisplaySession.LabelGainedPlaytime.Content = (session.DifferenceData.Playtime >= 0 ? "+" : "") + gainedPlayTime.Humanize(1, new System.Globalization.CultureInfo("en-US"), Humanizer.Localisation.TimeUnit.Hour);
                DisplaySession.LabelGainedPlaytime.Foreground = session.DifferenceData.Playtime == 0 ? Brushes.Gray : session.DifferenceData.Playtime >= 0 ? Brushes.LightGreen : Brushes.Pink;
            }));
        }

        private void SetLabelStat(Label label, double value, bool usePrefix = true, bool recolor = true)
        {
            label.Content = (usePrefix && (value >= 0) ? "+" : "") + value.ToString("#,##0.###");
            if (recolor)
                label.Foreground = value == 0 ? Brushes.Gray : value >= 0 ? Brushes.LightGreen : Brushes.Pink;
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
