using CefSharp;
using CefSharp.Wpf;
using OsuAchievedOverlay.Next.Managers;
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

            chromiumBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#viewLoader').hide()");
            chromiumBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#viewApp').show()");

            Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Cef.Shutdown();
        }

        public void InitializeChromium()
        {
            CefSettings settings = new CefSettings();
            //settings.CefCommandLineArgs.Add("disable-gpu", "");

            string start = string.Format(@"{0}\wwwroot\index.html", FileManager.GetExecutableDirectory());

            Cef.Initialize(settings);
            chromiumBrowser = new ChromiumWebBrowser(start);

            chromiumBrowser.MenuHandler = new cefOsuContextMenuHandler();

            BrowserWrapper.Children.Add(chromiumBrowser);

            BrowserSettings browserSettings = new BrowserSettings();
            browserSettings.FileAccessFromFileUrls = CefState.Enabled;
            browserSettings.UniversalAccessFromFileUrls = CefState.Enabled;
            chromiumBrowser.BrowserSettings = browserSettings;
        }
    }

    public class cefOsuApp
    {
        private static ChromiumWebBrowser _internalBrowser;
        private static Window _internalWindow;

        public cefOsuApp(ChromiumWebBrowser browser, Window window)
        {
            _internalBrowser = browser;
            _internalWindow = window;
        }

        public void showDevTools()
        {
            _internalBrowser.ShowDevTools();
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
