using System;
using System.Windows;
using System.Windows.Input;

namespace OsuAchievedOverlay
{
    /// <summary>
    /// Interaction logic for Display.xaml
    /// </summary>
    public partial class DisplayWindow : Window
    {
        public static Vector? displayPosition = null;

        public enum DisplayType{
            Preparing,
            BanchoDown,
            Stats
        }

        public DisplayWindow()
        {
            InitializeComponent();
            ShowInTaskbar = false;

            Closed += (object sender, EventArgs e) =>
            {
                displayPosition = new Vector(Left, Top);
            };

            Activated += (object sender, EventArgs e) =>
            {
                Topmost = GameManager.Instance.Settings["display"]["alwaysOnTop"] == "1";
            };

            Deactivated += (object sender, EventArgs e) =>
            {
                Topmost = GameManager.Instance.Settings["display"]["alwaysOnTop"] == "1";
                if(Topmost)
                    Activate();
            };

            if (displayPosition != null)
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
                Left = ((Vector)displayPosition).X;
                Top = ((Vector)displayPosition).Y;
            }

            ScreenPreparing.Visibility = Visibility.Visible;
            ScreenBanchoDown.Visibility = Visibility.Hidden;
            ScreenRegular.Visibility = Visibility.Hidden;
        }

        public void SetDisplay(DisplayType displayType){
            switch(displayType){
                case DisplayType.Preparing:
                    ScreenPreparing.Visibility = Visibility.Visible;
                    ScreenBanchoDown.Visibility = Visibility.Hidden;
                    ScreenRegular.Visibility = Visibility.Hidden;
                    break;
                case DisplayType.BanchoDown:
                    ScreenPreparing.Visibility = Visibility.Hidden;
                    ScreenBanchoDown.Visibility = Visibility.Visible;
                    ScreenRegular.Visibility = Visibility.Hidden;
                    break;
                case DisplayType.Stats:
                    ScreenPreparing.Visibility = Visibility.Hidden;
                    ScreenBanchoDown.Visibility = Visibility.Hidden;
                    ScreenRegular.Visibility = Visibility.Visible;
                    break;
            }
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
        public void SetAverageScore(long value) => LabelAveragePlayScore.Content = Math.Abs(Math.Round((double)value)).ToString("#,##0.###")+" score/play";
    }
}
