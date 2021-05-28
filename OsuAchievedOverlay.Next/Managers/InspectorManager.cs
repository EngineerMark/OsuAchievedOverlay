using CefSharp;
using osu.Shared;
using osu_database_reader.BinaryFiles;
using osu_database_reader.Components.Beatmaps;
using OsuAchievedOverlay.Next.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace OsuAchievedOverlay.Next.Managers
{
    public class InspectorManager : Singleton<InspectorManager>
    {
        public bool IsUpdating { get; set; }
        public OsuDb CurrentDatabase { get; set; }
        public CollectionDb CurrentCollections { get; set; }
        public ScoresDb CurrentScores { get; set; }
        public List<BeatmapSetEntry> BeatmapSets { get; set; }
        public string SongsPath { get; set; }

        public void ProcessOsu(Dispatcher dispatcher)
        {
            dispatcher.Invoke(() =>
            {
                ThreadPool.QueueUserWorkItem((object stateInfo) =>
                {
                    BrowserViewModel.Instance.AttachedJavascriptWrapper.SetElementDisabled("#settingsProcessOsu", true);
                    BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#settingsProcessOsu", "<span class=\"spinner-border spinner-border-sm\" role=\"status\" aria-hidden=\"true\"></span> processing");
                    if (!ApiHelper.IsValidOsuInstallation(SettingsManager.Instance.Settings["misc"]["osuFolder"]))
                    {
                        BrowserViewModel.Instance.SendNotification(NotificationType.Danger, "There is no or invalid osu folder selected");
                    }
                    else
                    {
                        IsUpdating = true;

                        if (CurrentDatabase != null)
                            CurrentDatabase.Beatmaps.Clear();

                        string configPath = Path.Combine(SettingsManager.Instance.Settings["misc"]["osuFolder"], "osu!." + Environment.UserName + ".cfg");
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
                            BrowserViewModel.Instance.SendNotification(NotificationType.Danger, "osu! directory is correct, but no configuration was found");
                            validConfig = false;
                        }

                        if (validConfig)
                        {
                            CurrentDatabase = OsuDb.Read(System.IO.Path.Combine(SettingsManager.Instance.Settings["misc"]["osuFolder"], "osu!.db"));
                            //BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#inspectorBeatmapDiffCount", ""+CurrentDatabase.Beatmaps.Count);
                            BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#inspectorBeatmapDiffCount').countTo({from: 0, to: " + CurrentDatabase.Beatmaps.Count + "});");

                            BeatmapSets?.Clear();
                            BeatmapSets = new List<BeatmapSetEntry>();

                            foreach (BeatmapEntry difficulty in CurrentDatabase.Beatmaps)
                            {
                                BeatmapSetEntry set = BeatmapSets.Find(a => a.BeatmapSetID == difficulty.BeatmapSetId);
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
                                    set.BeatmapFolder = difficulty.FolderName;
                                    set.RankStatus = difficulty.RankedStatus;
                                    set.Beatmaps.Add(difficulty);
                                    set.Difficulties = 1;
                                    if (Directory.Exists(Path.Combine(SongsPath, set.BeatmapFolder)))
                                    {
                                        List<string> images = ApiHelper.GetImages(Path.Combine(SongsPath, set.BeatmapFolder));
                                        images.Sort((a, b) => new FileInfo(a).Length.CompareTo(new FileInfo(b).Length));
                                        images.Reverse();
                                        string backgroundImage = images.FirstOrDefault();

                                        set.SongSource = difficulty.SongSource;
                                        set.AudioFileName = difficulty.AudioFileName;

                                        set.BackgroundPath = backgroundImage;
                                        string[] tags = difficulty.SongTags.Split(' ');
                                        if (tags.Length > 0)
                                            foreach (string tag in tags)
                                                set.SongTags.Add(tag);
                                        BeatmapSets.Add(set);
                                    }
                                }
                            }
                            //BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#inspectorBeatmapSetCount", "" + BeatmapSets.Count);
                            BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#inspectorBeatmapSetCount').countTo({from: 0, to: " + BeatmapSets.Count + "});");

                            CurrentCollections = CollectionDb.Read(Path.Combine(SettingsManager.Instance.Settings["misc"]["osuFolder"], "collection.db"));
                            //BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#inspectorCollectionCount", "" + CurrentCollections.Collections.Count);
                            BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#inspectorCollectionCount').countTo({from: 0, to: " + CurrentCollections.Collections.Count + "});");

                            BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#inspectorCollectionList').html('')");
                            foreach (Collection coll in CurrentCollections.Collections)
                            {
                                BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#inspectorCollectionList').append('<button type=\\\"button\\\" class=\\\"btn btn-sm btn-block btn-osu-dark\\\">" + coll.Name + "</button>')");
                            }

                            CurrentScores = ScoresDb.Read(Path.Combine(SettingsManager.Instance.Settings["misc"]["osuFolder"], "scores.db"));
                            //BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#inspectorScoreCount", "" + CurrentScores.Scores.Count());
                            BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#inspectorScoreCount').countTo({from: 0, to: " + CurrentScores.Scores.Count() + "});");

                            int rankedCount = CurrentDatabase.Beatmaps.Count(a => a.RankedStatus == SubmissionStatus.Ranked || a.RankedStatus == SubmissionStatus.Approved);
                            int lovedCount = CurrentDatabase.Beatmaps.Count(a => a.RankedStatus == SubmissionStatus.Loved);
                            int unrankedCount = CurrentDatabase.Beatmaps.Count(a => a.RankedStatus != SubmissionStatus.Ranked && a.RankedStatus != SubmissionStatus.Approved && a.RankedStatus != SubmissionStatus.Loved);

                            BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#inspectorCellRankedMaps", "" + rankedCount);
                            BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#inspectorCellLovedMaps", "" + lovedCount);
                            BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#inspectorCellUnrankedMaps", "" + unrankedCount);

                            BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("updateMapChart(" + rankedCount + "," + lovedCount + "," + unrankedCount + ");");

                            BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#inspectorCellVersion", "" + CurrentDatabase.OsuVersion);
                            BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#inspectorCellUsername", "" + CurrentDatabase.AccountName);
                        }
                    }
                    BrowserViewModel.Instance.AttachedJavascriptWrapper.SetElementDisabled("#settingsProcessOsu", false);
                    BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#settingsProcessOsu", "Process osu");
                });
            });
        }
    }
}
