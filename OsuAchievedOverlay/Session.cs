using Newtonsoft.Json;
using System;

namespace OsuAchievedOverlay
{
    public class Session : ICloneable
    {
        public const string CurrentVersion = "APIv1";

        [JsonProperty("SessionDataInitial")]
        public SessionData InitialData { get; set; } = null;

        [JsonProperty("SessionDataDifference")]
        public SessionData DifferenceData { get; set; } = null;

        [JsonProperty("SessionDataCurrent")]
        public SessionData CurrentData { get; set; } = null;

        [JsonProperty("SessionDate")]
        public long SessionDate { get; set; }

        [JsonProperty("ApplicationVersion")]
        public string Version { get; set; } = CurrentVersion;

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
                InitialData = (SessionData)InitialData.Clone(),
                DifferenceData = (SessionData)DifferenceData.Clone(),
                CurrentData = (SessionData)CurrentData.Clone(),
                SessionDate = SessionDate
            };
        }
    }

    public class SessionData : ICloneable
    {
        [JsonProperty("DataTotalScore")] 
        public long TotalScore;
        [JsonProperty("DataRankedScore")]
        public long RankedScore;
        [JsonProperty("DataPlaycount")]
        public int Playcount;
        [JsonProperty("DataRankSSH")]
        public int RankSilverSS;
        [JsonProperty("DataRankSS")]
        public int RankGoldSS;
        [JsonProperty("DataRankSH")]
        public int RankSilverS;
        [JsonProperty("DataRankS")]
        public int RankGoldS;
        [JsonProperty("DataRankA")]
        public int RankA;
        [JsonProperty("DataPlaytime")]
        public long Playtime;

        public static void FromUser(OsuApiHelper.OsuUser user, ref SessionData output){
            output = FromUser(user);
        }

        public static SessionData FromUser(OsuApiHelper.OsuUser user){
            return new SessionData()
            {
                TotalScore = Convert.ToInt64(user.TotalScore),
                RankedScore = Convert.ToInt64(user.RankedScore),
                Playcount = user.Playcount,
                RankSilverSS = user.CountRankSSH,
                RankGoldSS = user.CountRankSS,
                RankSilverS = user.CountRankSH,
                RankGoldS = user.CountRankS,
                RankA = user.CountRankA,
                Playtime = user.Playtime
            };
        }

        public static SessionData CalculateDifference(SessionData a, SessionData b){
            SessionData output = new SessionData
            {
                TotalScore = a.TotalScore - b.TotalScore,
                RankedScore = a.RankedScore - b.RankedScore,
                Playcount = a.Playcount - b.Playcount,
                RankSilverSS = a.RankSilverSS - b.RankSilverSS,
                RankGoldSS = a.RankGoldSS - b.RankGoldSS,
                RankSilverS = a.RankSilverS - b.RankSilverS,
                RankGoldS = a.RankGoldS - b.RankGoldS,
                RankA = a.RankA - b.RankA,
                Playtime = a.Playtime - b.Playtime
            };
            return output;
        }

        public object Clone(){
            return new SessionData()
            {
                TotalScore = TotalScore,
                RankedScore = RankedScore,
                RankSilverSS = RankSilverSS,
                RankGoldSS = RankGoldSS,
                RankSilverS = RankSilverS,
                RankGoldS = RankGoldS,
                RankA = RankA,
                Playtime = Playtime,
                Playcount = Playcount,
            };
        }
    }
}
