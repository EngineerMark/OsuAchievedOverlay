using CefSharp;
using Humanizer;
using Humanizer.Bytes;
using Newtonsoft.Json;
using osu.Shared;
using osu_database_reader.BinaryFiles;
using osu_database_reader.Components.Beatmaps;
using OsuAchievedOverlay.Next.Helpers;
using OsuAchievedOverlay.Next.Managers;
using OsuAchievedOverlay.Next.OsuWeb;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Threading;

namespace OsuAchievedOverlay.Next.Tools
{
    public class ToolInspector : Singleton<ToolInspector>
    {
        public bool IsUpdating { get; set; }
        public OsuDb CurrentDatabase { get; set; }
        public CollectionDb CurrentCollections { get; set; }
        public ScoresDb CurrentScores { get; set; }
        public List<BeatmapSetEntry> BeatmapSets { get; set; }
        private Object BeatmapSetLock = new Object();
        public string SongsPath { get; set; }

        public InspectorBeatmapListing InspectorBeatmapListing { get; set; }

        private async Task ProcessBeatmaps(){
            //int i = 0;
            //for(int id=0; id<CurrentDatabase.Beatmaps.Count;id++)
            //{
            //    if (i % 10 == 0)
            //        BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#settingsProcessOsuButtonProcessText').html('processing (creating sets " + i + "/" + CurrentDatabase.Beatmaps.Count + ")');");
            //    i++;
            //    ProcessBeatmap(id);
            //}

            int threads = 5;
            int mapsPerThread = (int)System.Math.Ceiling((double)CurrentDatabase.Beatmaps.Count / threads);
            int mapsCompleted = 0;
            List<Thread> localThreads = new List<Thread>();

            ConcurrentDictionary<int, BeatmapSetEntry> threadsafeBeatmapSets = new ConcurrentDictionary<int, BeatmapSetEntry>();

            for (int threadID = 0; threadID < threads; threadID++)
            {
                int localThreadID = threadID;

                Thread t = new Thread(new ThreadStart(()=>{
                    for (int i = 0; i < mapsPerThread; i++)
                    {
                        bool state = ProcessBeatmap(ref threadsafeBeatmapSets, localThreadID * mapsPerThread + i);
                        if(state)
                            Interlocked.Increment(ref mapsCompleted);
                    }
                }));
                localThreads.Add(t);
                t.Start();
            }

            while(mapsCompleted!=CurrentDatabase.Beatmaps.Count){
                BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#settingsProcessOsuButtonProcessText').html('processing (creating sets " + mapsCompleted + "/" + CurrentDatabase.Beatmaps.Count + ")');");
                await Task.Delay(100);
            }

            localThreads.ForEach(t =>
            {
                t.Join();
            });

            BeatmapSets = new List<BeatmapSetEntry>(threadsafeBeatmapSets.Values);
        }

        private bool ProcessBeatmap(ref ConcurrentDictionary<int, BeatmapSetEntry> threadsafeBeatmapSets, int id){
            if (id >= CurrentDatabase.Beatmaps.Count)
                return false;

            BeatmapEntry difficulty = CurrentDatabase.Beatmaps[id];
            BeatmapSetEntry set = null;
            //lock (BeatmapSetLock)
            //{
            //    set = BeatmapSets.Find(a => a.BeatmapSetID == difficulty.BeatmapSetId);
            //}
            if (threadsafeBeatmapSets.ContainsKey(difficulty.BeatmapSetId))
                set = threadsafeBeatmapSets[difficulty.BeatmapSetId];
            if (set != null)
            {
                set.Difficulties++;
                string[] tags = difficulty.SongTags.Split(' ');
                if (tags.Length > 0)
                    foreach (string tag in tags)
                        set.SongTags.Add(tag);
                set.Beatmaps.Add(difficulty);
            }
            else
            {
                set = new BeatmapSetEntry();
                set.BeatmapSetID = difficulty.BeatmapSetId;
                set.Title = difficulty.Title;
                set.Artist = difficulty.Artist;
                set.Creator = difficulty.Creator;
                set.BeatmapFolder = difficulty.FolderName.Trim();
                set.RankStatus = difficulty.RankedStatus;
                set.Beatmaps.Add(difficulty);
                set.Difficulties = 1;
                if (Directory.Exists(Path.Combine(SettingsManager.Instance.Settings["misc"]["osuFolder"], SongsPath, set.BeatmapFolder)))
                {
                    Queue<string> images = new Queue<string>();
                    List<string> _imgs = ApiHelper.GetImages(Path.Combine(SettingsManager.Instance.Settings["misc"]["osuFolder"], SongsPath, set.BeatmapFolder));
                    _imgs.ForEach(a => images.Enqueue(a));
                    string backgroundImage = "";
                    if (images.Count > 0)
                    {
                        backgroundImage = images.Dequeue();
                        if (images.Count > 1)
                        {
                            //string nextImage = images.rem
                            while (images.Count > 0)
                            {
                                string next = images.Dequeue();
                                FileInfo thisFile = new FileInfo(next);
                                FileInfo currentFile = new FileInfo(backgroundImage);

                                if (next.Length > currentFile.Length)
                                    backgroundImage = next;
                            }
                        }
                    }

                    set.SongSource = difficulty.SongSource;
                    set.AudioFileName = difficulty.AudioFileName;

                    set.BackgroundPath = backgroundImage;
                    string[] tags = difficulty.SongTags.Split(' ');
                    if (tags.Length > 0)
                        foreach (string tag in tags)
                            set.SongTags.Add(tag);
                    threadsafeBeatmapSets.TryAdd(set.BeatmapSetID, set);
                }
            }
            return true;
        }

        public void ProcessOsu(Dispatcher dispatcher)
        {
            dispatcher.Invoke(() =>
            {
                ThreadPool.QueueUserWorkItem((object stateInfo) =>
                {
                    Task.Run(async () =>
                    {
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        BrowserViewModel.Instance.AttachedJavascriptWrapper.Hide("#beatmapProcessTimeString");
                        BrowserViewModel.Instance.AttachedJavascriptWrapper.SetElementDisabled("#settingsProcessOsu", true);
                        BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#settingsProcessOsu", "<span class=\"spinner-border spinner-border-sm\" role=\"status\" aria-hidden=\"true\"></span> <span id=\"settingsProcessOsuButtonProcessText\">processing</span>");
                        IsUpdating = true;

                        if (CurrentDatabase != null)
                            CurrentDatabase.Beatmaps.Clear();

                        string configPath = Path.Combine(SettingsManager.Instance.Settings["misc"]["osuFolder"], "osu!." + Environment.UserName + ".cfg");
                        if (File.Exists(configPath))
                        {
                            string[] configLines = File.ReadAllLines(configPath);
                            SongsPath = string.Empty;
                            foreach (string s in configLines)
                            {
                                if (s.Contains("BeatmapDirectory = "))
                                    SongsPath = s.Replace("BeatmapDirectory = ", "");
                            }

                            bool validConfig = true;
                            if (string.IsNullOrEmpty(SongsPath))
                            {
                                BrowserViewModel.Instance.SendNotification(NotificationType.Danger, StringStorage.Get("Message.Osu.NoConfig"));
                                validConfig = false;
                            }

                            if (validConfig)
                            {
                                BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#settingsProcessOsuButtonProcessText').html('processing (loading beatmaps)');");
                                CurrentDatabase = OsuDb.Read(System.IO.Path.Combine(SettingsManager.Instance.Settings["misc"]["osuFolder"], "osu!.db"));
                                //BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#inspectorBeatmapDiffCount", ""+CurrentDatabase.Beatmaps.Count);
                                BrowserViewModel.Instance.AttachedJavascriptWrapper.Show("#inspectorBeatmapDiffCountParent");
                                BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#inspectorBeatmapDiffCount').countTo({from: 0, to: " + CurrentDatabase.Beatmaps.Count + "});");

                                BeatmapSets?.Clear();
                                BeatmapSets = new List<BeatmapSetEntry>();

                                BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#settingsProcessOsuButtonProcessText').html('processing (creating sets 0/" + CurrentDatabase.Beatmaps.Count + ")');");

                                await ProcessBeatmaps();

                                //BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#inspectorBeatmapSetCount", "" + BeatmapSets.Count);
                                BrowserViewModel.Instance.AttachedJavascriptWrapper.FadeIn("#inspectorBeatmapSetCountParent");
                                BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#inspectorBeatmapSetCount').countTo({from: 0, to: " + BeatmapSets.Count + "});");

                                BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#settingsProcessOsuButtonProcessText').html('processing (reading collections)');");
                                CurrentCollections = CollectionDb.Read(Path.Combine(SettingsManager.Instance.Settings["misc"]["osuFolder"], "collection.db"));
                                //BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#inspectorCollectionCount", "" + CurrentCollections.Collections.Count);
                                BrowserViewModel.Instance.AttachedJavascriptWrapper.FadeIn("#inspectorCollectionCountParent");
                                BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#inspectorCollectionCount').countTo({from: 0, to: " + CurrentCollections.Collections.Count + "});");

                                BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#inspectorCollectionList').html('')");
                                foreach (Collection coll in CurrentCollections.Collections)
                                {
                                    BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#inspectorCollectionList').append('<button type=\\\"button\\\" class=\\\"btn btn-block btn-sm btn-dark btn-rounded\\\" style=\\\"margin:10px auto;\\\"><span class=\\\"float-left\\\">" + coll.Name + "</span></button>')");
                                }

                                BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#settingsProcessOsuButtonProcessText').html('processing (reading scores)');");
                                CurrentScores = ScoresDb.Read(Path.Combine(SettingsManager.Instance.Settings["misc"]["osuFolder"], "scores.db"));
                                //BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#inspectorScoreCount", "" + CurrentScores.Scores.Count());
                                BrowserViewModel.Instance.AttachedJavascriptWrapper.FadeIn("#inspectorScoreCountParent");
                                BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#inspectorScoreCount').countTo({from: 0, to: " + CurrentScores.Scores.Count() + "});");

                                BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#settingsProcessOsuButtonProcessText').html('processing (finishing up)');");
                                int rankedCount = CurrentDatabase.Beatmaps.Count(a => a.RankedStatus == SubmissionStatus.Ranked || a.RankedStatus == SubmissionStatus.Approved);
                                int lovedCount = CurrentDatabase.Beatmaps.Count(a => a.RankedStatus == SubmissionStatus.Loved);
                                int unrankedCount = CurrentDatabase.Beatmaps.Count(a => a.RankedStatus != SubmissionStatus.Ranked && a.RankedStatus != SubmissionStatus.Approved && a.RankedStatus != SubmissionStatus.Loved);

                                BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#inspectorCellRankedMaps", "" + rankedCount);
                                BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#inspectorCellLovedMaps", "" + lovedCount);
                                BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#inspectorCellUnrankedMaps", "" + unrankedCount);

                                BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("updateMapChart(" + rankedCount + "," + lovedCount + "," + unrankedCount + ");");

                                BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#inspectorCellVersion", "" + CurrentDatabase.OsuVersion);
                                BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#inspectorCellUsername", "" + CurrentDatabase.AccountName);
                                BrowserViewModel.Instance.AttachedJavascriptWrapper.FadeIn("#inspectorRegularInfoParent");

                                InspectorBeatmapListing = new InspectorBeatmapListing(BeatmapSets);
                                InspectorBeatmapListing.PreGenerate();

                                //string beatmapDataJson = HttpUtility.JavaScriptStringEncode(JsonConvert.SerializeObject(BeatmapSets));
                                //BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("processBeatmaps('"+ beatmapDataJson + "');");
                                //cefOsuApp.JsExecuter.GetBrowser().ShowDevTools();
                            }
                        }
                        else
                        {
                            BrowserViewModel.Instance.SendNotification(NotificationType.Danger, StringStorage.Get("Message.Osu.NoConfig"), 5000);
                        }
                        BrowserViewModel.Instance.AttachedJavascriptWrapper.SetElementDisabled("#settingsProcessOsu", false);
                        BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#settingsProcessOsu", StringStorage.Get("Message.Osu.Process"));
                        BrowserViewModel.Instance.AttachedJavascriptWrapper.Show("#beatmapProcessTimeString");
                        stopwatch.Stop();
                        TimeSpan ts = stopwatch.Elapsed;
                        BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#beatmapProcessTimeString",string.Format("Process took {0} seconds", ""+Math.Round(ts.TotalSeconds, 2)));

                    });
                });
            });
        }
    }
}   
