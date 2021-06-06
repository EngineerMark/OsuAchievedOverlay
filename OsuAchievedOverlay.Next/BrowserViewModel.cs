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
            RegionInfo countryInfo = new RegionInfo(user.CountryCode);
            string query = "" +
                "$('#sessionUsername').html('" + user.Name + "');" +
                "$('#sessionFlag').attr('src', './img/flags/" + user.CountryCode + ".png');" +
                "$('#sessionFlag').attr('data-original-title', '" + (countryInfo.DisplayName) + "');" +
                "$('#sessionProfileImage').attr('src', 'https://a.ppy.sh/" + user.ID + "');" +
                "$('#sessionHeaderImage').attr('src', '" + ApiHelper.GetOsuUserHeaderUrl("https://osu.ppy.sh/users/" + user.ID) + "');" +
            "";
            AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded(query);
        }

        public void ApplySession(Session session)
        {
            //string query = "" +
            //    "$('#sessionTotalSSHCount').html('" + session.CurrentData.RankSilverSS + "');" +
            //    "$('#sessionTotalSSCount').html('" + session.CurrentData.RankGoldSS + "');" +
            //    "$('#sessionTotalSHCount').html('" + session.CurrentData.RankSilverS + "');" +
            //    "$('#sessionTotalSCount').html('" + session.CurrentData.RankGoldS + "');" +
            //    "$('#sessionTotalACount').html('" + session.CurrentData.RankA + "');" +
            //    "";
            //AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded(query);

            AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("ApplySession('" + session.ConvertToJson() + "', " + Convert.ToInt32(SettingsManager.Instance.Settings["display"]["roundingValue"]) + ");");

            //string query = "" +
            //    "$('#sessionDifferenceSSHCount').html('" + (session.DifferenceData.RankSilverSS >= 0 ? "+" : "-") + "" + session.DifferenceData.RankSilverSS + "');" +
            //    "$('#sessionDifferenceSSCount').html('" + (session.DifferenceData.RankGoldSS >= 0 ? "+" : "-") + "" + session.DifferenceData.RankGoldSS + "');" +
            //    "$('#sessionDifferenceSHCount').html('" + (session.DifferenceData.RankSilverS >= 0 ? "+" : "-") + "" + session.DifferenceData.RankSilverS + "');" +
            //    "$('#sessionDifferenceSCount').html('" + (session.DifferenceData.RankGoldS >= 0 ? "+" : "-") + "" + session.DifferenceData.RankGoldS + "');" +
            //    "$('#sessionDifferenceACount').html('" + (session.DifferenceData.RankA >= 0 ? "+" : "-") + "" + session.DifferenceData.RankA + "');" +
            //    "";
            //AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded(query);

            //string query = "" +
            //    "$('#sessionDifferenceSSHCount').removeClass('green').removeClass('red').removeClass('grey').addClass('" + (session.DifferenceData.RankSilverSS >= 0 ? (session.DifferenceData.RankSilverSS == 0 ? "grey" : "green") : "red") + "');" +
            //    "$('#sessionDifferenceSSCount').removeClass('green').removeClass('red').removeClass('grey').addClass('" + (session.DifferenceData.RankGoldSS >= 0 ? (session.DifferenceData.RankGoldSS == 0 ? "grey" : "green") : "red") + "');" +
            //    "$('#sessionDifferenceSHCount').removeClass('green').removeClass('red').removeClass('grey').addClass('" + (session.DifferenceData.RankSilverS >= 0 ? (session.DifferenceData.RankSilverS == 0 ? "grey" : "green") : "red") + "');" +
            //    "$('#sessionDifferenceSCount').removeClass('green').removeClass('red').removeClass('grey').addClass('" + (session.DifferenceData.RankGoldS >= 0 ? (session.DifferenceData.RankGoldS == 0 ? "grey" : "green") : "red") + "');" +
            //    "$('#sessionDifferenceACount').removeClass('green').removeClass('red').removeClass('grey').addClass('" + (session.DifferenceData.RankA >= 0 ? (session.DifferenceData.RankA == 0 ? "grey" : "green") : "red") + "');" +
            //    "";
            //AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded(query);

            //TimeSpan totalPlayTime = TimeSpan.FromSeconds(session.CurrentData.Playtime);
            //TimeSpan gainedPlayTime = TimeSpan.FromSeconds(session.DifferenceData.Playtime);
            //LabelTotalPlaytime.Content = totalPlayTime.Humanize(1, new System.Globalization.CultureInfo("en-US"), Humanizer.Localisation.TimeUnit.Hour);
            //string query = "" +
            //    "$('#sessionCurrentLevel').html('" + Math.Round(session.CurrentData.Level, 2).ToString("#,##0.###") + "');" +
            //    "$('#sessionCurrentTotalScore').html('" + session.CurrentData.TotalScore.ToString("#,##0.###") + "');" +
            //    "$('#sessionCurrentRankedScore').html('" + session.CurrentData.RankedScore.ToString("#,##0.###") + "');" +
            //    "$('#sessionCurrentWorldRank').html('#" + session.CurrentData.WorldRank + "');" +
            //    "$('#sessionCurrentCountryRank').html('#" + session.CurrentData.CountryRank + "');" +
            //    "$('#sessionCurrentPlaycount').html('" + session.CurrentData.Playcount.ToString("#,##0.###") + "');" +
            //    "$('#sessionCurrentPlaytime').html('" + totalPlayTime.Humanize(1, new System.Globalization.CultureInfo("en-US"), Humanizer.Localisation.TimeUnit.Hour) + "');" +
            //    "$('#sessionCurrentAccuracy').html('" + Math.Round(session.CurrentData.Accuracy, Convert.ToInt32(SettingsManager.Instance.Settings["display"]["roundingValue"])) + "%');" +
            //    "$('#sessionCurrentPerformance').html('" + Math.Round(session.CurrentData.Performance, Convert.ToInt32(SettingsManager.Instance.Settings["display"]["roundingValue"])).ToString("#,##0.###") + "');" +
            //    "";
            //AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded(query);

            //string query = "" +
            //    "$('#sessionDifferenceLevel').html('" + (session.DifferenceData.Level >= 0 ? "+" : "-") + Math.Round(session.DifferenceData.Level, 2).ToString("#,##0.###") + "');" +
            //    "$('#sessionDifferenceTotalScore').html('" + (session.DifferenceData.TotalScore >= 0 ? "+" : "-") + session.DifferenceData.TotalScore.ToString("#,##0.###") + "');" +
            //    "$('#sessionDifferenceRankedScore').html('" + (session.DifferenceData.RankedScore >= 0 ? "+" : "-") + session.DifferenceData.RankedScore.ToString("#,##0.###") + "');" +
            //    "$('#sessionDifferenceWorldRank').html('" + (session.DifferenceData.WorldRank > 0 ? "-" : "+") + session.DifferenceData.WorldRank + "');" +
            //    "$('#sessionDifferenceCountryRank').html('" + (session.DifferenceData.CountryRank > 0 ? "-" : "+") + session.DifferenceData.CountryRank + "');" +
            //    "$('#sessionDifferencePlaycount').html('" + (session.DifferenceData.Playcount >= 0 ? "+" : "-") + session.DifferenceData.Playcount.ToString("#,##0.###") + "');" +
            //    "$('#sessionDifferencePlaytime').html('" + (session.DifferenceData.Playtime >= 0 ? "+" : "-") + gainedPlayTime.Humanize(1, new System.Globalization.CultureInfo("en-US"), Humanizer.Localisation.TimeUnit.Hour, Humanizer.Localisation.TimeUnit.Second) + "');" +
            //    "$('#sessionDifferenceAccuracy').html('" + (session.DifferenceData.Accuracy >= 0 ? "+" : "-") + Math.Round(session.DifferenceData.Accuracy, Convert.ToInt32(SettingsManager.Instance.Settings["display"]["roundingValue"])) + "%');" +
            //    "$('#sessionDifferencePerformance').html('" + (session.DifferenceData.Performance >= 0 ? "+" : "-") + Math.Round(session.DifferenceData.Performance, Convert.ToInt32(SettingsManager.Instance.Settings["display"]["roundingValue"])).ToString("#,##0.###") + "');" +
            //    "";
            //AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded(query);
        }

        public void SendNotification(NotificationType notificationType, string message, int timeout = 1000)
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
