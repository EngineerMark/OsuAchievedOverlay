using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Xml;

namespace OsuAchievedOverlay.Managers
{
    public class InterfaceManager : Manager<InterfaceManager>
    {
        public readonly string[] labelList = new string[]{
            "LabelBanchoDown", "LabelPreparing",
            "LabelSSCurrent", "LabelSSNew", 
            "LabelSCurrent", "LabelSNew",
            "LabelACurrent", "LabelANew",
            "LabelScoreCurrent", "LabelScoreNew",
            "LabelPlaycountCurrent", "LabelPlaycountNew",
            "LabelTimeAgoStarted",
            "LabelUsername",
            "LabelAveragePlayScore"
        };

        public override void Start()
        {
            //Not implemented
        }

        public override void Stop()
        {
            //Not implemented
        }

        public BitmapImage LoadImage(string url){
            var webClient = new WebClient();
            byte[] imageBytes = webClient.DownloadData(url);
            MemoryStream memStream = new MemoryStream(imageBytes);
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = memStream;
            //bitmap.UriSource = new Uri(url, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private object FindItem(string name){
            return WindowManager.Instance.DisplayWin.FindName(name);
        }

        private Label FindLabel(string name){
            return (Label)FindItem(name);
        }

        public void SetLabelColor(Color c){
            Brush b = new SolidColorBrush(c);

            for (int i = 0; i < labelList.Length; i++)
                FindLabel(labelList[i]).Foreground = b;
        }

        public void SetLabelFont(FontFamily font){
            for (int i = 0; i < labelList.Length; i++)
                FindLabel(labelList[i]).FontFamily = font;
        }

        public UIElement CloneElement(UIElement orig)
        {
            if (orig == null)
                return null;
            string s = XamlWriter.Save(orig);
            StringReader stringReader = new StringReader(s);
            XmlReader xmlReader = XmlTextReader.Create(stringReader, new XmlReaderSettings());
            return (UIElement)XamlReader.Load(xmlReader);
        }

        public Storyboard AnimateOpacity(UIElement element, double from, double to, double t, Action callbackCompleted = null){
            element.Opacity = from;
            element.Visibility = Visibility.Visible;
        
            DoubleAnimation da = new DoubleAnimation()
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(t),
                EasingFunction = new QuadraticEase()
            };

            Storyboard sb = new Storyboard();
            sb.Children.Add(da);

            Storyboard.SetTarget(sb, element);
            Storyboard.SetTargetProperty(sb, new PropertyPath(Control.OpacityProperty));

            if (callbackCompleted != null)
                sb.Completed += (object sender, EventArgs e) => callbackCompleted();

            sb.Begin();

            return sb;
        }

        public static T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        return (T)child;
                    }

                    T childItem = FindVisualChild<T>(child);
                    if (childItem != null) return childItem;
                }
            }
            return null;
        }
    }
}
