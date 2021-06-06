using CefSharp;
using CefSharp.WinForms;
using IniParser.Model;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using OsuAchievedOverlay.Next.Helpers;
using OsuAchievedOverlay.Next.JavaScript;
using OsuAchievedOverlay.Next.Managers;
using OsuApiHelper;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace OsuAchievedOverlay.Next
{
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

        public void sessionHandlerStartNew()
        {
            SessionManager.Instance.PrepareSession();
            BrowserViewModel.Instance.AttachedJavascriptWrapper.Modal.Hide("#modalNewSession");
            BrowserViewModel.Instance.SendNotification(NotificationType.Info, "Started a new session");
        }

        public void sessionHandlerSave(bool _readonly = false)
        {
            if(SessionManager.Instance!=null && SessionManager.Instance.CurrentSession!=null){
                Session clonedSession = (Session)SessionManager.Instance.CurrentSession.Clone();
                clonedSession.ReadOnly = _readonly;
                string json = clonedSession.ConvertToJson();
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt";
                if (saveFileDialog.ShowDialog() == true)
                {
                    FileManager.WriteAllText(saveFileDialog.FileName, json);
                    BrowserViewModel.Instance.AttachedJavascriptWrapper.Modal.Hide("#modalSaveSession");
                    BrowserViewModel.Instance.SendNotification(NotificationType.Success, "Saved session");
                    //SessionManager.Instance.AddFile(saveFileDialog.FileName);
                }
            }
            else{
                BrowserViewModel.Instance.SendNotification(NotificationType.Danger, "Something went wrong while saving, please retry");
            }
        }

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

                    if (!string.IsNullOrEmpty(osudir) && processSettings)
                    {
                        if (!ApiHelper.IsValidOsuInstallation(osudir))
                        {
                            BrowserViewModel.Instance.SendNotification(NotificationType.Danger, "Selected osu directory is invalid");
                            processSettings = false;
                        }
                    }

                    if (processSettings)
                    {
                        string updateRate = await BrowserViewModel.Instance.SettingsGetUpdaterate();
                        try
                        {
                            updateRateInteger = Convert.ToInt32(updateRate);
                            updateRateInteger = Math.Min(SettingsManager.RefreshTimeMax, Math.Max(SettingsManager.RefreshTimeMin, updateRateInteger));
                        }
                        catch (Exception e)
                        {
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

                    if (processSettings)
                    {

                        KeyDataCollection displayOptions = SettingsManager.DefaultSettings["showingItems"];
                        foreach (KeyData keyData in displayOptions)
                        {
                            string key = keyData.KeyName;
                            bool state = await BrowserViewModel.Instance.AttachedJavascriptWrapper.Checkbox.IsChecked("#settingsInputDisplay" + (key.FirstCharToUpper()) + "");
                            SettingsManager.Instance.Settings["showingItems"][key] = state ? "true" : "false";
                        }
                    }

                    if (processSettings)
                    {
                        SettingsManager.Instance.Settings["api"]["key"] = apiKey;
                        SettingsManager.Instance.Settings["api"]["user"] = username;
                        SettingsManager.Instance.Settings["api"]["updateRate"] = updateRateInteger + "";
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

        public void finishSetup()
        {
            SetupFinished?.Invoke(null, null);
        }

        public void buttonProcessOsu()
        {
            InspectorManager.Instance.ProcessOsu(_internalWindow.Dispatcher);
        }
    }
}
