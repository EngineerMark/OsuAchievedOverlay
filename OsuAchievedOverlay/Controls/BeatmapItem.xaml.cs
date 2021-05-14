using osu.Shared;
using osu_database_reader.Components.Beatmaps;
using OsuAchievedOverlay.Helpers;
using OsuAchievedOverlay.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OsuAchievedOverlay.Controls
{
    /// <summary>
    /// Interaction logic for BeatmapItem.xaml
    /// </summary>
    public partial class BeatmapItem : UserControl
    {
        public static readonly DependencyProperty BeatmapTitleProperty = DependencyProperty.Register("BeatmapTitle", typeof(String), typeof(OsuDataEntry), new FrameworkPropertyMetadata(string.Empty));
        public String BeatmapTitle
        {
            get { return (String)GetValue(BeatmapTitleProperty); }
            set { SetValue(BeatmapTitleProperty, value); }
        }

        public static readonly DependencyProperty BeatmapNameProperty = DependencyProperty.Register("BeatmapName", typeof(String), typeof(OsuDataEntry), new FrameworkPropertyMetadata(string.Empty));
        public String BeatmapName
        {
            get { return (String)GetValue(BeatmapNameProperty); }
            set { SetValue(BeatmapNameProperty, value); }
        }

        public static readonly DependencyProperty BeatmapMapperProperty = DependencyProperty.Register("BeatmapMapper", typeof(String), typeof(OsuDataEntry), new FrameworkPropertyMetadata(string.Empty));
        public String BeatmapMapper
        {
            get { return (String)GetValue(BeatmapMapperProperty); }
            set { SetValue(BeatmapMapperProperty, value); }
        }

        public static EventHandler OnBeatmapClick;
        public static BeatmapItem CurrentPlayingSong { get; set; }
        public BeatmapSetEntry AttachedBeatmap { get; set; }

        private MediaPlayer SoundPlayer { get; set; }
        private UIElement PrefabBeatmapDifficultyBullet
        {
            get
            {
                return InterfaceManager.Instance.CloneElement(_prefabBeatmapDifficultyBullet);
            }
        }

        private UIElement _prefabBeatmapDifficultyBullet;



        public BeatmapItem()
        {
            InitializeComponent();

            SoundPlayer = new MediaPlayer();

            Loaded += BeatmapItem_Loaded;
            _prefabBeatmapDifficultyBullet = InterfaceManager.Instance.CloneElement(BeatmapDifficultyBullet);
        }

        public void ApplyBeatmapSet(BeatmapSetEntry set)
        {
            AttachedBeatmap = set;

            LabelTitle.Content = set.Title;
            LabelName.Content = set.Artist;
            LabelMapper.Content = set.Creator;

            switch (set.RankStatus)
            {
                case SubmissionStatus.Ranked:
                    MapStateRanked.Visibility = Visibility.Visible;
                    break;
                case SubmissionStatus.Loved:
                    MapStateLoved.Visibility = Visibility.Visible;
                    break;
                case SubmissionStatus.Unknown:
                case SubmissionStatus.Pending:
                case SubmissionStatus.EditableCutoff:
                    MapStateUnranked.Visibility = Visibility.Visible;
                    break;
                case SubmissionStatus.NotSubmitted:
                    MapStateUnsubmitted.Visibility = Visibility.Visible;
                    break;
                case SubmissionStatus.Qualified:
                    MapStateQualified.Visibility = Visibility.Visible;
                    break;
                case SubmissionStatus.Approved:
                    MapStateApproved.Visibility = Visibility.Visible;
                    break;
            }

            List<BeatmapEntry> osuDiffs = set.Beatmaps.Where(map => map.GameMode == GameMode.Standard).ToList();
            List<BeatmapEntry> taikoDiffs = set.Beatmaps.Where(map => map.GameMode == GameMode.Taiko).ToList();
            List<BeatmapEntry> maniaDiffs = set.Beatmaps.Where(map => map.GameMode == GameMode.Mania).ToList();
            List<BeatmapEntry> catchDiffs = set.Beatmaps.Where(map => map.GameMode == GameMode.CatchTheBeat).ToList();

            osuDiffs.Sort((BeatmapEntry a, BeatmapEntry b) => a.DiffStarRatingStandard[0].CompareTo(b.DiffStarRatingStandard[0]));
            taikoDiffs.Sort((BeatmapEntry a, BeatmapEntry b) => a.DiffStarRatingTaiko[0].CompareTo(b.DiffStarRatingTaiko[0]));
            maniaDiffs.Sort((BeatmapEntry a, BeatmapEntry b) => a.DiffStarRatingMania[0].CompareTo(b.DiffStarRatingMania[0]));
            catchDiffs.Sort((BeatmapEntry a, BeatmapEntry b) => a.DiffStarRatingCtB[0].CompareTo(b.DiffStarRatingCtB[0]));

            List<BeatmapEntry> refilledDiffs = new List<BeatmapEntry>();
            refilledDiffs.AddRange(osuDiffs);
            refilledDiffs.AddRange(taikoDiffs);
            refilledDiffs.AddRange(maniaDiffs);
            refilledDiffs.AddRange(catchDiffs);

            DifficultyStandardBulletList.Children.Clear();
            DifficultyTaikoBulletList.Children.Clear();
            DifficultyManiaBulletList.Children.Clear();
            DifficultyCatchBulletList.Children.Clear();
            //string t = "";
            if (refilledDiffs.Count > 0)
            {
                ((Grid)DifficultyStandardBulletList.Parent).Visibility = Visibility.Visible;
                foreach (BeatmapEntry map in refilledDiffs)
                {
                    double sr = BeatmapHelper.GetStarRating(map);
                    string mode = BeatmapHelper.GetGamemodeString(map.GameMode);

                    Grid bullet = (Grid)PrefabBeatmapDifficultyBullet;
                    bullet.ToolTip = "(" + mode + ", " + Math.Round(sr, 1) + "*) " + map.Version;

                    Border colorBorder = (Border)bullet.Children[0];
                    colorBorder.Background = BeatmapHelper.GetColorFromDifficulty(sr).Item2;

                    DifficultyStandardBulletList.Children.Add(bullet);
                }
            }
        }

        public void PlayMusic()
        {
            string song = System.IO.Path.Combine(SettingsManager.Instance.Settings["misc"]["osuFolder"], "Songs", AttachedBeatmap.BeatmapFolder, AttachedBeatmap.AudioFileName);

            if (File.Exists(song))
            {
                SoundPlayer.Open(new Uri(song));
                SoundPlayer.Play();
                MusicButton.Icon = FontAwesome.WPF.FontAwesomeIcon.Pause;
            }
            else
            {
                MessageBoxResult r = MessageBox.Show("The audio file of this beatmap is missing", "Oops");
            }
        }

        public void StopMusic()
        {
            if (CurrentPlayingSong == this)
                CurrentPlayingSong = null;
            SoundPlayer?.Stop();
            MusicButton.Icon = FontAwesome.WPF.FontAwesomeIcon.Play;
        }

        private void BeatmapItem_Loaded(object sender, RoutedEventArgs e)
        {
            MusicButton.MouseDown += (object sender, MouseButtonEventArgs e) =>
            {
                bool selfMute = CurrentPlayingSong == this;
                if (CurrentPlayingSong != null)
                    CurrentPlayingSong.StopMusic();

                if (selfMute)
                    return;

                CurrentPlayingSong = this;
                PlayMusic();
            };

            Opacity = 0;
            InterfaceManager.Instance.AnimateOpacity(this, 0, 1, 0.5f);
        }

        private void BeatmapMouseEnter(object sender, MouseEventArgs e)
        {
            InterfaceManager.Instance.AnimateOpacity(BeatmapHoverOverlayResult, 0, 0.4f, 0.3f);
        }

        private void BeatmapMouseLeave(object sender, MouseEventArgs e)
        {
            InterfaceManager.Instance.AnimateOpacity(BeatmapHoverOverlayResult, 0.4f, 0, 0.3f);
        }

        private void BeatmapMouseClick(object sender, MouseButtonEventArgs e)
        {
            OnBeatmapClick?.Invoke(this, null);
        }

        private void MusicMouseEnter(object sender, MouseEventArgs e)
        {
            InterfaceManager.Instance.AnimateOpacity(MusicHoverOverlayResult, 0f, 0.4f, 0.2f);
        }

        private void MusicMouseLeave(object sender, MouseEventArgs e)
        {
            InterfaceManager.Instance.AnimateOpacity(MusicHoverOverlayResult, 0.4f, 0, 0.2f);
        }

        private void MusicButtonMouseEnter(object sender, MouseEventArgs e)
        {
            MusicButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD966"));
        }

        private void MusicButtonMouseLeave(object sender, MouseEventArgs e)
        {
            MusicButton.Foreground = Brushes.White;
        }
    }
}
