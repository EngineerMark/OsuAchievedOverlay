using Humanizer;
using OsuAchievedOverlay.Managers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OsuAchievedOverlay.Controls
{
    /// <summary>
    /// Interaction logic for DisplaySession.xaml
    /// </summary>
    public partial class DisplaySession : UserControl
    {
        public DisplaySession()
        {
            InitializeComponent();
        }

        public void ApplyUser(OsuApiHelper.OsuUser user)
        {
            if (!LabelUserName.Content.Equals(user.Name))
            {
                LabelUserName.Content = user.Name;

                string t = ApiHelper.GetOsuUserHeaderUrl(@"https://osu.ppy.sh/users/" + user.ID);

                ThreadPool.QueueUserWorkItem((Object stateInfo) =>
                {
                    BitmapImage img = InterfaceManager.Instance.LoadImage(@"https://a.ppy.sh/" + user.ID);
                    Dispatcher.Invoke(new Action(() =>
                    {
                        ImageProfilePicture.ImageSource = img;
                    }));
                });

                ThreadPool.QueueUserWorkItem((Object stateInfo) =>
                {
                    BitmapImage img = InterfaceManager.Instance.LoadImage(ApiHelper.GetOsuUserHeaderUrl(@"https://osu.ppy.sh/users/" + user.ID));
                    Dispatcher.Invoke(new Action(() =>
                    {
                        ImageProfileHeader.ImageSource = img;
                    }));
                });

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
            Dispatcher.Invoke(new Action(() =>
            {
                if (!session.ReadOnly)
                {
                    if (WindowManager.Instance.DisplayWin.ButtonWarning.Visibility != Visibility.Hidden)
                        WindowManager.Instance.DisplayWin.ButtonWarning.Visibility = Visibility.Hidden;

                    if (WindowManager.Instance.DisplayWin.DisplaySession.GridNonReadonly.Visibility != Visibility.Visible)
                        WindowManager.Instance.DisplayWin.DisplaySession.GridNonReadonly.Visibility = Visibility.Visible;
                    if (WindowManager.Instance.DisplayWin.DisplaySession.GridReadonly.Visibility != Visibility.Hidden)
                        WindowManager.Instance.DisplayWin.DisplaySession.GridReadonly.Visibility = Visibility.Hidden;
                }
                else
                {
                    if (WindowManager.Instance.DisplayWin.ButtonWarning.Visibility != Visibility.Visible)
                        WindowManager.Instance.DisplayWin.ButtonWarning.Visibility = Visibility.Visible;

                    if (WindowManager.Instance.DisplayWin.DisplaySession.GridNonReadonly.Visibility != Visibility.Hidden)
                        WindowManager.Instance.DisplayWin.DisplaySession.GridNonReadonly.Visibility = Visibility.Hidden;
                    if (WindowManager.Instance.DisplayWin.DisplaySession.GridReadonly.Visibility != Visibility.Visible)
                        WindowManager.Instance.DisplayWin.DisplaySession.GridReadonly.Visibility = Visibility.Visible;

                    DateTime sessionStart = DateTimeOffset.FromUnixTimeSeconds(session.SessionDate).UtcDateTime.ToLocalTime();
                    DateTime sessionEnd = DateTimeOffset.FromUnixTimeSeconds(session.SessionEndDate).UtcDateTime.ToLocalTime();

                    WindowManager.Instance.DisplayWin.DisplaySession.LabelReadonlySessionDate.Content = sessionStart.ToString("g") + " - " + sessionEnd.ToString("g");
                }

                WindowManager.Instance.DisplayWin.DisplaySession.ApplyUser(GameManager.Instance.OsuUser);

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
                LabelTotalPlaytime.Content = totalPlayTime.Humanize(1, new System.Globalization.CultureInfo("en-US"), Humanizer.Localisation.TimeUnit.Hour);

                LabelGainedPlaytime.Content = (session.DifferenceData.Playtime >= 0 ? "+" : "") + gainedPlayTime.Humanize(1, new System.Globalization.CultureInfo("en-US"), Humanizer.Localisation.TimeUnit.Hour);
                LabelGainedPlaytime.Foreground = session.DifferenceData.Playtime == 0 ? Brushes.Gray : session.DifferenceData.Playtime >= 0 ? Brushes.LightGreen : Brushes.Pink;
            }));
        }

        private void SetLabelStat(Label label, double value, bool usePrefix = true, bool recolor = true)
        {
            label.Content = (usePrefix && (value >= 0) ? "+" : "") + value.ToString("#,##0.###");
            if (recolor)
                label.Foreground = value == 0 ? Brushes.Gray : value >= 0 ? Brushes.LightGreen : Brushes.Pink;
        }
    }
}
