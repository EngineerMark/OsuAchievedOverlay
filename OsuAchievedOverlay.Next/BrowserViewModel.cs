using CefSharp.Wpf;
using OsuAchievedOverlay.Next.JavaScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OsuAchievedOverlay.Next
{
    /// <summary>
    /// Layer between the code and the browser/display for osu!achieved
    /// </summary>
    public class BrowserViewModel : Singleton<BrowserViewModel>
    {
        public ChromiumWebBrowser AttachedBrowser { get; set; }
        public JSWrapper AttachedJavascriptWrapper { get; set; }

        public void SetAppVersionText(string text) => AttachedJavascriptWrapper.SetHtml("#aboutAppVersion", text);
        public void SetChromiumVersionText(string text) => AttachedJavascriptWrapper.SetHtml("#aboutCefVersion", text);

        public async Task<string> GetAppVersion()
        {
            return await AttachedJavascriptWrapper.GetHtml("#aboutAppVersion");
            //return Task.Run(async()=>await AttachedJavascriptWrapper.GetHtml("#aboutAppVersion")).Result;
        }

        public async Task<string> SettingsGetApiKey(){
            return await AttachedJavascriptWrapper.GetInputValue("#settingsInputApikey");
        }
    }
}
