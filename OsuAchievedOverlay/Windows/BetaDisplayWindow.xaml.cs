﻿using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for BetaDisplayWindow.xaml
    /// </summary>
    public partial class BetaDisplayWindow : Window
    {
        public BetaDisplayWindow()
        {
            InitializeComponent();

            Closed += (object sender, EventArgs e) =>
            {
                WindowManager.Instance.MainWin.Close();
            };
        }

        public void ApplyUser(OsuApiHelper.OsuUser user)
        {
            LabelUserName.Content = user.Name;

            ImageProfilePicture.Source = InterfaceManager.Instance.LoadImage(@"https://a.ppy.sh/" + user.ID);
            ImageCountryFlag.Source = InterfaceManager.Instance.LoadImage(@"https://osu.ppy.sh/images/flags/" + user.CountryCode + ".png");

            RegionInfo countryInfo = new RegionInfo(user.CountryCode);
            LabelCountryName.Content = countryInfo.DisplayName;
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
            SetLabelStat(LabelGainedRank, session.DifferenceData.WorldRank);

            SetLabelStat(LabelTotalCountryRank, session.CurrentData.CountryRank, false, false);
            SetLabelStat(LabelGainedCountryRank, session.DifferenceData.CountryRank);

            SetLabelStat(LabelTotalPlaycount, session.CurrentData.Playcount, false, false);
            SetLabelStat(LabelGainedPlaycount, session.DifferenceData.Playcount);

            SetLabelStat(LabelTotalAccuracy, session.CurrentData.Accuracy, false, false);
            SetLabelStat(LabelGainedAccuracy, session.DifferenceData.Accuracy);

            SetLabelStat(LabelTotalPerformance, session.CurrentData.Performance, false, false);
            SetLabelStat(LabelGainedPerformance, session.DifferenceData.Performance);
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
    }
}
