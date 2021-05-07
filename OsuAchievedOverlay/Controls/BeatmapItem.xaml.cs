using osu_database_reader.Components.Beatmaps;
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

        public BeatmapItem()
        {
            InitializeComponent();

            SoundPlayer = new MediaPlayer();

            Loaded += BeatmapItem_Loaded;
        }

        public void PlayMusic()
        {
            string song = System.IO.Path.Combine(SettingsManager.Instance.Settings["misc"]["osuFolder"],"Songs",AttachedBeatmap.BeatmapFolder,AttachedBeatmap.AudioFileName);

            if(File.Exists(song)){
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
