using osu_database_reader.Components.Beatmaps;
using OsuAchievedOverlay.Managers;
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

        public BeatmapSetEntry AttachedBeatmap { get; set; }

        public BeatmapItem()
        {
            InitializeComponent();

            Loaded += BeatmapItem_Loaded;
        }

        private void BeatmapItem_Loaded(object sender, RoutedEventArgs e)
        {
            Opacity = 0;
            InterfaceManager.Instance.AnimateOpacity(this, 0, 1, 0.5f);
        }

        private void BeatmapMouseEnter(object sender, MouseEventArgs e)
        {
            InterfaceManager.Instance.AnimateOpacity(HoverOverlayResult, 0, 0.4f, 0.3f);
        }

        private void BeatmapMouseLeave(object sender, MouseEventArgs e)
        {
            InterfaceManager.Instance.AnimateOpacity(HoverOverlayResult, 0.4f, 0, 0.3f);
        }

        private void BeatmapMouseClick(object sender, MouseButtonEventArgs e)
        {
            OnBeatmapClick?.Invoke(this, null);
        }
    }
}
