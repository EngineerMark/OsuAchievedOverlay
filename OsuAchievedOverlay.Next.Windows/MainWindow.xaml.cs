using CefSharp;
using CefSharp.WinForms;
using OsuAchievedOverlay.Next.Managers;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace OsuAchievedOverlay.Next
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ChromiumWebBrowser chromiumBrowser;

        public MainWindow(StartupEventArgs e)
        {
            InitializeComponent();

            string webpackage = "webdata.pak";
            string webfolder = "wwwroot";

            if(!File.Exists(webpackage)){
                MessageBox.Show("File data for the display is missing! Did you delete it? (webdata.pak)");
                Environment.Exit(-1);
            }

            if(Directory.Exists(webfolder))
                Directory.Delete(webfolder, true);

            DirectoryInfo dirInfo = Directory.CreateDirectory(webfolder);
            dirInfo.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

            ZipFile.ExtractToDirectory(webpackage, webfolder);

            InitializeChromium();

            CefSharpSettings.WcfEnabled = true;
            chromiumBrowser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
            chromiumBrowser.JavascriptObjectRepository.Register("cefOsuApp", new cefOsuApp(chromiumBrowser, this), false);
            //chromiumBrowser.RegisterJsObject("cefOsuApp", new cefOsuApp(chromiumBrowser, this));
            SourceInitialized += MainWindow_SourceInitialized;
            Closed += MainWindow_Closed;

            //Forced sleep so browser has time to load
            Thread.Sleep(5);

            BrowserViewModel.Instance.AttachedBrowser = chromiumBrowser;
            BrowserViewModel.Instance.AttachedJavascriptWrapper = cefOsuApp.JsExecuter;

            AppManager.Instance.Start();
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.Top != -1)
            {
                // Very quick and dirty - but it does the job
                if (Properties.Settings.Default.Maximized)
                {
                    WindowState = WindowState.Maximized;
                }else{
                    this.Top = Properties.Settings.Default.Top;
                    this.Left = Properties.Settings.Default.Left;
                    this.Height = Properties.Settings.Default.Height;
                    this.Width = Properties.Settings.Default.Width;
                }
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                Properties.Settings.Default.Top = RestoreBounds.Top;
                Properties.Settings.Default.Left = RestoreBounds.Left;
                Properties.Settings.Default.Height = RestoreBounds.Height;
                Properties.Settings.Default.Width = RestoreBounds.Width;
                Properties.Settings.Default.Maximized = true;
            }
            else
            {
                Properties.Settings.Default.Top = this.Top;
                Properties.Settings.Default.Left = this.Left;
                Properties.Settings.Default.Height = this.Height;
                Properties.Settings.Default.Width = this.Width;
                Properties.Settings.Default.Maximized = false;
            }

            Properties.Settings.Default.Save();

            AppManager.Instance.Stop();
            Cef.Shutdown();
        }

        public void InitializeChromium()
        {
            CefSettings settings = new CefSettings();

            string start = string.Format(@"{0}\wwwroot\launcher.html", FileManager.GetExecutableDirectory());

            Cef.Initialize(settings);
            chromiumBrowser = new ChromiumWebBrowser(start);

            //chromiumBrowser.MenuHandler = new cefOsuContextMenuHandler();

            BrowserWrapper.Child = chromiumBrowser;

            BrowserSettings browserSettings = new BrowserSettings();
            browserSettings.FileAccessFromFileUrls = CefState.Enabled;
            browserSettings.UniversalAccessFromFileUrls = CefState.Enabled;
            chromiumBrowser.BrowserSettings = browserSettings;
        }
    }

    public class cefOsuContextMenuHandler : IContextMenuHandler
    {
        public void OnBeforeContextMenu(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
        {
            model.Clear();
        }

        public bool OnContextMenuCommand(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
        {
            return false;
        }

        public void OnContextMenuDismissed(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame)
        {
        }

        public bool RunContextMenu(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
        {
            return false;
        }
    }
}
