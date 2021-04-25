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
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace OsuAchievedOverlay
{
    /// <summary>
    /// Interaction logic for BetaDisplayWindow.xaml
    /// </summary>
    public partial class BetaDisplayWindow : Window
    {
        public DispatcherTimer Timer { get; set; }
        public KeyValuePair<long, Session> UpdateSession { get; set; }
        public long LastSessionUpdate { get; private set; }

        #region Stuff for profile picture
        public KeyValuePair<long, BitmapImage> UpdateProfileImage { get; set; }
        public long LastProfileImageUpdate { get; private set; }
        private Thread UpdateProfileImageThread = null;
        #endregion

        #region Stuff for profile header
        public KeyValuePair<long, BitmapImage> UpdateHeaderImage { get; set; }
        public long LastHeaderImageUpdate { get; private set; }
        private Thread UpdateHeaderImageThread = null;
        #endregion

        public BetaDisplayWindow()
        {
            InitializeComponent();

            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromSeconds(5);
            Timer.Tick += (object s, EventArgs e) => Update();
            Timer.Start();

            Closed += (object sender, EventArgs e) =>
            {
                Timer?.Stop();

                GameManager.Instance.Stop();
            };
        }

        private void Update()
        {
            if (LastSessionUpdate != UpdateSession.Key)
            {
                ApplySession(UpdateSession.Value);
                LastSessionUpdate = UpdateSession.Key;
            }

            if (LastProfileImageUpdate != UpdateProfileImage.Key)
            {
                ImageProfilePicture.ImageSource = UpdateProfileImage.Value;
                LastProfileImageUpdate = UpdateProfileImage.Key;
            }

            if (LastHeaderImageUpdate != UpdateHeaderImage.Key)
            {
                ImageProfileHeader.ImageSource = UpdateHeaderImage.Value;
                LastHeaderImageUpdate = UpdateHeaderImage.Key;
            }
        }

        public void ApplyUser(OsuApiHelper.OsuUser user)
        {
            if (!LabelUserName.Content.Equals(user.Name))
            {
                LabelUserName.Content = user.Name;

                string t = ApiHelper.GetOsuUserHeaderUrl(@"https://osu.ppy.sh/users/"+user.ID);

                if (UpdateProfileImageThread!=null && UpdateProfileImageThread.IsAlive)
                    UpdateProfileImageThread?.Join();
                UpdateProfileImageThread = new Thread(() =>
                {
                    BitmapImage img = InterfaceManager.Instance.LoadImage(@"https://a.ppy.sh/" + user.ID);
                    UpdateProfileImage = new KeyValuePair<long, BitmapImage>(DateTimeOffset.Now.ToUnixTimeSeconds(), img);
                });
                UpdateProfileImageThread.Start();

                if (UpdateHeaderImageThread != null && UpdateHeaderImageThread.IsAlive)
                    UpdateHeaderImageThread?.Join();
                UpdateHeaderImageThread = new Thread(() =>
                {
                    BitmapImage img = InterfaceManager.Instance.LoadImage(ApiHelper.GetOsuUserHeaderUrl(@"https://osu.ppy.sh/users/" + user.ID));
                    UpdateHeaderImage = new KeyValuePair<long, BitmapImage>(DateTimeOffset.Now.ToUnixTimeSeconds(), img);
                });
                UpdateHeaderImageThread.Start();

                try
                {
                    ImageCountryFlag.Source = new BitmapImage(new Uri("pack://application:,,,/OsuAchievedOverlay;component/Assets/Images/Flags/" + user.CountryCode + ".png"));
                }
                catch (Exception)
                {
                    //Its fine to go here without any issue, try-catch is pretty much for non-existing flag files
                    ImageCountryFlag.Source = new BitmapImage(new Uri("pack://application:,,,/OsuAchievedOverlay;component/Assets/Images/Flags/__.png"));
                }
                RegionInfo countryInfo = new RegionInfo(user.CountryCode);
                LabelCountryName.Content = countryInfo.DisplayName;
            }
        }

        public void ApplySession(Session session)
        {
            LabelSSHCount.Content = session.CurrentData.RankSilverSS;
            LabelSSCount.Content = session.CurrentData.RankGoldSS;
            LabelSHCount.Content = session.CurrentData.RankSilverS;
            LabelSCount.Content = session.CurrentData.RankGoldS;
            LabelACount.Content = session.CurrentData.RankA;

            SetLabelStat(LabelGainedSSHCount, session.DifferenceData.RankSilverSS);
            SetLabelStat(LabelGainedSSCount, session.DifferenceData.RankGoldSS);
            SetLabelStat(LabelGainedSHCount, session.DifferenceData.RankSilverS);
            SetLabelStat(LabelGainedSCount, session.DifferenceData.RankGoldS);
            SetLabelStat(LabelGainedACount, session.DifferenceData.RankA);

            SetLabelStat(LabelTotalLevel, session.CurrentData.Level, false, false);
            SetLabelStat(LabelGainedLevel, session.DifferenceData.Level);

            SetLabelStat(LabelTotalScore, session.CurrentData.TotalScore, false, false);
            SetLabelStat(LabelGainedScore, session.DifferenceData.TotalScore);

            SetLabelStat(LabelTotalRankedScore, session.CurrentData.RankedScore, false, false);
            SetLabelStat(LabelGainedRankedScore, session.DifferenceData.RankedScore);

            SetLabelStat(LabelTotalRank, session.CurrentData.WorldRank, false, false);
            SetLabelStat(LabelGainedRank, -session.DifferenceData.WorldRank);

            SetLabelStat(LabelTotalCountryRank, session.CurrentData.CountryRank, false, false);
            SetLabelStat(LabelGainedCountryRank, -session.DifferenceData.CountryRank);

            SetLabelStat(LabelTotalPlaycount, session.CurrentData.Playcount, false, false);
            SetLabelStat(LabelGainedPlaycount, session.DifferenceData.Playcount);

            SetLabelStat(LabelTotalAccuracy, session.CurrentData.Accuracy, false, false);
            SetLabelStat(LabelGainedAccuracy, session.DifferenceData.Accuracy);

            SetLabelStat(LabelTotalPerformance, session.CurrentData.Performance, false, false);
            SetLabelStat(LabelGainedPerformance, session.DifferenceData.Performance);

            TimeSpan totalPlayTime = TimeSpan.FromSeconds(session.CurrentData.Playtime);
            TimeSpan gainedPlayTime = TimeSpan.FromSeconds(session.DifferenceData.Playtime);
            LabelTotalPlaytime.Content = totalPlayTime.Humanize(maxUnit: Humanizer.Localisation.TimeUnit.Hour);

            LabelGainedPlaytime.Content = (session.DifferenceData.Playtime >= 0 ? "+" : "") + gainedPlayTime.Humanize(maxUnit: Humanizer.Localisation.TimeUnit.Hour);
            LabelGainedPlaytime.Foreground = session.DifferenceData.Playtime >= 0 ? Brushes.LightGreen : Brushes.Pink;
        }

        private void SetLabelStat(Label label, double value, bool usePrefix = true, bool recolor = true)
        {
            label.Content = (usePrefix && (value >= 0) ? "+" : "") + value.ToString("#,##0.###");
            if (recolor)
                label.Foreground = value >= 0 ? Brushes.LightGreen : Brushes.Pink;
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
            if (WindowManager.Instance.SettingsWin == null)
            {
                WindowManager.Instance.SettingsWin = new SettingsWindow();
                WindowManager.Instance.SettingsWin.Show();
            }
            WindowManager.Instance.SettingsWin.Focus();
        }

        private void btnSaveSession_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("Do you want to save this session as read-only?", "Read-only mode", MessageBoxButton.YesNo);

            Session clonedSession = (Session)GameManager.Instance.CurrentSession.Clone();
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
