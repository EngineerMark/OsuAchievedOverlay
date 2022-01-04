using CefSharp;
using CefSharp.WinForms;
using IniParser.Model;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using OsuAchievedOverlay.Next.Helpers;
using OsuAchievedOverlay.Next.JavaScript;
using OsuAchievedOverlay.Next.Managers;
using OsuApiHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using OsuAchievedOverlay.Next.Tools;

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

        public void updateHandlerVisit()
        {
            System.Diagnostics.Process.Start(UpdateManager.Instance.AvailableUpdate.HTMLURL);
        }

        public void sessionHandlerStartNew()
        {
            SessionManager.Instance.PrepareSession();
            BrowserViewModel.Instance.AttachedJavascriptWrapper.Modal.Hide("#modalNewSession");
            BrowserViewModel.Instance.SendNotification(NotificationType.Info, StringStorage.Get("Message.NewSessionStart"));
        }

        public void sessionHandlerLoad(string id)
        {
            //Can't use regular int64 values since c# > js > c# causes glitches in it and we end up with some different value
            string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(id));
            long longID = Convert.ToInt64(decoded);
            SessionFileData sessionData = SessionManager.Instance.SessionFiles.Find(a => a.FileDate == longID);

            if (sessionData != null)
            {
                BrowserViewModel.Instance.AttachedJavascriptWrapper.Modal.Hide("#modalLoadSession");
                SessionLoadFromFile(Path.Combine(sessionData.FileLocation, sessionData.FileName + sessionData.FileExtension));
            }
            else
                BrowserViewModel.Instance.SendNotification(NotificationType.Danger, StringStorage.Get("Message.LoadSessionFail"));
        }

        public void sessionHandlerLoadFromFile()
        {
            BrowserViewModel.Instance.AttachedJavascriptWrapper.Modal.Hide("#modalLoadSession");
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                SessionLoadFromFile(openFileDialog.FileName);
            }
        }

        private void SessionLoadFromFile(string path)
        {
            string data;
            try
            {
                data = FileManager.ReadAllText(path);
            }
            catch (Exception)
            {
                BrowserViewModel.Instance.SendNotification(NotificationType.Danger, StringStorage.Get("Message.OpenFileFail"));
                return;
            }

            Session loaded;
            try
            {
                loaded = JsonConvert.DeserializeObject<Session>(data);
            }
            catch (Exception)
            {
                BrowserViewModel.Instance.SendNotification(NotificationType.Danger, StringStorage.Get("Message.LoadSessionFail"));
                return;
            }

            if (loaded != null)
            {
                SessionManager.Instance.PrepareSession(loaded);
                SessionManager.Instance.AddFile(path);
            }
            else
            {
                BrowserViewModel.Instance.SendNotification(NotificationType.Danger, StringStorage.Get("Message.LoadSessionDeserializeFail"));
            }
        }

        public void sessionHandlerSave(bool _readonly = false)
        {
            if (SessionManager.Instance != null && SessionManager.Instance.CurrentSession != null)
            {
                Session clonedSession = (Session)SessionManager.Instance.CurrentSession.Clone();
                clonedSession.ReadOnly = _readonly;
                string json = clonedSession.ConvertToJson();
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    FileManager.WriteAllText(saveFileDialog.FileName, json);
                    BrowserViewModel.Instance.AttachedJavascriptWrapper.Modal.Hide("#modalSaveSession");
                    BrowserViewModel.Instance.SendNotification(NotificationType.Success, StringStorage.Get("Message.SessionSaved"));
                    SessionManager.Instance.AddFile(saveFileDialog.FileName);
                }
            }
            else
            {
                BrowserViewModel.Instance.SendNotification(NotificationType.Danger, StringStorage.Get("Message.SessionSaveFail"));
            }
        }

        public void showDevTools()
        {
            _internalBrowser.ShowDevTools();
        }

        public void openUrl(string url)
        {
            System.Diagnostics.Process.Start(url);
        }

        public void dirDialogOsuInstall()
        {
            _internalWindow.Dispatcher.Invoke(() =>
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog
                {
                    IsFolderPicker = true,
                    Title = StringStorage.Get("Title.OsuSelector")
                };
                CommonFileDialogResult result = dialog.ShowDialog();
                if (result == CommonFileDialogResult.Ok)
                {
                    JsExecuter.GetBrowser().ExecuteScriptAsyncWhenPageLoaded("$('#settingsInputOsuDir').val('" + HttpUtility.JavaScriptStringEncode(dialog.FileName) + "');");
                    JsExecuter.AddClass("#settingsInputOsuDirLabel", "active");
                }
            });
        }

        public void buttonSaveSettings(bool checkUsername = true, bool newSession = false)
        {
            SettingsManager.Instance.SaveChangedSettings(checkUsername, newSession);
        }

        public void finishSetup()
        {
            SetupFinished?.Invoke(null, null);
        }

        public string toolUsersSearch(string input)
        {
            return JsonConvert.SerializeObject(ToolUsers.Instance.FindUser(input));
        }

        public string getOsuUserProfile(int userid)
        {
            return JsonConvert.SerializeObject(ApiHelper.GetOsuUserProfile("https://osu.ppy.sh/users/" + userid));
        }

        public string osuUserGetHeader(int userid)
        {
            return ApiHelper.GetOsuUserHeaderUrl("https://osu.ppy.sh/users/" + userid);
        }
    }
}
