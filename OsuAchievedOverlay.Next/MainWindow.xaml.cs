using CefSharp;
using CefSharp.WinForms;
using OsuAchievedOverlay.Next.Managers;
using System;
using System.Threading;
using System.Windows;

namespace OsuAchievedOverlay.Next
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ChromiumWebBrowser chromiumBrowser;

        public MainWindow()
        {
            InitializeComponent();
            InitializeChromium();

            CefSharpSettings.WcfEnabled = true;
            chromiumBrowser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
            chromiumBrowser.JavascriptObjectRepository.Register("cefOsuApp", new cefOsuApp(chromiumBrowser, this), false);
            //chromiumBrowser.RegisterJsObject("cefOsuApp", new cefOsuApp(chromiumBrowser, this));
            Closed += MainWindow_Closed;

            //Forced sleep so browser has time to load
            Thread.Sleep(5);

            BrowserViewModel.Instance.AttachedBrowser = chromiumBrowser;
            BrowserViewModel.Instance.AttachedJavascriptWrapper = cefOsuApp.JsExecuter;

            AppManager.Instance.Start();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            AppManager.Instance.Stop();
            Cef.Shutdown();
        }

        public void InitializeChromium()
        {
            CefSettings settings = new CefSettings();

            string start = string.Format(@"{0}\wwwroot\launcher.html", FileManager.GetExecutableDirectory());

            Cef.Initialize(settings);
            chromiumBrowser = new ChromiumWebBrowser(start);

            chromiumBrowser.MenuHandler = new cefOsuContextMenuHandler();

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
