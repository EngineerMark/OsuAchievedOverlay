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
using System.Windows.Forms;

namespace OsuAchievedOverlay.Next.Managers
{
    public class AppManager : Singleton<AppManager>
    {
        public void Start()
        {
            //BrowserViewModel.Instance.AttachedJavascriptWrapper.Show("#viewApp");

            //BrowserViewModel.Instance.SetAppVersionText("2.0.0dev");
            //BrowserViewModel.Instance.SetChromiumVersionText("CEF: " + Cef.CefSharpVersion + ", Chromium: " + Cef.ChromiumVersion);
            ThemeManager.Instance.Start();

            StartupManager.StartupFinished += (object sender, EventArgs e) => Prepare();

            Directory.CreateDirectory("Data");
            SettingsManager.Instance.FixSettingsPath();
            StartupManager.Instance.CheckSetup();
        }

        public void Stop()
        {
            SessionManager.Instance.Stop();
        }

        public void Prepare()
        {
            BrowserViewModel.Instance.LoadPage("index.html");

            //Forced sleep to give browser time to load (maybe switch to event later)
            Thread.Sleep(3);


            Task.Run(()=>PopulateTabData());
            
        }

        private async void PopulateTabData()
        {
            await Task.Delay(1000);
            string tabsDataTask = "getTabFields();";
            JavascriptResponse res = await BrowserViewModel.Instance.AttachedBrowser.EvaluateScriptAsync(tabsDataTask);
            List<object> data = (List<object>)res.Result;
            data.ForEach(element =>
            {
                string name = (string)element;
                string path = "wwwroot/tabs/tab_" + name + ".html";
                if (File.Exists(path)){
                    string file_data = FileManager.ReadAllText(path);
                    string prepared_file = HttpUtility.JavaScriptStringEncode(file_data);
                    BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("populateTab('"+name+"','"+prepared_file+"');");
                }
                else
                {
                    BrowserViewModel.Instance.SendNotification(NotificationType.Danger, "Tab content for '"+name+"' does not exist! Please reinstall the program.");
                }
            });
            Proceed();
        }

        private void Proceed(){
            Thread.Sleep(100);
            PopulateSettings();


            BrowserViewModel.Instance.SetAppVersionText(AppVersion.Version);
            BrowserViewModel.Instance.SetChromiumVersionText("CEF: " + Cef.CefSharpVersion + ", Chromium: " + Cef.ChromiumVersion);

            if (!NetworkManager.Instance.HasConnection())
                BrowserViewModel.Instance.SendNotification(NotificationType.Warning, "You are not connected to the internet");
            SessionManager.Instance.PrepareSession();
            UpdateManager.Instance.Start();
        }

        private void PopulateSettings()
        {
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

            if(SettingsManager.Instance.Settings["display"]["nsfwMode"]=="true"){
                BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#settingsNsfwMode').click();");
            }

            BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#settingsThemeSelectList').empty();");
            foreach (Theme theme in ThemeManager.Instance.Themes)
            {
                string option = "<div class='form-check'>" +
                    "<input type='radio' class='form-check-input' theme_name='"+theme.InternalName+"' id='themeSelect_" + theme.InternalName + "' name='groupRadioThemes' "+(SettingsManager.Instance.Settings["display"]["theme"]==theme.InternalName ? "checked" : "")+">" +
                    "<label class='form-check-label' for='themeSelect_" + theme.InternalName + "'>" + theme.PrettyName + "</label>" +
                "</div>";
                BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#settingsThemeSelectList').append('" + HttpUtility.JavaScriptStringEncode(option) + "');");
            }

            SettingsManager.Instance.SettingsApply();
        }
    }
}
