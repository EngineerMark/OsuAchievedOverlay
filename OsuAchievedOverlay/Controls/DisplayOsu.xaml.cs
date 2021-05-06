using osu.Shared;
using osu_database_reader.BinaryFiles;
using osu_database_reader.Components.Beatmaps;
using OsuAchievedOverlay.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace OsuAchievedOverlay.Controls
{
    /// <summary>
    /// Interaction logic for DisplayOsu.xaml
    /// </summary>
    public partial class DisplayOsu : UserControl
    {
        private bool IsUpdating { get; set; }

        private OsuDb CurrentDatabase { get; set; }
        private CollectionDb CurrentCollections { get; set; }
        private ScoresDb CurrentScores { get; set; }
        private List<BeatmapSetEntry> BeatmapSets { get; set; }
        private List<BeatmapSetEntry> ResultBeatmapSets { get; set; }

        private int CurrentPage { get; set; }
        private int MaxPage { get; set; }
        private string SearchQuery { get; set; }

        private const int resultsPerPage = 10;

        public DisplayOsu()
        {
            InitializeComponent();

            Btn_HideOverlay(null, null);
        }

        public void UpdateData()
        {
            if (!ApiHelper.IsValidOsuInstallation(SettingsManager.Instance.Settings["misc"]["osuFolder"]))
            {
                MessageBox.Show("Current set osu! folder is invalid", "Invalid osu! directory");
                return;
            }
            if (!IsUpdating)
            {
                CurrentPage = 0;
                SearchQuery = string.Empty;
                EntryVersion.ItemLoader.Visibility = Visibility.Visible;
                EntryVersion.ItemValue.Visibility = Visibility.Hidden;
                EntryBeatmapCount.ItemLoader.Visibility = Visibility.Visible;
                EntryBeatmapCount.ItemValue.Visibility = Visibility.Hidden;
                EntryCollectionCount.ItemLoader.Visibility = Visibility.Visible;
                EntryCollectionCount.ItemValue.Visibility = Visibility.Hidden;
                EntryBeatmapSetCount.ItemLoader.Visibility = Visibility.Visible;
                EntryBeatmapSetCount.ItemValue.Visibility = Visibility.Hidden;
                EntryReplayCount.ItemLoader.Visibility = Visibility.Visible;
                EntryReplayCount.ItemValue.Visibility = Visibility.Hidden;
                ButtonShowScores.IsEnabled = false;
                ButtonShowBeatmaps.IsEnabled = false;
                ButtonShowCollections.IsEnabled = false;
                ListBeatmapItems.Children.Clear();
                ThreadPool.QueueUserWorkItem((Object stateInfo) =>
                {
                    IsUpdating = true;

                    if (CurrentDatabase != null)
                    {
                        CurrentDatabase.Beatmaps.Clear();
                    }

                    //CurrentOsuDb = OsuDb.ProcessDatabase(System.IO.Path.Combine(SettingsManager.Instance.Settings["misc"]["osuFolder"], "osu!.db"));

                    CurrentDatabase = OsuDb.Read(System.IO.Path.Combine(SettingsManager.Instance.Settings["misc"]["osuFolder"], "osu!.db"));

                    BeatmapSets?.Clear();
                    BeatmapSets = new List<BeatmapSetEntry>();

                    foreach (BeatmapEntry difficulty in CurrentDatabase.Beatmaps)
                    {
                        BeatmapSetEntry set = BeatmapSets.Find(a => a.BeatmapSetID == difficulty.BeatmapSetId);
                        if (set != null)
                        {
                            set.Difficulties++;
                            string[] tags = difficulty.SongTags.Split(' ');
                            if(tags.Length>0)
                                foreach(string tag in tags)
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
                            string[] tags = difficulty.SongTags.Split(' ');
                            if (tags.Length > 0)
                                foreach (string tag in tags)
                                    set.SongTags.Add(tag);
                            BeatmapSets.Add(set);
                        }
                    }

                    ResultBeatmapSets = new List<BeatmapSetEntry>(BeatmapSets);

                    Dispatcher.Invoke(() =>
                    {
                        UpdateEntry(EntryVersion, "" + CurrentDatabase.OsuVersion);
                        UpdateEntry(EntryBeatmapCount, "" + CurrentDatabase.Beatmaps.Count.ToString("#,##0.###"));
                        UpdateEntry(EntryBeatmapSetCount, "" + BeatmapSets.Count.ToString("#,##0.###"));

                        //SetBeatmapResults(CurrentPage);

                        ButtonShowScores.IsEnabled = true;
                        ButtonShowBeatmaps.IsEnabled = true;
                        ButtonShowCollections.IsEnabled = true;

                        ProcessSearchData(SearchQuery);
                    });

                    CurrentCollections = CollectionDb.Read(Path.Combine(SettingsManager.Instance.Settings["misc"]["osuFolder"], "collection.db"));

                    Dispatcher.Invoke(() =>
                    {
                        UpdateEntry(EntryCollectionCount, "" + CurrentCollections.Collections.Count.ToString("#,##0.###"));
                    });

                    CurrentScores = ScoresDb.Read(Path.Combine(SettingsManager.Instance.Settings["misc"]["osuFolder"], "scores.db"));

                    Dispatcher.Invoke(() =>
                    {
                        UpdateEntry(EntryReplayCount, "" + CurrentScores.Scores.Count().ToString("#,##0.###"));
                    });

                    IsUpdating = false;
                });
            }
        }

        private void ProcessSearchData(string query){
            if (string.IsNullOrEmpty(query))
            {
                ResultBeatmapSets = new List<BeatmapSetEntry>(BeatmapSets);
            }
            else
            {
                string[] splitQuery = query.ToLower().Split(' ');
                List<BeatmapSetEntry> result = new List<BeatmapSetEntry>();
                foreach (BeatmapSetEntry item in BeatmapSets)
                {
                    bool add = true;
                    foreach (string queryItem in splitQuery)
                    {
                        if (!item.Title.ToLower().Contains(queryItem) &&
                            !item.Artist.ToLower().Contains(queryItem) &&
                            !item.Creator.ToLower().Contains(queryItem))
                        {
                            add = false;
                            break;
                        }
                    }

                    if (add)
                        result.Add(item);
                }

                ResultBeatmapSets = result;
            }

            MaxPage = (int)Math.Ceiling((double)ResultBeatmapSets.Count / resultsPerPage) - 1;
            CurrentPage = 0;
            LabelCurrentPage.Content = CurrentPage + 1;
            SetBeatmapResults(CurrentPage);
        }

        private void SetBeatmapResults(int page)
        {
            ListBeatmapItems.Children.Clear();
            ThreadPool.QueueUserWorkItem((Object stateInfo) =>
            {
                if (ResultBeatmapSets.Count > 0)
                {
                    for (int i = 0; i < resultsPerPage; i++)
                    {
                        int actualIndex = i + (page * resultsPerPage);

                        if (actualIndex >= ResultBeatmapSets.Count)
                            break;

                        BeatmapSetEntry map = ResultBeatmapSets[actualIndex];

                        string path = Path.Combine(SettingsManager.Instance.Settings["misc"]["osuFolder"], "Songs", map.BeatmapFolder);
                        string backgroundImage = Directory.GetFiles(path).FirstOrDefault(file =>
                        {
                            return Path.GetExtension(file) == ".png" ||
                                Path.GetExtension(file) == ".jpg" ||
                                Path.GetExtension(file) == ".jpeg";
                        });

                        BitmapImage image = null;
                        if (backgroundImage != string.Empty && File.Exists(backgroundImage))
                        {
                            try
                            {
                                image = new BitmapImage(new Uri(backgroundImage));
                                if (image != null)
                                    image.Freeze();
                            }
                            catch (Exception) { image = null; }
                        }

                        Dispatcher.Invoke(() =>
                        {
                            BeatmapItem item = new BeatmapItem();
                            item.LabelTitle.Content = map.Title;
                            item.LabelName.Content = map.Artist;
                            item.LabelMapper.Content = map.Creator;

                            switch (map.RankStatus)
                            {
                                case SubmissionStatus.Ranked:
                                    item.MapStateRanked.Visibility = Visibility.Visible;
                                    break;
                                case SubmissionStatus.Loved:
                                    item.MapStateLoved.Visibility = Visibility.Visible;
                                    break;
                                case SubmissionStatus.Unknown:
                                case SubmissionStatus.Pending:
                                case SubmissionStatus.EditableCutoff:
                                    item.MapStateUnranked.Visibility = Visibility.Visible;
                                    break;
                                case SubmissionStatus.NotSubmitted:
                                    item.MapStateUnsubmitted.Visibility = Visibility.Visible;
                                    break;
                                case SubmissionStatus.Qualified:
                                    item.MapStateQualified.Visibility = Visibility.Visible;
                                    break;
                                case SubmissionStatus.Approved:
                                    item.MapStateApproved.Visibility = Visibility.Visible;
                                    break;
                            }

                            if (image != null)
                            {
                                item.ImageMainIcon.Source = image;
                                item.ImageBackground.Source = item.ImageMainIcon.Source;
                            }
                            ListBeatmapItems.Children.Add(item);
                        });
                    }
                }
            });
        }

        private void UpdateEntry(OsuDataEntry entry, string value)
        {
            entry.ItemLoader.Visibility = Visibility.Hidden;
            entry.ItemValue.Visibility = Visibility.Visible;
            entry.Value = "" + value;
        }

        private void Btn_RefreshData(object sender, RoutedEventArgs e)
        {
            UpdateData();
        }

        private void Btn_ShowScores(object sender, RoutedEventArgs e)
        {
            if (GridViewScores.Visibility != Visibility.Visible)
                GridViewScores.Visibility = Visibility.Visible;
            GameManager.Instance.SetPageButtonState(false);
        }

        private void Btn_ShowMaps(object sender, RoutedEventArgs e)
        {
            if (GridViewBeatmaps.Visibility != Visibility.Visible)
                GridViewBeatmaps.Visibility = Visibility.Visible;
            GameManager.Instance.SetPageButtonState(false);
        }

        private void Btn_ShowCollections(object sender, RoutedEventArgs e)
        {
            if (GridViewCollections.Visibility != Visibility.Visible)
                GridViewCollections.Visibility = Visibility.Visible;
            GameManager.Instance.SetPageButtonState(false);
        }

        private void Btn_HideOverlay(object sender, RoutedEventArgs e)
        {
            GridViewScores.Visibility = Visibility.Hidden;
            GridViewBeatmaps.Visibility = Visibility.Hidden;
            GridViewCollections.Visibility = Visibility.Hidden;

            GameManager.Instance.SetPageButtonState(true);
        }

        private void Btn_PageButton(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            int initialPage = CurrentPage;
            string t = (string)btn.Tag;
            switch (t)
            {
                case "Forward":
                    CurrentPage++;
                    break;
                case "ForwardFull":
                    CurrentPage = MaxPage;
                    break;
                case "Back":
                    CurrentPage--;
                    break;
                case "BackFull":
                    CurrentPage = 0;
                    break;
            }

            if (CurrentPage < 0)
                CurrentPage = 0;
            if (CurrentPage > MaxPage)
                CurrentPage = MaxPage;

            if(initialPage!=CurrentPage){
                LabelCurrentPage.Content = CurrentPage + 1;
                SetBeatmapResults(CurrentPage);
            }
        }

        private void Btn_Search(object sender, RoutedEventArgs e)
        {
            //string title = (string)BeatmapSetEntry.TitleProperty.GetValue(BeatmapSets[0]);
            //string t = "";

            ProcessSearchData(InputSearchQuery.Text);
        }
    }
}
