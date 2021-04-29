using Newtonsoft.Json;
using OsuAchievedOverlay.Github;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace OsuAchievedOverlay.Managers
{
    public class UpdateManager : Manager<UpdateManager>
    {
        private const string version = "1.0.8";

        public Queue<Release> Updates { get; set; }

        public UpdateManager()
        {
            Start();
        }

        public override void Start()
        {
            Updates = new Queue<Release>();
        }

        public override void Stop()
        {
        }

        public List<Release> GetAvailableUpdates()
        {
            string data = "";
            try
            {
                data = ApiHelper.GetData("https://api.github.com/repos/EngineerMark/OsuAchievedOverlay/releases");
            }
            catch (Exception e)
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

        public void PerformUpdates()
        {
            List<Release> releases = GetAvailableUpdates();

            WindowManager.Instance.UpdateWin.LabelUpdatingWhat.Content = "Updating from " + version + " to " + releases[0].Version;

            releases.Reverse();

            foreach (Release rel in releases)
                Updates.Enqueue(rel);

            ProcessUpdate(Updates.Dequeue());
        }

        private void ProcessUpdate(Release update)
        {
            ThreadPool.QueueUserWorkItem((Object stateInfo) =>
            {
                string relName = "OsuAchievedOverlay_" + update.Version;
                string zipurl = update.Assets[0].DownloadLink;

                DirectoryInfo root = FileManager.Instance.GetApplicationDirectory();
                string tempFolder = "temp_" + relName;
                string tempPath = Path.Combine(root.FullName, tempFolder);

                WebClient client = new WebClient();
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    WindowManager.Instance.UpdateWin.ProgressBarUpdater.IsIndeterminate = false;
                    WindowManager.Instance.UpdateWin.TextBlockProgressStates.Text += "\nStarted downloading " + relName + ".zip";
                }));
                client.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        WindowManager.Instance.UpdateWin.ProgressBarUpdater.Value = e.ProgressPercentage;
                    }));
                };
                client.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) =>
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        WindowManager.Instance.UpdateWin.ProgressBarUpdater.IsIndeterminate = true;
                        WindowManager.Instance.UpdateWin.TextBlockProgressStates.Text += "\nFinished downloading " + relName + ".zip";
                        WindowManager.Instance.UpdateWin.TextBlockProgressStates.Text += "\nExtracting " + relName + ".zip";
                    }));

                    if (Directory.Exists("temp_" + relName))
                        Directory.Delete("temp_" + relName, true);

                    ZipFile.ExtractToDirectory(tempPath + ".zip", Path.Combine(root.FullName, tempPath));

                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        WindowManager.Instance.UpdateWin.TextBlockProgressStates.Text += "\nFinished extracting " + relName + ".zip";
                        WindowManager.Instance.UpdateWin.TextBlockProgressStates.Text += "\nMoving files";
                    }));

                    List<string> files = FileManager.Instance.GetAllFilesInDirectory(tempPath);

                    DirectoryInfo targetFolder = Directory.GetParent(root.FullName);
                    foreach (string file in files)
                    {
                        string from = Path.Combine(root.FullName, file);
                        string to = Path.Combine(targetFolder.FullName, file.Replace("temp\\temp_" + relName + "\\", ""));
                        FileManager.Instance.MoveFile(from, to, true);
                    }

                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        WindowManager.Instance.UpdateWin.TextBlockProgressStates.Text += "\nFinished moving files";
                        WindowManager.Instance.UpdateWin.TextBlockProgressStates.Text += "\nDeleting temporary files";
                    }));

                    Directory.Delete(tempPath, true);

                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        WindowManager.Instance.UpdateWin.TextBlockProgressStates.Text += "\nDone deleting temporary files";
                    }));

                    if (Updates.Count > 0)
                        ProcessUpdate(Updates.Dequeue());
                    else
                    {
                        Process.Start(Path.Combine(targetFolder.FullName, "OsuAchievedOverlay.exe"), "-osufinishupdate");
                        Application.Current.Shutdown();
                    }
                };
                client.DownloadFileAsync(new Uri(zipurl), tempPath + ".zip");
            });
        }
    }
}
