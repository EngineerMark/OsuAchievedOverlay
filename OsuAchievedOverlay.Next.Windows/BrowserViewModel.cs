using CefSharp;
using CefSharp.WinForms;
using Humanizer;
using OsuAchievedOverlay.Next.Helpers;
using OsuAchievedOverlay.Next.JavaScript;
using OsuAchievedOverlay.Next.Managers;
using OsuApiHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        public void LoadPage(string page)
        {
            AttachedBrowser.Load(string.Format(@"{0}\wwwroot\" + page, FileManager.GetExecutableDirectory()));
        }

        public void SetAppVersionText(string text) => AttachedJavascriptWrapper.SetHtml("#aboutAppVersion", text);
        public void SetChromiumVersionText(string text) => AttachedJavascriptWrapper.SetHtml("#aboutCefVersion", text);

        public async Task<string> GetAppVersion()
        {
            return await AttachedJavascriptWrapper.GetHtml("#aboutAppVersion");
            //return Task.Run(async()=>await AttachedJavascriptWrapper.GetHtml("#aboutAppVersion")).Result;
        }

        #region SETTINGS
        public async Task<string> SettingsGetApiKey() => await AttachedJavascriptWrapper.TextInput.GetValue("#settingsInputApikey");
        public void SettingsSetApikey(string key) => AttachedJavascriptWrapper.TextInput.SetValue("#settingsInputApikey", key);

        public async Task<string> SettingsGetUsername() => await AttachedJavascriptWrapper.TextInput.GetValue("#settingsInputUsername");
        public void SettingsSetUsername(string name) => AttachedJavascriptWrapper.TextInput.SetValue("#settingsInputUsername", name);

        public async Task<string> SettingsGetUpdaterate() => await AttachedJavascriptWrapper.RangeInput.GetValue("#settingsInputRefreshTime");
        public void SettingsSetUpdaterate(string val) => AttachedJavascriptWrapper.RangeInput.SetValue("#settingsInputRefreshTime", val);

        public async Task<string> SettingsGetRoundingValue() => await AttachedJavascriptWrapper.RangeInput.GetValue("#settingsInputRoundDigit");
        public void SettingsSetRoundingValue(string val) => AttachedJavascriptWrapper.RangeInput.SetValue("#settingsInputRoundDigit", val);

        public async Task<string> SettingsGetOsuDirectory() => await AttachedJavascriptWrapper.TextInput.GetValue("#settingsInputOsuDir");
        public void SettingsSetOsuDirectory(string val) => AttachedJavascriptWrapper.TextInput.SetValue("#settingsInputOsuDir", val);

        public async Task<OsuMode> SettingsGetGamemode()
        {
            string selected = await AttachedJavascriptWrapper.SelectInput.GetValue("#settingsInputGamemodeDropdown");
            int selectedID = Convert.ToInt32(selected);
            return (OsuMode)selectedID;
        }
        public void SettingsSetGamemode(OsuMode val) => AttachedJavascriptWrapper.SelectInput.SetValue("#settingsInputGamemodeDropdown", ((int)val).ToString());
        #endregion

        public void ApplyUser(OsuUser user)
        {
            BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$(\"#tab_session_loader_view\").show();");
            BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$(\"#tab_session_default_view\").hide();");
            RegionInfo countryInfo = new RegionInfo(user.CountryCode);
            string query = "" +
                "$('#sessionUsername').html('" + user.Name + "');" +
                //"$('#sessionFlag').attr('src', './img/flags/" + user.CountryCode + ".png');" +
                //"$('#sessionFlag').attr('data-original-title', '" + (countryInfo.DisplayName) + "');" +
                "$('#sessionFlag').html('<i data-toggle=\"tooltip\" title=\""+countryInfo.DisplayName+ "\" class=\"material-tooltip-main twf twf-s twf-" + user.CountryCode.ToLower()+"\"></i>');" +
                "$('#sessionProfileImage').attr('src', 'https://a.ppy.sh/" + user.ID + "');" +
                "$('#sessionHeaderImage').attr('src', '" + ApiHelper.GetOsuUserHeaderUrl("https://osu.ppy.sh/users/" + user.ID) + "');" +
                "$('[data-toggle=\"tooltip\"]').tooltip();" +
            "";
            AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded(query);
            BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$(\"#tab_session_loader_view\").hide();");
            BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$(\"#tab_session_default_view\").show();");
        }

        public void ApplySession(Session session)
        {
            AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("ApplySession('" + session.ConvertToJson() + "', " + Convert.ToInt32(SettingsManager.Instance.Settings["display"]["roundingValue"]) + ");");
        }

        public void SendNotification(NotificationType notificationType, string message, int timeout = 1000)
        {
            string t;
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
            AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("toastr." + t + "('" + HttpUtility.JavaScriptStringEncode(message) + "', '', {timeOut: " + timeout + "});");
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
