using osu_database_reader.Components.Beatmaps;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OsuAchievedOverlay.Controls
{
    public partial class BeatmapDisplay : UserControl
    {
        public event EventHandler OnEscape;

        public BeatmapDisplay()
        {
            InitializeComponent();

            Loaded += BeatmapDisplay_Loaded;

        }

        private void BeatmapDisplay_Loaded(object sender, RoutedEventArgs e)
        {
            Managers.InputManager.OnKeyUp += (object sender, KeyEventArgs e) =>
            {
                if (e.Key == Key.Escape && Visibility==Visibility.Visible)
                    OnEscape?.Invoke(this, null);
            };
        }

        public void ApplyBeatmapSet(BeatmapSetEntry map){
            if(!string.IsNullOrEmpty(map.BackgroundPath)){
                ImageBeatmapBackground.Source = new BitmapImage(new Uri(map.BackgroundPath));
            }else{
                ImageBeatmapBackground.Source = null;
            }

            LabelBeatmapsetTitle.Content = map.Title;
            LabelBeatmapsetArtist.Content = map.Artist;
        }
    }
}
