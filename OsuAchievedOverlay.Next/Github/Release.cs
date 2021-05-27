using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Next.Github
{
    public class Release
    {
        [JsonProperty("url")]
        public string URL { get; set; } = "";

        [JsonProperty("assets_url")]
        public string AssetsURL { get; set; } = "";

        [JsonProperty("upload_url")]
        public string UploadURL { get; set; } = "";

        [JsonProperty("html_url")]
        public string HTMLURL { get; set; } = "";

        [JsonProperty("id")]
        public long ID { get; set; }

        [JsonProperty("node_id")]
        public string NodeID { get; set; } = "";

        [JsonProperty("target_commitish")]
        public string TargetBranch { get; set; } = "";

        [JsonProperty("tag_name")]
        public string Version { get; set; } = "";

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("prerelease")]
        public bool IsPrerelease { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; } = "";

        [JsonProperty("published_at")]
        public string PublishedAt { get; set; } = "";

        [JsonProperty("body")]
        public string Body { get; set; } = "";

        [JsonProperty("assets")]
        public List<Asset> Assets { get; set; }
    }
}
