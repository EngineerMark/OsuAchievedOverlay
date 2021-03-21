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
using System.Windows.Shapes;

namespace OsuAchievedOverlay
{
    /// <summary>
    /// Interaction logic for Display.xaml
    /// </summary>
    public partial class Display : Window
    {
        public Display()
        {
            InitializeComponent();
            ShowInTaskbar = false;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // Begin dragging the window
            this.DragMove();
        }

        public void SetCurrentSS(int value) => LabelSSCurrent.Content = value.ToString("#,##0.###");
        public void SetCurrentS(int value) => LabelSCurrent.Content = value.ToString("#,##0.###");
        public void SetCurrentA(int value) => LabelACurrent.Content = value.ToString("#,##0.###");

        public void SetNewSS(int value) => LabelSSNew.Content = (value >= 0 ? "+" : "-") + Math.Abs(value).ToString("#,##0.###");
        public void SetNewS(int value) => LabelSNew.Content = (value >= 0 ? "+" : "-") + Math.Abs(value).ToString("#,##0.###");
        public void SetNewA(int value) => LabelANew.Content = (value >= 0 ? "+" : "-") + Math.Abs(value).ToString("#,##0.###");

        public void SetCurrentScore(long value) => LabelScoreCurrent.Content = "Score: " + Math.Abs(value).ToString("#,##0.###");
        public void SetNewScore(long value) => LabelScoreNew.Content = (value >= 0 ? "+" : "-") + Math.Abs(value).ToString("#,##0.###");
        public void SetCurrentPlaycount(int value) => LabelPlaycountCurrent.Content = "Playcount: " + Math.Abs(value).ToString("#,##0.###");
        public void SetNewPlaycount(int value) => LabelPlaycountNew.Content = (value >= 0 ? "+" : "-") + Math.Abs(value).ToString("#,##0.###");
    }
}
