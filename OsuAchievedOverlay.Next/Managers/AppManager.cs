using CefSharp;
using IniParser.Model;
using OsuAchievedOverlay.Next.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace OsuAchievedOverlay.Next.Managers
{
    public class AppManager : Singleton<AppManager>
    {
        public void Start()
        {
            //BrowserViewModel.Instance.AttachedJavascriptWrapper.Show("#viewApp");

            //BrowserViewModel.Instance.SetAppVersionText("2.0.0dev");
            //BrowserViewModel.Instance.SetChromiumVersionText("CEF: " + Cef.CefSharpVersion + ", Chromium: " + Cef.ChromiumVersion);

            StartupManager.StartupFinished += (object sender, EventArgs e) => Prepare();

            Directory.CreateDirectory("Data");
            SettingsManager.Instance.FixSettingsPath();
            StartupManager.Instance.CheckSetup();
        }

        public void Stop(){
            SessionManager.Instance.Stop();
        }

        public void Prepare()
        {
            BrowserViewModel.Instance.LoadPage("index.html");

            //Forced sleep to give browser time to load (maybe switch to event later)
            Thread.Sleep(3);

            PopulateSettings();

            BrowserViewModel.Instance.SetAppVersionText(UpdateManager.version);
            BrowserViewModel.Instance.SetChromiumVersionText("CEF: " + Cef.CefSharpVersion + ", Chromium: " + Cef.ChromiumVersion);

            if (!NetworkManager.Instance.HasConnection())
                BrowserViewModel.Instance.SendNotification(NotificationType.Warning, "You are not connected to the internet");
            SessionManager.Instance.PrepareSession();
            UpdateManager.Instance.Start();

        }

        private void PopulateSettings(){
            BrowserViewModel.Instance.SettingsSetApikey(SettingsManager.Instance.Settings["api"]["key"]);
            BrowserViewModel.Instance.SettingsSetUsername(SettingsManager.Instance.Settings["api"]["user"]);
            BrowserViewModel.Instance.SettingsSetUpdaterate(SettingsManager.Instance.Settings["api"]["updateRate"]);
            BrowserViewModel.Instance.SettingsSetRoundingValue(SettingsManager.Instance.Settings["display"]["roundingValue"]);
            if (!string.IsNullOrEmpty(SettingsManager.Instance.Settings["misc"]["osuFolder"]))
                BrowserViewModel.Instance.SettingsSetOsuDirectory(SettingsManager.Instance.Settings["misc"]["osuFolder"]);
            BrowserViewModel.Instance.SettingsSetGamemode((OsuApiHelper.OsuMode)Enum.Parse(typeof(OsuApiHelper.OsuMode), SettingsManager.Instance.Settings["api"]["gamemode"]));
            
            KeyDataCollection displayOptions = SettingsManager.Instance.Settings["showingItems"];
            List<string> selectedKeys = new List<string>();
            foreach (KeyData keyData in displayOptions)
            {
                string key = keyData.KeyName;
                string id = "settingsInputDisplay" + key.FirstCharToUpper();
                if (keyData.Value == "true")
                    selectedKeys.Add(id);
                //BrowserViewModel.Instance.AttachedJavascriptWrapper.Checkbox.SetChecked("#settingsInputDisplay" + (key.FirstCharToUpper()) + "", keyData.Value == "true");
                //BrowserViewModel.Instance.AttachedJavascriptWrapper.SetProp(id, "selected", keyData.Value=="true");
            }
            string jsString = "" +
                "var values = '" + (string.Join(",", selectedKeys)) + "';" +
                "$('#settingsVisualSelectList').val(values.split(','));";
            BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded(jsString);
            SettingsManager.Instance.SettingsApply();
        }
    }
}
