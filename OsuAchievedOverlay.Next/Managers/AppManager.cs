using CefSharp;
using OsuAchievedOverlay.Next.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        public void Prepare()
        {
            BrowserViewModel.Instance.LoadPage("index.html");

            //Forced sleep to give browser time to load (maybe switch to event later)
            Thread.Sleep(3);

            PopulateSettings();

            BrowserViewModel.Instance.SetAppVersionText("2.0.0dev");
            BrowserViewModel.Instance.SetChromiumVersionText("CEF: " + Cef.CefSharpVersion + ", Chromium: " + Cef.ChromiumVersion);

            OsuApiHelper.OsuUser user = OsuApiHelper.OsuApi.GetUser(SettingsManager.Instance.Settings["api"]["user"]);
            //Test
            Session s = new Session()
            {
                InitialData = SessionData.FromUser(user),
                CurrentData = SessionData.FromUser(user)
            };
            s.DifferenceData = s.CurrentData - s.InitialData;

            BrowserViewModel.Instance.ApplyUser(user);
            BrowserViewModel.Instance.ApplySession(s);
        }

        private void PopulateSettings(){
            BrowserViewModel.Instance.SettingsSetApikey(SettingsManager.Instance.Settings["api"]["key"]);
            BrowserViewModel.Instance.SettingsSetUsername(SettingsManager.Instance.Settings["api"]["user"]);
            BrowserViewModel.Instance.SettingsSetUpdaterate(SettingsManager.Instance.Settings["api"]["updateRate"]);
            if(!string.IsNullOrEmpty(SettingsManager.Instance.Settings["misc"]["osuFolder"]))
                BrowserViewModel.Instance.SettingsSetOsuDirectory(SettingsManager.Instance.Settings["misc"]["osuFolder"]);
            BrowserViewModel.Instance.SettingsSetGamemode((OsuApiHelper.OsuMode)Enum.Parse(typeof(OsuApiHelper.OsuMode), SettingsManager.Instance.Settings["api"]["gamemode"]));
        }
    }
}
