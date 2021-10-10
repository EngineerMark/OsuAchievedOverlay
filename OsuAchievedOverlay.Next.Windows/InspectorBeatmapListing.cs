using CefSharp;
using Newtonsoft.Json;
using OsuAchievedOverlay.Next.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OsuAchievedOverlay.Next
{
    public class InspectorBeatmapListing
    {
        private int currentPageIndex = 0;
        private int totalPages = -1;
        private const int setsPerPage = 20;

        private List<BeatmapSetEntry> currentBeatmapSets = new List<BeatmapSetEntry>();
        private string searchQuery = "";

        public InspectorBeatmapListing(List<BeatmapSetEntry> sets)
        {
            currentBeatmapSets = sets;
        }

        public void PreGenerate()
        {
            currentPageIndex = 0;
            totalPages = (int)Math.Ceiling((double)currentBeatmapSets.Count / (double)setsPerPage);

            //BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#beatmapSearchInput').val('');");
            BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#beatmapPaginationGroup').empty();");

            if (totalPages > 0)
            {
                BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("" +
                "for(let i=0;i<" + totalPages + ";i++){" +
                    "$('#beatmapPaginationGroup').append('<li onclick=\"cefOsuApp.beatmapBrowserSetPage('+i+')\" class=\"page-item\"><a class=\"page-link text-white\">'+(i+1)+'</a></li>')" +
                "}");

                BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("var pageItem = $('#beatmapPaginationGroup li');" +
                "pageItem.click(function() {" +
                    "pageItem.removeClass('active');" +
                    "$(this).addClass('active');" +
                "});" +
                "$('#beatmapPaginationGroup li').first().addClass('active');");
            }

            LoadPage(currentPageIndex);
        }

        public void LoadPage(int index)
        {
            BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#beatmapListGroup').empty();");
            if (currentBeatmapSets.Count > 0)
            {
                currentPageIndex = index;
                BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("clearBeatmaps();");

                for (int i = 0; i < setsPerPage; i++)
                {
                    int lookupIndex = i + currentPageIndex*setsPerPage;
                    if (lookupIndex < currentBeatmapSets.Count)
                    {
                        BeatmapSetEntry set = currentBeatmapSets[lookupIndex];
                        BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("addBeatmapset('" + HttpUtility.JavaScriptStringEncode(JsonConvert.SerializeObject(set)) + "', " + i + ");");
                    }
                }
                BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("generateBeatmapsetList();");
            }
            else
            {
                BrowserViewModel.Instance.SendNotification(NotificationType.Warning, StringStorage.Get("Message.NoBeatmaps"));
                BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#beatmapListGroup').html('No results found');");
            }
        }

        public void ApplySearchQuery(string searchQuery)
        {
            this.searchQuery = searchQuery;
            this.searchQuery = this.searchQuery.ToLower();

            string[] querySections = this.searchQuery.Split(' ');
            currentBeatmapSets = new List<BeatmapSetEntry>();

            foreach (BeatmapSetEntry set in InspectorManager.Instance.BeatmapSets)
            {
                foreach (string querySection in querySections)
                {
                    if (set.Title.ToLower().Contains(querySection) || set.Artist.ToLower().Contains(querySection) || set.Creator.ToLower().Contains(querySection) || set.SongTags.ToList().Find(a=>a.ToLower().Contains(querySection)) !=null)
                    {
                        currentBeatmapSets.Add(set);
                    }
                }
            }

            PreGenerate();
        }
    }
}
