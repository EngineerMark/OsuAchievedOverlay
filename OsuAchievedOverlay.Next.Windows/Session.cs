using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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

        [JsonProperty("SessionEndDate")]
        public long SessionEndDate { get; set; } = -1;

        [JsonProperty("ApplicationVersion")]
        public string Version { get; set; } = CurrentVersion;

        [JsonProperty("GainedTopPlays")]
        public List<OsuApiHelper.OsuPlay> GainedPlays { get; set; } = new List<OsuApiHelper.OsuPlay>();

        [JsonProperty("ReadOnly")]
        public bool ReadOnly { get; set; } = false;

        [JsonProperty("User")]
        public string Username { get; set; } = null;

        public Session()
        {
            SessionDate = DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        //public void AddNewPlays(List<OsuApiHelper.OsuPlay> plays)
        //{
            // For the future, once we secured a safe way to get Performance data without basically killing the program for data retrieval.
            //foreach (OsuApiHelper.OsuPlay play in plays)
            //{
            //    if (!play.Rank.Equals("F", StringComparison.InvariantCultureIgnoreCase) && GainedPlays.Find(a => a.DateAchieved == play.DateAchieved) == null)
            //    {
            //        GainedPlays.Add(play);
            //    }
            //}
        //}

        public string ConvertToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public object Clone()
        {
            return new Session()
            {
                InitialData = (SessionData)InitialData?.Clone(),
                DifferenceData = (SessionData)DifferenceData?.Clone(),
                CurrentData = (SessionData)CurrentData?.Clone(),
                SessionDate = SessionDate,
                Username = Username,
                GainedPlays = new List<OsuApiHelper.OsuPlay>(GainedPlays)
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
        [JsonProperty("DataPerformance")]
        public float Performance;
        [JsonProperty("DataLevel")]
        public float Level;
        [JsonProperty("DataPPRank")]
        public float WorldRank;
        [JsonProperty("DataCountryRank")]
        public float CountryRank;
        [JsonProperty("DataAccuracy")]
        public float Accuracy;
        [JsonProperty("Data300x")]
        public float Hits300;
        [JsonProperty("Data100x")]
        public float Hits100;
        [JsonProperty("Data50x")]
        public float Hits50;
        [JsonProperty("DataHitsPerPlay")]
        public float HitsPerPlay;

        public static void FromUser(OsuApiHelper.OsuUser user, ref SessionData output)
        {
            output = FromUser(user);
        }

        public static SessionData FromUser(OsuApiHelper.OsuUser user)
        {
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
                Playtime = user.Playtime,
                Performance = user.Performance,
                Level = user.Level,
                WorldRank = user.Globalrank,
                CountryRank = user.Countryrank,
                Accuracy = user.Accuracy,
                Hits300 = user.Hits300,
                Hits100 = user.Hits100,
                Hits50 = user.Hits50,
                HitsPerPlay = (user.Hits300 + user.Hits100 + user.Hits50) / user.Playcount,
            };
        }

        public static SessionData operator -(SessionData a, SessionData b)
        {
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
                Playtime = a.Playtime - b.Playtime,
                Performance = a.Performance - b.Performance,
                Level = a.Level - b.Level,
                WorldRank = a.WorldRank-b.WorldRank,
                CountryRank = a.CountryRank-b.CountryRank,
                Accuracy = a.Accuracy - b.Accuracy,
                Hits300 = a.Hits300 - b.Hits300,
                Hits100 = a.Hits100 - b.Hits100,
                Hits50 = a.Hits50 - b.Hits50,
                HitsPerPlay = a.HitsPerPlay-b.HitsPerPlay,
            };
            return output;
        }

        public static SessionData operator +(SessionData a, SessionData b)
        {
            SessionData output = new SessionData
            {
                TotalScore = a.TotalScore + b.TotalScore,
                RankedScore = a.RankedScore + b.RankedScore,
                Playcount = a.Playcount + b.Playcount,
                RankSilverSS = a.RankSilverSS + b.RankSilverSS,
                RankGoldSS = a.RankGoldSS + b.RankGoldSS,
                RankSilverS = a.RankSilverS + b.RankSilverS,
                RankGoldS = a.RankGoldS + b.RankGoldS,
                RankA = a.RankA + b.RankA,
                Playtime = a.Playtime + b.Playtime,
                Performance = a.Performance + b.Performance,
                Level = a.Level + b.Level,
                WorldRank = a.WorldRank + b.WorldRank,
                CountryRank = a.CountryRank + b.CountryRank,
                Accuracy = a.Accuracy + b.Accuracy,
                Hits300 = a.Hits300 + b.Hits300,
                Hits100 = a.Hits100 + b.Hits100,
                Hits50 = a.Hits50 + b.Hits50,
                HitsPerPlay = a.HitsPerPlay - b.HitsPerPlay
            };
            return output;
        }

        public static SessionData CalculateDifference(SessionData a, SessionData b)
        {
            return a - b;
        }

        public object Clone()
        {
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
                Performance = Performance,
                Level = Level,
                WorldRank = WorldRank,
                CountryRank = CountryRank,
                Accuracy = Accuracy,
                Hits300 = Hits300,
                Hits100 = Hits100,
                Hits50 = Hits50,
                HitsPerPlay = HitsPerPlay
            };
        }
    }
}
