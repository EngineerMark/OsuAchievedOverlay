using Newtonsoft.Json;
using System;

namespace OsuAchievedOverlay
{
    public class Session : ICloneable
    {
        public const string CurrentVersion = "Legacy";

        [JsonProperty("StartDataScore")]
        public long StartDataTotalScore { get; set; }

        [JsonProperty("StartDataRankedScore")]
        public long StartDataRankedScore { get; set; }

        [JsonProperty("StartDataPlaycount")]
        public int StartDataPlaycount { get; set; }

        [JsonProperty("StartDataSS")]
        public int StartDataSSCount { get; set; }

        [JsonProperty("StartDataS")]
        public int StartDataSCount { get; set; }

        [JsonProperty("StartDataA")]
        public int StartDataACount { get; set; }

        [JsonProperty("SessionDate")]
        public long SessionDate { get; set; }

        [JsonProperty("StartPlayTime")]
        public long StartPlaytime{ get; set; }

        [JsonProperty("ApplicationVersion")]
        public string Version { get; set; } = CurrentVersion; //Unknown version gets Legacy tag. Does not work with newer version of the app

        public Session(){
            SessionDate = DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        public string ConvertToJson(){
            return JsonConvert.SerializeObject(this);
        }

        public object Clone()
        {
            return new Session()
            {
                StartDataTotalScore = this.StartDataTotalScore,
                StartDataRankedScore = this.StartDataRankedScore,
                StartDataPlaycount = this.StartDataPlaycount,
                StartDataSSCount = this.StartDataSSCount,
                StartDataSCount = this.StartDataSCount,
                StartDataACount = this.StartDataACount,
                SessionDate = this.SessionDate,
                StartPlaytime = this.StartPlaytime
            };
        }
    }
}
