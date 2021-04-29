using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Github
{
    public class Asset
    {
        [JsonProperty("url")]
        public string URL { get; set; } = "";

        [JsonProperty("id")]
        public long ID { get; set; } = -1;

        [JsonProperty("node_id")]
        public string NodeID { get; set; } = "";

        [JsonProperty("browser_download_url")]
        public string DownloadLink { get; set; } = "";

        [JsonProperty("size")]
        public long Size { get; set; } = -1;
    }
}
