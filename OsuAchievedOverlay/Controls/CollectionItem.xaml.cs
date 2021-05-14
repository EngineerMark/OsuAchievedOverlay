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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OsuAchievedOverlay.Controls
{
    /// <summary>
    /// Interaction logic for CollectionItem.xaml
    /// </summary>
    public partial class CollectionItem : UserControl
    {
        public static readonly DependencyProperty CollectionTitleProperty = DependencyProperty.Register("CollectionTitle", typeof(String), typeof(CollectionItem), new FrameworkPropertyMetadata(string.Empty));
        public String CollectionTitle
        {
            get { return (String)GetValue(CollectionTitleProperty); }
            set { SetValue(CollectionTitleProperty, value); }
        }

        public static readonly DependencyProperty CollectionSizeProperty = DependencyProperty.Register("CollectionSize", typeof(String), typeof(CollectionItem), new FrameworkPropertyMetadata(string.Empty));
        public String CollectionSize
        {
            get { return (String)GetValue(CollectionSizeProperty); }
            set { SetValue(CollectionSizeProperty, value); }
        }

        public CollectionItem()
        {
            InitializeComponent();

            Loaded += CollectionItem_Loaded;
        }

        private void CollectionItem_Loaded(object sender, RoutedEventArgs e)
        {
            HoverOverlay.MouseEnter += BackBorder_MouseEnter;
            HoverOverlay.MouseLeave += BackBorder_MouseLeave;
        }

        private void BackBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            InterfaceManager.Instance.AnimateOpacity(BackBorder, 1, 0.5, 0.3);
        }

        private void BackBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            InterfaceManager.Instance.AnimateOpacity(BackBorder, 0.5, 1, 0.3);
        }

    }
}
