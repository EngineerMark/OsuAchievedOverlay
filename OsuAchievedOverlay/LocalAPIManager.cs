using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay
{
    //Takes care of the files containing session data for stream usage or for other software to read
    public class LocalAPIManager : Manager<LocalAPIManager>
    {
        public const string ApiFolder = "api";
        public const string ApiDataFile = "api.json";

        private List<LocalApiFile> apiDataList = new List<LocalApiFile>();

        public List<LocalApiFile> ApiDataList { get => apiDataList; set => apiDataList = value; }

        public override void Start()
        {
            //throw new NotImplementedException();
            if(File.Exists(ApiDataFile)){
                string data = File.ReadAllText(ApiDataFile);
                List<LocalApiFile> tempApiDataList = null;
                try
                {
                    tempApiDataList = JsonConvert.DeserializeObject<List<LocalApiFile>>(data);
                }
                catch (Exception e)
                {

                }

                if (tempApiDataList != null)
                    apiDataList = new List<LocalApiFile>(tempApiDataList);
            }

            foreach (LocalApiFile apiFile in ApiDataList)
            {
                WindowManager.Instance.ApiWin.AddItem(apiFile);
            }
        }

        public override void Stop()
        {
            //throw new NotImplementedException();
        }

        public string Parse(LocalApiFile apiFile, Session session)
        {
            string input = apiFile.StringData;
            input = ParsePartial(apiFile, "{new_ssh}", session.DifferenceData.RankSilverSS);
            input = ParsePartial(apiFile, "{new_ss}", session.DifferenceData.RankGoldSS);
            input = ParsePartial(apiFile, "{new_sh}", session.DifferenceData.RankSilverS);
            input = ParsePartial(apiFile, "{new_s}", session.DifferenceData.RankGoldS);
            input = ParsePartial(apiFile, "{new_a}", session.DifferenceData.RankA);
            input = ParsePartial(apiFile, "{new_totalscore}", session.DifferenceData.TotalScore);
            input = ParsePartial(apiFile, "{new_rankedscore}", session.DifferenceData.RankedScore);
            input = ParsePartial(apiFile, "{new_playtime}", session.DifferenceData.Playtime);
            input = ParsePartial(apiFile, "{new_pc}", session.DifferenceData.Playcount);

            input = ParsePartial(apiFile, "{current_ssh}", session.CurrentData.RankSilverSS);
            input = ParsePartial(apiFile, "{current_ss}", session.CurrentData.RankGoldSS);
            input = ParsePartial(apiFile, "{current_sh}", session.CurrentData.RankSilverS);
            input = ParsePartial(apiFile, "{current_s}", session.CurrentData.RankGoldS);
            input = ParsePartial(apiFile, "{current_a}", session.CurrentData.RankA);
            input = ParsePartial(apiFile, "{current_totalscore}", session.CurrentData.TotalScore);
            input = ParsePartial(apiFile, "{current_rankedscore}", session.CurrentData.RankedScore);
            input = ParsePartial(apiFile, "{current_playtime}", session.CurrentData.Playtime);
            input = ParsePartial(apiFile, "{current_pc}", session.CurrentData.Playcount);

            input = ParsePartial(apiFile, "{initial_ssh}", session.InitialData.RankSilverSS);
            input = ParsePartial(apiFile, "{initial_ss}", session.InitialData.RankGoldSS);
            input = ParsePartial(apiFile, "{initial_sh}", session.InitialData.RankSilverS);
            input = ParsePartial(apiFile, "{initial_s}", session.InitialData.RankGoldS);
            input = ParsePartial(apiFile, "{initial_a}", session.InitialData.RankA);
            input = ParsePartial(apiFile, "{initial_totalscore}", session.InitialData.TotalScore);
            input = ParsePartial(apiFile, "{initial_rankedscore}", session.InitialData.RankedScore);
            input = ParsePartial(apiFile, "{initial_playtime}", session.InitialData.Playtime);
            input = ParsePartial(apiFile, "{initial_pc}", session.InitialData.Playcount);

            return input;
        }

        private string ParsePartial(LocalApiFile apiFile, string check, long data){
            string input = apiFile.StringData;
            if (input.Contains(check))
                input = input.Replace(check, GetPrefix(data, apiFile) +Math.Abs(data));
            return input;
        }

        private string GetPrefix(long val, LocalApiFile apiFile)
        {
            return val >= 0 ? apiFile.PositivePrefix : apiFile.NegativePrefix;
        }
    }
}
