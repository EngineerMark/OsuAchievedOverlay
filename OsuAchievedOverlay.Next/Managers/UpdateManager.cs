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
        public const string version = "2.0.0";

        public Release AvailableUpdate { get; set; } = null;

        public void Start(){
            BrowserViewModel.Instance.AttachedJavascriptWrapper.Hide("#updateButtonInstall");
            CheckAsync();
        }

        public void InstallUpdate(Release update){
            Task.Run(() =>
            {
                if(!Directory.Exists("downloads") || !File.Exists(Path.Combine("downloads", "update_" + update.Version + ".zip"))){
                    BrowserViewModel.Instance.SendNotification(NotificationType.Danger, "Install files missing. Redownload?");
                    return;
                }
                BrowserViewModel.Instance.AttachedJavascriptWrapper.SetElementDisabled("#updateButtonDownload", true);
                BrowserViewModel.Instance.AttachedJavascriptWrapper.SetElementDisabled("#updateButtonInstall", true);

                BrowserViewModel.Instance.SendNotification(NotificationType.Info, "Starting install of " + update.Version);

                if (Directory.Exists("temp"))
                    Directory.Delete("temp", true);

                DirectoryInfo tempDir = Directory.CreateDirectory("temp");
                tempDir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

                string root = FileManager.GetExecutableDirectory();
                string[] ignorePaths = new string[]{
                    Path.Combine(root, "temp"),
                    Path.Combine(root, "data"),
                };

                List<string> files = FileManager.GetAllFilesInDirectory(root);

                files.ForEach(file => file = file.Replace(root + "\\", ""));

                foreach (string file in files){
                    string localPath = file.Replace(root+"\\", "");
                    string from = Path.Combine(localPath);
                    string to = Path.Combine(Path.Combine("temp", localPath));
                    FileManager.CopyFile(from, to);
                }

                Process.Start(Path.Combine(root, "temp", "OsuAchievedOverlay.Next.exe"), "-install-update");
                Application.Current.Shutdown();
            });
        }

        public void DownloadUpdate(Release update){
            Task.Run(() =>
            {
                BrowserViewModel.Instance.AttachedJavascriptWrapper.SetElementDisabled("#updateButtonDownload", true);

                if (Directory.Exists("temp"))
                    Directory.Delete("temp", true);

                DirectoryInfo tempDir = Directory.CreateDirectory("temp");
                tempDir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

                string relName = "OsuAchievedOverlay_" + update.Version;
                string zipurl = update.Assets[0].DownloadLink;

                string root = FileManager.GetExecutableDirectory();
                string downloadPath = Path.Combine(root, "downloads");

                if (!Directory.Exists("downloads"))
                    Directory.CreateDirectory("downloads");

                WebClient client = new WebClient();
                BrowserViewModel.Instance.SendNotification(NotificationType.Info, "Started downloading version " + update.Version);

                client.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) =>
                {
                    BrowserViewModel.Instance.SendNotification(NotificationType.Success, "Finished downloading version " + update.Version);
                    BrowserViewModel.Instance.AttachedJavascriptWrapper.SetElementDisabled("#updateButtonDownload", false);
                    BrowserViewModel.Instance.AttachedJavascriptWrapper.Show("#updateButtonInstall");
                };

                if (File.Exists(Path.Combine(downloadPath, "update_" + update.Version + ".zip")))
                    File.Delete(Path.Combine(downloadPath, "update_" + update.Version + ".zip"));

                client.DownloadFileAsync(new Uri(zipurl), Path.Combine(downloadPath, "update_" + update.Version + ".zip"));
            });
        }

        public void CheckAsync(){
            Task.Run(() =>
            {
                BrowserViewModel.Instance.AttachedJavascriptWrapper.SetElementDisabled("#updateButtonDownload", true);
                List<Release> available = GetAvailableUpdates();
                BrowserViewModel.Instance.AttachedJavascriptWrapper.Hide("#cardUpdateReady");
                if (available.Count > 0)
                    AvailableUpdate = available[0];

                if (AvailableUpdate != null)
                {
                    PrepareUpdate(AvailableUpdate);
                }
                BrowserViewModel.Instance.AttachedJavascriptWrapper.SetElementDisabled("#updateButtonDownload", false);
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
