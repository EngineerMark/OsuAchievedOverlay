using CefSharp;
using CefSharp.WinForms;
using OsuAchievedOverlay.Next.JavaScript;
using OsuAchievedOverlay.Next.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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

        public void LoadPage(string page){
            AttachedBrowser.Load(string.Format(@"{0}\wwwroot\"+page, FileManager.GetExecutableDirectory()));
        }

        public void SetAppVersionText(string text) => AttachedJavascriptWrapper.SetHtml("#aboutAppVersion", text);
        public void SetChromiumVersionText(string text) => AttachedJavascriptWrapper.SetHtml("#aboutCefVersion", text);

        public async Task<string> GetAppVersion()
        {
            return await AttachedJavascriptWrapper.GetHtml("#aboutAppVersion");
            //return Task.Run(async()=>await AttachedJavascriptWrapper.GetHtml("#aboutAppVersion")).Result;
        }

        public async Task<string> SettingsGetApiKey()
        {
            return await AttachedJavascriptWrapper.Input.GetValue("#settingsInputApikey");
        }

        public void SettingsSetApikey(string key) => AttachedJavascriptWrapper.Input.SetValue("#settingsInputApikey", key);

        public async Task<string> SettingsGetUsername()
        {
            return await AttachedJavascriptWrapper.Input.GetValue("#settingsInputUsername");
        }
        public void SettingsSetUsername(string name) => AttachedJavascriptWrapper.Input.SetValue("#settingsInputUsername", name);

        public void SendNotification(NotificationType notificationType, string message)
        {
            string t = "";
            switch (notificationType)
            {
                case NotificationType.Info:
                default:
                    t = "info";
                    break;
                case NotificationType.Danger:
                    t = "error";
                    break;
                case NotificationType.Warning:
                    t = "warning";
                    break;
                case NotificationType.Success:
                    t = "success";
                    break;
            }
            AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("toastr." + t + "('" + HttpUtility.JavaScriptStringEncode(message) + "');");
        }
    }

    public enum NotificationType
    {
        Info,
        Warning,
        Success,
        Danger
    }
}
