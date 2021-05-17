using HeyRed.MarkdownSharp;
using Newtonsoft.Json;
using OsuAchievedOverlay.Github;
using OsuAchievedOverlay.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OsuAchievedOverlay.Managers
{
    public class UpdateManager : Manager
    {
        [Header("UI")]
        public GameObject UpdaterWindow;
        public GameObject UpdateReadyView;
        public GameObject UpdateProcessView;
        public Text TextNewVersion;
        public TMP_Text TextUpdateDetails;
        public TMP_Text TextVersionNumber;
        public TMP_Text TextUpdateProgress;
        public Text TextUpdateFromTo;

        [Header("Available Update")]
        public Release LatestUpdate = null;
        public List<Release> AllNewerUpdates;

        public override void Start()
        {
        }

        public override void Stop()
        {
        }

        public void InternalStart(Action callbackIfNoUpdate){
            TextVersionNumber.text = Application.version;
            string[] cmds = System.Environment.GetCommandLineArgs();

            UpdaterWindow.SetActive(false);
            UpdateReadyView.SetActive(false);
            UpdateProcessView.SetActive(false);

            GetAvailableUpdates();
            if (AllNewerUpdates.Count > 0)
            {
                LatestUpdate = AllNewerUpdates[0];
                if (cmds.Contains("-osustartupdate"))
                {
                    TextUpdateFromTo.text = "Updating from " + Application.version + " to " + LatestUpdate.Version;
                    UpdaterWindow.SetActive(true);
                    UpdateProcessView.SetActive(true);
                    ProcessUpdate(LatestUpdate);
                }
                else
                {

                    UpdaterWindow.SetActive(true);
                    UpdateReadyView.SetActive(true);
                    TextNewVersion.text = "New version available: " + LatestUpdate.Version;

                    TextUpdateDetails.text = string.Empty;

                    string val = "";
                    Markdown md = new Markdown();
                    int i = 0;
                    foreach (Release rel in AllNewerUpdates)
                    {
                        string transformedBody = md.Transform(rel.Body);
                        val += "Version " + rel.Version + "\n";
                        val += transformedBody + "\n";
                        i++;
                        if (i < AllNewerUpdates.Count)
                            val += "<hr />";
                    }

                    Ref<string> valRef = new Ref<string>(val);
                    //val = HtmlRichParser.Parse(val);
                    StartCoroutine(HtmlRichParser.ParseAsync(valRef, () =>
                    {
                        TextUpdateDetails.text = valRef.Value;
                    }));
                }
            }else{
                callbackIfNoUpdate.Invoke();
            }
        }

        public void ProcessUpdate(Release update)
        {
            DirectoryInfo appPath = FileManager.GetApplicationDirectory();
            UnityEngine.Debug.LogError(appPath);
            TextUpdateProgress.text = string.Empty;
            ThreadPool.QueueUserWorkItem((object stateInfo) =>
            {
                string relName = "OsuAchievedOverlay_" + update.Version;
                string zipurl = update.Assets[0].DownloadLink;

                string tempFolder = "temp_" + relName;
                string tempPath = Path.Combine(appPath.FullName, "temp", tempFolder);

                UnityEngine.Debug.LogError("Temp Folder: "+tempFolder);
                UnityEngine.Debug.LogError("Temp Path: "+ tempPath);

                WebClient client = new WebClient();
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    TextUpdateProgress.text += "Downloading " + relName + ".zip";
                });
                client.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) =>
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        TextUpdateProgress.text += "\nFinished downloading " + relName + ".zip";
                        TextUpdateProgress.text += "\nExtracting " + relName + ".zip";
                    });

                    if (Directory.Exists(Path.Combine(appPath.FullName, "temp_" + relName)))
                        Directory.Delete(Path.Combine(appPath.FullName, "temp_" + relName));

                    ZipFile.ExtractToDirectory(Path.Combine("temp",tempFolder) + ".zip", Path.Combine(appPath.FullName, tempPath));

                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        TextUpdateProgress.text += "\nFinished extracting " + relName + ".zip";
                        TextUpdateProgress.text += "\nCopying files...";
                    });

                    List<string> files = FileManager.GetAllFilesInDirectory(tempPath);

                    DirectoryInfo targetFolder = Directory.GetParent(appPath.FullName);
                    foreach (string file in files)
                    {
                        string from = Path.Combine(appPath.FullName, file);
                        string to = Path.Combine(targetFolder.FullName, file.Replace("temp\\temp_" + relName + "\\", ""));
                        UnityMainThreadDispatcher.Instance().Enqueue(()=>
                        {
                            TextUpdateProgress.text += "\nCopying "+to;
                        });
                        FileManager.MoveFile(from, to, true);
                    }

                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        TextUpdateProgress.text += "\nFinished copying files";
                        TextUpdateProgress.text += "\nDeleting temporary files";
                    });

                    Directory.Delete(tempPath, true);

                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        TextUpdateProgress.text += "\nFinished deleting temporary files";
                    });

                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        UnityEngine.Debug.Log("ProcessStart: " + Path.Combine(targetFolder.FullName, "OsuAchievedOverlay.exe"));
                        Process.Start(Path.Combine(appPath.FullName, "OsuAchievedOverlay.exe"), "-osufinishupdate");
                        Application.Quit();
                    });
                };
                client.DownloadFileAsync(new Uri(zipurl), tempPath + ".zip");
                UnityEngine.Debug.Log("Downloading \"" + zipurl + "\" to \"" + tempPath + ".zip\"");
            });
        }

        public void GetAvailableUpdates()
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

            if (releases.Count > 0)
            {
                AllNewerUpdates = new List<Release>();
                Version currentVersion = new Version(Application.version);

                foreach (Release rel in releases)
                {
                    Version relVersion = new Version(rel.Version);
                    if (relVersion > currentVersion)
                        AllNewerUpdates.Add(rel);
                }
            }
        }

        public void InitiateUpdate()
        {
            string path = System.Environment.CurrentDirectory;

            if (Directory.Exists(Path.Combine(path, "temp")))
                Directory.Delete(Path.Combine(path, "temp"), true);

            DirectoryInfo tempDir = Directory.CreateDirectory(Path.Combine(path, "temp"));
            tempDir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

            List<string> allFiles = FileManager.GetAllFilesInDirectory(path);

            foreach (string file in allFiles)
            {
                string relFile = file.Replace(path + "\\", "");

                string tpath = System.IO.Path.GetDirectoryName(relFile);

                if (path.Length > 0 && !Directory.Exists(Path.Combine(path, "temp", tpath)))
                    Directory.CreateDirectory(Path.Combine(path, "temp", tpath));

                File.Copy(relFile, Path.Combine(path, "temp", relFile), true);
            }

            Process.Start(System.IO.Path.Combine(path, "temp", "OsuAchievedOverlay.exe"), "-osustartupdate");
            Application.Quit();
        }
    }
}
