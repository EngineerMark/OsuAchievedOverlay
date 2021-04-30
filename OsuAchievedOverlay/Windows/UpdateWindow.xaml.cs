using MarkdownSharp;
using OsuAchievedOverlay.Github;
using OsuAchievedOverlay.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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

namespace OsuAchievedOverlay.Windows
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        public UpdateWindow(bool isUpdating = false)
        {
            InitializeComponent();
            WindowManager.Instance.UpdateWin = this;

            TextBlockProgressStates.Text = string.Empty;

            if (isUpdating)
            {
                GridUpdateReady.Visibility = Visibility.Hidden;
                GridUpdater.Visibility = Visibility.Visible;

                UpdateManager.Instance.PerformUpdates();
            }else{
                GridUpdateReady.Visibility = Visibility.Visible;
                GridUpdater.Visibility = Visibility.Hidden;
            }

            Closed += (object sender, EventArgs e) =>
            {
                Application.Current.Shutdown();
            };
        }

        public void SetReleaseData(List<Release> releases){
            //TextBlockReleaseLog.Text = string.Empty;
            //TextBlockReleaseLog.Text += "Version "+releases[0].Version+"\n";
            //TextBlockReleaseLog.Text += releases[0].Body;
            string val = "<style>" +
            "body { background-color:#3D2932;color:#ffffff;font-family:'Torus', Fallback, sans-serif;}" +
            "img { display: block;max-width:100%;border-radius:10px;}" +
            "hr { color:#F977B1; }" +
            "</style>";

            val += "<body>";
            Markdown md = new Markdown();
            int i = 0;
            foreach(Release rel in releases){
                string transformedBody = md.Transform(rel.Body);
                val += "Version "+rel.Version+"\n";
                val += transformedBody+"\n";
                i++;
                if(i<releases.Count)
                    val += "<hr />";
            }
            val += "</body>";
            BrowserDisplayReleases.NavigateToString(val);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Btn_Close(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Btn_Minimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Btn_Continue(object sender, RoutedEventArgs e)
        {
            Process.Start(System.Reflection.Assembly.GetEntryAssembly().CodeBase, "-osunoupdate");
            Close();
        }

        private void Btn_Update(object sender, RoutedEventArgs e)
        {
            if(Directory.Exists("temp"))
                Directory.Delete("temp", true);

            DirectoryInfo tempDir = Directory.CreateDirectory("temp");
            tempDir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

            List<string> allFiles = FileManager.Instance.GetAllFilesInDirectory(FileManager.Instance.GetApplicationDirectory().FullName);

            foreach(string file in allFiles){
                string relFile = file.Replace(FileManager.Instance.GetApplicationDirectory().FullName+"\\", "");

                string path = System.IO.Path.GetDirectoryName(relFile);

                if (path.Length > 0 && !Directory.Exists("temp/"+path))
                    Directory.CreateDirectory("temp/"+path);

                File.Copy(relFile, "temp/" + relFile, true);
            }

            Process.Start(System.IO.Path.Combine(FileManager.Instance.GetApplicationDirectory().FullName,"temp","OsuAchievedOverlay.exe"), "-osustartupdate");
            Close();
        }
    }
}
