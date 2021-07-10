using CefSharp;
using MarkdownSharp;
using Newtonsoft.Json;
using OsuAchievedOverlay.Next.Github;
using OsuAchievedOverlay.Next.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace OsuAchievedOverlay.Next.Managers
{
    public class UpdateManager : Singleton<UpdateManager>
    {
        public const string version = "1.0.7";

        public Release AvailableUpdate { get; set; } = null;

        public void Start(){
            BrowserViewModel.Instance.AttachedJavascriptWrapper.Hide("#updateButtonInstall");
            CheckAsync();
        }

        public void CheckAsync(){
            Task.Run(() =>
            {
                //BrowserViewModel.Instance.AttachedJavascriptWrapper.SetElementDisabled("#updateButtonDownload", true);
                List<Release> available = GetAvailableUpdates();
                BrowserViewModel.Instance.AttachedJavascriptWrapper.Hide("#cardUpdateReady");

                if (available.Count > 0)
                {
                    AvailableUpdate = available[0];

                    if(AvailableUpdate!=null)
                        PrepareUpdate(AvailableUpdate);
                }
                //if (available.Count > 0)
                //    AvailableUpdate = available[0];

                //if (AvailableUpdate != null)
                //{
                //    PrepareUpdate(AvailableUpdate);
                //}
                //BrowserViewModel.Instance.AttachedJavascriptWrapper.SetElementDisabled("#updateButtonDownload", false);
            });
        }

        public void PrepareUpdate(Release update){
            BrowserViewModel.Instance.AttachedJavascriptWrapper.Show("#cardUpdateReady");
            BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#updatemanagerVersionID", update.Version);

            BrowserViewModel.Instance.AttachedJavascriptWrapper.Show("#navigationUpdateWarning");
            BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#updatemanagerVersionBody", "No update info to show. Perhaps something went wrong?");
            Markdown md = new Markdown();
            string updateString = "<div style=\"-webkit-text-size-adjust: 50%;\">";
            updateString += md.Transform(update.Body);
            updateString += "</div>";
            BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#updatemanagerVersionBody", HttpUtility.JavaScriptStringEncode(updateString));
        }

        public List<Release> GetAvailableUpdates()
        {
            string data = "";
            try
            {
                data = ApiHelper.GetData("https://api.github.com/repos/EngineerMark/OsuAchievedOverlay/releases");
            }
            catch (Exception)
            {
                data = "[]";
            }

            List<Release> releases = JsonConvert.DeserializeObject<List<Release>>(data);

            Version currentVersion = new Version(version);
            releases.RemoveAll(delegate (Release release)
            {
                string _v = release.Version;
                Version v = new Version(_v);
                bool res = currentVersion >= v;
                return res;
            });

            return releases;
        }
    }
}
