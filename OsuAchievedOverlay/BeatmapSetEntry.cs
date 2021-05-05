using osu.Shared;
using osu_database_reader.Components.Beatmaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay
{
    public class BeatmapSetEntry
    {
        public int BeatmapSetID { get; set; }
        public string BeatmapFolder { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Creator { get; set; }
        public SubmissionStatus RankStatus { get; set; }
        public int Difficulties { get; set; }
        public List<BeatmapEntry> Beatmaps { get; set; } = new List<BeatmapEntry>();
    }
}
