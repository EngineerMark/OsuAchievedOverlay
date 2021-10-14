using CefSharp;
using CefSharp.WinForms;
using IniParser.Model;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using osu_database_reader.Components.Player;
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
using osu_database_reader.Components.Beatmaps;

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

        public string requestBeatmapApiData(string hash, int mods, int mode){
            BeatmapEntry local_map = ToolInspector.Instance.CurrentDatabase.Beatmaps.Find(x=>x.BeatmapChecksum==hash);
            if(local_map == null){
                return null;
            }
            OsuBeatmap map = OsuApi.GetBeatmap(local_map.BeatmapId.ToString(), (OsuMods)mods, (OsuMode)mode);
            if(map==null){
                return null;
            }
            map.MapStats.Beatmap = null; // loop fix
            OsuPlay simulatedPlay = new OsuPlay();
            simulatedPlay.Mods = (OsuMods)mods;
            simulatedPlay.Mode = (OsuMode)mode;
            OsuPerformance pp = new OsuPerformance(simulatedPlay, map);
            AccuracyDistribution acc95 = new AccuracyDistribution((int)map.ObjectCount, 0, 0.95f);
            AccuracyDistribution acc98 = new AccuracyDistribution((int)map.ObjectCount, 0, 0.98f);
            AccuracyDistribution acc99 = new AccuracyDistribution((int)map.ObjectCount, 0, 0.99f);
            AccuracyDistribution acc100 = new AccuracyDistribution((int)map.ObjectCount, 0, 1f);

            float pp95 = pp.CalculatePerformance((float)map.MaxCombo, acc95.Hits50, acc95.Hits100, acc95.Hits300, acc95.Misses);
            float pp98 = pp.CalculatePerformance((float)map.MaxCombo, acc98.Hits50, acc98.Hits100, acc98.Hits300, acc98.Misses);
            float pp99 = pp.CalculatePerformance((float)map.MaxCombo, acc99.Hits50, acc99.Hits100, acc99.Hits300, acc99.Misses);
            float pp100 = pp.CalculatePerformance((float)map.MaxCombo, acc100.Hits50, acc100.Hits100, acc100.Hits300, acc100.Misses);

            Tuple<OsuBeatmap, float, float, float, float> data = new Tuple<OsuBeatmap, float, float, float, float>(map, pp95, pp98, pp99, pp100);


            return JsonConvert.SerializeObject(data);
        }

        public void beatmapBrowserSetPage(int index){
            ToolInspector.Instance.InspectorBeatmapListing.LoadPage(index);
        }

        public void beatmapBrowserSearch(string query){
            ToolInspector.Instance.InspectorBeatmapListing.ApplySearchQuery(Encoding.UTF8.GetString(Convert.FromBase64String(query)));
        }

        public void requestBeatmapScores(string hash){
            string encoded = "";
            if(ToolInspector.Instance.CurrentScores.Beatmaps.ContainsKey(hash)){
                encoded = JsonConvert.SerializeObject(ToolInspector.Instance.CurrentScores.Beatmaps[hash]);
            }
            BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("beatmapViewerPopulateScores('"+ encoded +"');");
        }

        public void updateHandlerVisit(){
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

        public void sessionHandlerLoadFromFile(){
            BrowserViewModel.Instance.AttachedJavascriptWrapper.Modal.Hide("#modalLoadSession");
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt"
            };
            if (openFileDialog.ShowDialog()==true){
                SessionLoadFromFile(openFileDialog.FileName);
            }
        }

        private void SessionLoadFromFile(string path){
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
            if(SessionManager.Instance!=null && SessionManager.Instance.CurrentSession!=null){
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
            else{
                BrowserViewModel.Instance.SendNotification(NotificationType.Danger, StringStorage.Get("Message.SessionSaveFail"));
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
                CommonOpenFileDialog dialog = new CommonOpenFileDialog
                {
                    IsFolderPicker = true,
                    Title = StringStorage.Get("Title.OsuSelector")
                };
                CommonFileDialogResult result = dialog.ShowDialog();
                JsExecuter.GetBrowser().ExecuteScriptAsyncWhenPageLoaded("$('#settingsInputOsuDir').val('"+ HttpUtility.JavaScriptStringEncode(dialog.FileName) + "');");
                JsExecuter.AddClass("#settingsInputOsuDirLabel", "active");
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

        public void buttonProcessOsu()
        {
            ToolInspector.Instance.ProcessOsu(_internalWindow.Dispatcher);
        }
    }
}
