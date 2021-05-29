using CefSharp;
using CefSharp.WinForms;
using Microsoft.WindowsAPICodePack.Dialogs;
using OsuAchievedOverlay.Next.JavaScript;
using OsuAchievedOverlay.Next.Managers;
using OsuAchievedOverlay.Next.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using OsuApiHelper;
using IniParser.Model;

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

    public class cefOsuApp
    {
        private static ChromiumWebBrowser _internalBrowser;
        private static Window _internalWindow;
        private static JSWrapper _jsExecuter;

        public static event EventHandler SetupFinished;

        public static JSWrapper JsExecuter { get => _jsExecuter; set => _jsExecuter = value; }

        public cefOsuApp(ChromiumWebBrowser browser, Window window)
        {
            _internalBrowser = browser;
            _internalWindow = window;

            JsExecuter = new JSWrapper(_internalBrowser);
        }

        public static Window GetWindow() => _internalWindow;

        public void showDevTools()
        {
            _internalBrowser.ShowDevTools();
        }

        public void OpenUrl(string url)
        {
            System.Diagnostics.Process.Start(url);
        }

        public void dirDialogOsuInstall()
        {
            _internalWindow.Dispatcher.Invoke(() =>
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;
                CommonFileDialogResult result = dialog.ShowDialog();
                JsExecuter.SetAttribute("#settingsInputOsuDir", "value", HttpUtility.JavaScriptStringEncode(dialog.FileName));
                JsExecuter.AddClass("#settingsInputOsuDirLabel", "active");
            });
        }

        public void buttonSaveSettings()
        {
            _internalWindow.Dispatcher.Invoke(() =>
            {
                Task task = Task.Run(async () =>
                {
                    JsExecuter.SetElementDisabled("#settingsConfirmButton", true);
                    JsExecuter.SetHtml("#settingsConfirmButton", "<span class=\"spinner-border spinner-border-sm\" role=\"status\" aria-hidden=\"true\"></span> saving");

                    string apiKey = await BrowserViewModel.Instance.SettingsGetApiKey();
                    string username = await BrowserViewModel.Instance.SettingsGetUsername();
                    string osudir = await BrowserViewModel.Instance.SettingsGetOsuDirectory();
                    OsuMode gamemode = await BrowserViewModel.Instance.SettingsGetGamemode();
                    int updateRateInteger = -1;
                    int roundingDigit = -1;


                    bool processSettings = true;
                    if (!ApiHelper.IsUserValid(apiKey, username))
                    {
                        BrowserViewModel.Instance.SendNotification(NotificationType.Danger, "API Key or username is invalid");
                        processSettings = false;
                    }

                    if(!string.IsNullOrEmpty(osudir) && processSettings){
                        if(!ApiHelper.IsValidOsuInstallation(osudir)){
                            BrowserViewModel.Instance.SendNotification(NotificationType.Danger, "Selected osu directory is invalid");
                            processSettings = false;
                        }
                    }

                    if(processSettings){
                        string updateRate = await BrowserViewModel.Instance.SettingsGetUpdaterate();
                        try
                        {
                            updateRateInteger = Convert.ToInt32(updateRate);
                            updateRateInteger = Math.Min(SettingsManager.RefreshTimeMax, Math.Max(SettingsManager.RefreshTimeMin, updateRateInteger));
                        }catch(Exception e){
                            BrowserViewModel.Instance.SendNotification(NotificationType.Danger, "Update rate value seems to be invalid");
                            processSettings = false;
                        }
                    }

                    if (processSettings)
                    {
                        string roundingDigitVal = await BrowserViewModel.Instance.SettingsGetRoundingValue();
                        try
                        {
                            roundingDigit = Convert.ToInt32(roundingDigitVal);
                            roundingDigit = Math.Min(SettingsManager.RoundingMax, Math.Max(SettingsManager.RoundingMin, roundingDigit));
                        }
                        catch (Exception e)
                        {
                            BrowserViewModel.Instance.SendNotification(NotificationType.Danger, "Rounding value seems to be invalid");
                            processSettings = false;
                        }
                    }

                    if (processSettings){

                        KeyDataCollection displayOptions = SettingsManager.DefaultSettings["showingItems"];
                        foreach (KeyData keyData in displayOptions)
                        {
                            string key = keyData.KeyName;
                            bool state = await BrowserViewModel.Instance.AttachedJavascriptWrapper.Checkbox.IsChecked("#settingsInputDisplay"+(key.FirstCharToUpper())+ "");
                            SettingsManager.Instance.Settings["showingItems"][key] = state ? "true" : "false";
                        }
                    }

                    if (processSettings)
                    {
                        SettingsManager.Instance.Settings["api"]["key"] = apiKey;
                        SettingsManager.Instance.Settings["api"]["user"] = username;
                        SettingsManager.Instance.Settings["api"]["updateRate"] = updateRateInteger+"";
                        SettingsManager.Instance.Settings["api"]["gamemode"] = gamemode + "";
                        SettingsManager.Instance.Settings["display"]["roundingValue"] = roundingDigit + "";
                        SettingsManager.Instance.Settings["misc"]["osuFolder"] = osudir;

                        // Save stuff
                        SettingsManager.Instance.SettingsSave();
                        SettingsManager.Instance.SettingsApply();
                        BrowserViewModel.Instance.SendNotification(NotificationType.Success, "Saved settings");
                    }

                    //MessageBox.Show(apiKey);
                    JsExecuter.SetElementDisabled("#settingsConfirmButton", false);
                    JsExecuter.SetHtml("#settingsConfirmButton", "Save and apply");

                });
            });
        }
    
        public void finishSetup(){
            SetupFinished?.Invoke(null, null);
        }

        public void buttonProcessOsu(){
            InspectorManager.Instance.ProcessOsu(_internalWindow.Dispatcher);
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
