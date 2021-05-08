using FontAwesome.WPF;
using osu_database_reader.Components.Beatmaps;
using OsuAchievedOverlay.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public partial class BeatmapDisplay : UserControl
    {
        public event EventHandler OnEscape;

        private UIElement PrefabHeaderDifficultyBullet
        {
            get
            {
                return InterfaceManager.Instance.CloneElement(_prefabHeaderDifficultyBullet);
            }
        }
        private UIElement _prefabHeaderDifficultyBullet;
        private BeatmapSetEntry CurrentBeatmapSet { get; set; }

        public BeatmapDisplay()
        {
            InitializeComponent();

            _prefabHeaderDifficultyBullet = InterfaceManager.Instance.CloneElement(HeaderDifficultyBullet);

            Loaded += BeatmapDisplay_Loaded;

        }

        private void BeatmapDisplay_Loaded(object sender, RoutedEventArgs e)
        {
            Managers.InputManager.OnKeyUp += (object sender, KeyEventArgs e) =>
            {
                if (e.Key == Key.Escape && Visibility == Visibility.Visible)
                    OnEscape?.Invoke(this, null);
            };

            HyperlinkBeatmapsetCreator.RequestNavigate += (object sender, RequestNavigateEventArgs e) =>
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                e.Handled = true;
            };
        }

        public void ApplyBeatmapSet(BeatmapSetEntry set)
        {
            if (!string.IsNullOrEmpty(set.BackgroundPath))
            {
                ImageBeatmapBackground.Source = new BitmapImage(new Uri(set.BackgroundPath));
            }
            else
            {
                ImageBeatmapBackground.Source = null;
            }

            //pogchamp

            ListViewDifficulties.Children.Clear();

            // Reordering the difficulties
            ThreadPool.QueueUserWorkItem((Object stateInfo) =>
            {

                List<BeatmapEntry> osuMaps = set.Beatmaps.Where(map => map.GameMode == osu.Shared.GameMode.Standard).ToList();
                List<BeatmapEntry> taikoMaps = set.Beatmaps.Where(map => map.GameMode == osu.Shared.GameMode.Taiko).ToList();
                List<BeatmapEntry> maniaMaps = set.Beatmaps.Where(map => map.GameMode == osu.Shared.GameMode.Mania).ToList();
                List<BeatmapEntry> catchMaps = set.Beatmaps.Where(map => map.GameMode == osu.Shared.GameMode.CatchTheBeat).ToList();

                if (osuMaps.Count > 0)
                    osuMaps.Sort((a, b) => a.DiffStarRatingStandard[0].CompareTo(b.DiffStarRatingStandard[0]));
                if (taikoMaps.Count > 0)
                    taikoMaps.Sort((a, b) => a.DiffStarRatingTaiko[0].CompareTo(b.DiffStarRatingTaiko[0]));
                if (maniaMaps.Count > 0)
                    maniaMaps.Sort((a, b) => a.DiffStarRatingMania[0].CompareTo(b.DiffStarRatingMania[0]));
                if (catchMaps.Count > 0)
                    catchMaps.Sort((a, b) => a.DiffStarRatingCtB[0].CompareTo(b.DiffStarRatingCtB[0]));

                List<BeatmapEntry> refilledList = new List<BeatmapEntry>();
                refilledList.AddRange(osuMaps);
                refilledList.AddRange(taikoMaps);
                refilledList.AddRange(maniaMaps);
                refilledList.AddRange(catchMaps);

                foreach (BeatmapEntry map in refilledList)
                {

                    string modeString = BeatmapHelper.GetGamemodeString(map.GameMode);
                    double sr = BeatmapHelper.GetStarRating(map);

                    BitmapImage gamemodeIcon = BeatmapHelper.GetGamemodeIcon(map.GameMode);

                    Dispatcher.Invoke(() =>
                    {
                        Grid difficultyBullet = (Grid)PrefabHeaderDifficultyBullet;
                        difficultyBullet.ToolTip = "(" + modeString + ", " + Math.Round(sr, 1) + "*) " + map.Version;

                        //Image childIcon = (Image)difficultyBullet.Children[0];
                        //childIcon.Foreground = BeatmapHelper.GetColorFromDifficulty(sr);
                        //childIcon.Source = gamemodeIcon;

                        Rectangle childRect = (Rectangle)difficultyBullet.Children[0];
                        Tuple<double, Brush, Brush> color = BeatmapHelper.GetColorFromDifficulty(sr);
                        childRect.Fill = color.Item2;
                        ((ImageBrush)childRect.OpacityMask).ImageSource = gamemodeIcon;

                        difficultyBullet.MouseEnter += (object sender, MouseEventArgs e) =>
                        {
                            Grid actualSender = (Grid)sender;
                            //InterfaceManager.Instance.AnimateOpacity(actualSender, 1, 0.7, 0.2);
                            childRect.Fill = color.Item3;
                        };
                        difficultyBullet.MouseLeave += (object sender, MouseEventArgs e) =>
                        {
                            Grid actualSender = (Grid)sender;
                            //InterfaceManager.Instance.AnimateOpacity(actualSender, 0.7, 1, 0.2);
                            childRect.Fill = color.Item2;
                        };

                        difficultyBullet.MouseDown += Btn_ClickDifficultyBullet;

                        difficultyBullet.Tag = map.BeatmapId;

                        ListViewDifficulties.Children.Add(difficultyBullet);
                    });
                }
            });

            CurrentBeatmapSet = set;
            ApplyBeatmap(set.Beatmaps[0]);

            LabelBeatmapsetArtist.Content = set.Artist;

            HyperlinkBeatmapsetCreatorText.Text = set.Creator;
            HyperlinkBeatmapsetCreator.NavigateUri = new Uri("https://osu.ppy.sh/users/" + set.Creator);
        }

        public void ApplyBeatmap(BeatmapEntry beatmap)
        {
            double sr = BeatmapHelper.GetStarRating(beatmap);

            LabelBeatmapsetTitle.Content = beatmap.Title + " [" + beatmap.Version + "]";

            LabelBeatmapBpm.Content = Math.Round(beatmap.TimingPoints[0].MsPerQuarter.MsToBpm());
            LabelBeatmapLength.Content = TimeSpan.FromMilliseconds(beatmap.TotalTime).ToString(@"mm\:ss");
            LabelBeatmapObjects.Content = beatmap.CountHitCircles + beatmap.CountSliders + beatmap.CountSpinners;

            LabelBeatmapApproachRate.Content = Math.Round(beatmap.ApproachRate, 1);
            LabelBeatmapCircleSize.Content = Math.Round(beatmap.CircleSize, 1);
            LabelBeatmapDrain.Content = Math.Round(beatmap.HPDrainRate, 1);
            LabelBeatmapOverallDifficulty.Content = Math.Round(beatmap.OveralDifficulty, 1);
            LabelBeatmapStarRating.Content = Math.Round(sr, 1);
        }

        private void Btn_ClickDifficultyBullet(object sender, MouseButtonEventArgs e)
        {
            //Grid actualSender = (Grid)sender;
            int mapID = (int)((Grid)sender).Tag;

            BeatmapEntry clickedMap = CurrentBeatmapSet.Beatmaps.Find(map => map.BeatmapId == mapID);
            if (clickedMap != null)
            {
                ApplyBeatmap(clickedMap);
            }
        }

        private void Btn_HoverEnterDifficultyBullet(object sender, MouseEventArgs e)
        {
            Grid actualSender = (Grid)sender;
            InterfaceManager.Instance.AnimateOpacity(actualSender, 1, 0.7, 0.2);
        }

        private void Btn_HoverLeaveDifficultyBullet(object sender, MouseEventArgs e)
        {
            Grid actualSender = (Grid)sender;
            InterfaceManager.Instance.AnimateOpacity(actualSender, 0.7, 1, 0.2);

        }
    }
}
