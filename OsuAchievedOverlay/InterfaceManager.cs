using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace OsuAchievedOverlay
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
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(url, UriKind.Absolute);
            bitmap.EndInit();
            return bitmap;
        }

        private object FindItem(string name){
            //return WindowManager.Instance.DisplayWin.FindName(name);
            return WindowManager.Instance.BetaDisplayWin.FindName(name);
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
                return (null);
            string s = XamlWriter.Save(orig);
            StringReader stringReader = new StringReader(s);
            XmlReader xmlReader = XmlTextReader.Create(stringReader, new XmlReaderSettings());
            return (UIElement)XamlReader.Load(xmlReader);
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
