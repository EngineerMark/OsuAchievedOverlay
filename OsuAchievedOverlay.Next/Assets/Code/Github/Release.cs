using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OsuAchievedOverlay.Github
{
    [System.Serializable]
    public class Release
    {
        [field: SerializeField]
        [JsonProperty("url")]
        public string URL { get; set; } = "";

        [field: SerializeField]
        [JsonProperty("assets_url")]
        public string AssetsURL { get; set; } = "";

        [field: SerializeField]
        [JsonProperty("upload_url")]
        public string UploadURL { get; set; } = "";

        [field: SerializeField]
        [JsonProperty("html_url")]
        public string HTMLURL { get; set; } = "";

        [field: SerializeField]
        [JsonProperty("id")]
        public long ID { get; set; }

        [field: SerializeField]
        [JsonProperty("node_id")]
        public string NodeID { get; set; } = "";

        [field: SerializeField]
        [JsonProperty("target_commitish")]
        public string TargetBranch { get; set; } = "";

        [field: SerializeField]
        [JsonProperty("tag_name")]
        public string Version { get; set; } = "";

        [field: SerializeField]
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [field: SerializeField]
        [JsonProperty("prerelease")]
        public bool IsPrerelease { get; set; }

        [field: SerializeField]
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; } = "";

        [field: SerializeField]
        [JsonProperty("published_at")]
        public string PublishedAt { get; set; } = "";

        [field: SerializeField]
        [JsonProperty("body")]
        public string Body { get; set; } = "";

        [field: SerializeField]
        [JsonProperty("assets")]
        public List<Asset> Assets { get; set; }
    }
}
