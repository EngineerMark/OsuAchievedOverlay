using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay
{
    public class Session : ICloneable
    {
        [JsonProperty("StartDataScore")]
        public long StartDataTotalScore { get; set; }

        [JsonProperty("StartDataScore")]
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
                SessionDate = this.SessionDate
            };
        }
    }
}
