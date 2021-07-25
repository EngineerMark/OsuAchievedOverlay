using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Next.OsuWeb
{
    public struct WebUser
    {
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("last_visit")]
        public string LastOnline { get; set; }

        [JsonProperty("cover_url")]
        public string CoverURL { get; set; }

        [JsonProperty("interests")]
        public string Interests { get; set; }

        [JsonProperty("join_date")]
        public string JoinDate { get; set; }
    }
}
