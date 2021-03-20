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

        public void SetCurrentSS(string value) => LabelSSCurrent.Content = value;
        public void SetCurrentS(string value) => LabelSCurrent.Content = value;
        public void SetCurrentA(string value) => LabelACurrent.Content = value;

        public void SetNewSS(string value) => LabelSSNew.Content = "+" + value;
        public void SetNewS(string value) => LabelSNew.Content = "+" + value;
        public void SetNewA(string value) => LabelANew.Content = "+" + value;

        public void SetCurrentScore(string value) => LabelScoreCurrent.Content = "Score: " + value;
        public void SetNewScore(string value) => LabelScoreNew.Content = "+" + value;
        public void SetCurrentPlaycount(string value) => LabelPlaycountCurrent.Content = "Playcount: " + value;
        public void SetNewPlaycount(string value) => LabelPlaycountNew.Content = "+" + value;
    }
}
