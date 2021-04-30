using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Managers
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
                catch (Exception) { }

                if (tempApiDataList != null)
                    apiDataList = new List<LocalApiFile>(tempApiDataList);
            }
        }

        public void LoadItems(){
            foreach (LocalApiFile apiFile in ApiDataList)
            {
                WindowManager.Instance.ApiWin.AddItem(apiFile);
            }
        }

        public override void Stop()
        {
            //throw new NotImplementedException();
        }

        public void SaveData(LocalApiFile apiFile){
            Directory.CreateDirectory(ApiFolder);
            string data = Parse(apiFile, SessionManager.Instance.CurrentSession);
            string file = Path.Combine(ApiFolder, apiFile.FileName);
            //File.WriteAllText(file, data);
            FileManager.Instance.WriteAllText(file, data);
        }

        public string Parse(LocalApiFile apiFile, Session session)
        {
            if (session == null || session.InitialData == null || session.DifferenceData == null || session.CurrentData == null)
                return "";

            LocalApiFile input = (LocalApiFile)apiFile.Clone();
            input.StringData = ParsePartial(input, "{new_ssh}", session.DifferenceData.RankSilverSS);
            input.StringData = ParsePartial(input, "{new_ss}", session.DifferenceData.RankGoldSS);
            input.StringData = ParsePartial(input, "{new_total_ss}", session.DifferenceData.RankSilverSS+session.DifferenceData.RankGoldSS);
            input.StringData = ParsePartial(input, "{new_sh}", session.DifferenceData.RankSilverS);
            input.StringData = ParsePartial(input, "{new_s}", session.DifferenceData.RankGoldS);
            input.StringData = ParsePartial(input, "{new_total_s}", session.DifferenceData.RankSilverS+session.DifferenceData.RankGoldS);
            input.StringData = ParsePartial(input, "{new_a}", session.DifferenceData.RankA);
            input.StringData = ParsePartial(input, "{new_totalscore}", session.DifferenceData.TotalScore);
            input.StringData = ParsePartial(input, "{new_rankedscore}", session.DifferenceData.RankedScore);
            input.StringData = ParsePartial(input, "{new_playtime}", session.DifferenceData.Playtime);
            input.StringData = ParsePartial(input, "{new_pc}", session.DifferenceData.Playcount);

            input.StringData = ParsePartial(input, "{current_ssh}", session.CurrentData.RankSilverSS);
            input.StringData = ParsePartial(input, "{current_ss}", session.CurrentData.RankGoldSS);
            input.StringData = ParsePartial(input, "{current_total_ss}", session.CurrentData.RankSilverSS+session.CurrentData.RankGoldSS);
            input.StringData = ParsePartial(input, "{current_sh}", session.CurrentData.RankSilverS);
            input.StringData = ParsePartial(input, "{current_s}", session.CurrentData.RankGoldS);
            input.StringData = ParsePartial(input, "{current_total_s}", session.CurrentData.RankSilverS+session.CurrentData.RankGoldS);
            input.StringData = ParsePartial(input, "{current_a}", session.CurrentData.RankA);
            input.StringData = ParsePartial(input, "{current_totalscore}", session.CurrentData.TotalScore);
            input.StringData = ParsePartial(input, "{current_rankedscore}", session.CurrentData.RankedScore);
            input.StringData = ParsePartial(input, "{current_playtime}", session.CurrentData.Playtime);
            input.StringData = ParsePartial(input, "{current_pc}", session.CurrentData.Playcount);

            input.StringData = ParsePartial(input, "{initial_ssh}", session.InitialData.RankSilverSS);
            input.StringData = ParsePartial(input, "{initial_ss}", session.InitialData.RankGoldSS);
            input.StringData = ParsePartial(input, "{initial_total_ss}", session.InitialData.RankSilverSS+session.InitialData.RankGoldSS);
            input.StringData = ParsePartial(input, "{initial_sh}", session.InitialData.RankSilverS);
            input.StringData = ParsePartial(input, "{initial_s}", session.InitialData.RankGoldS);
            input.StringData = ParsePartial(input, "{initial_total_s}", session.InitialData.RankSilverS+session.InitialData.RankGoldS);
            input.StringData = ParsePartial(input, "{initial_a}", session.InitialData.RankA);
            input.StringData = ParsePartial(input, "{initial_totalscore}", session.InitialData.TotalScore);
            input.StringData = ParsePartial(input, "{initial_rankedscore}", session.InitialData.RankedScore);
            input.StringData = ParsePartial(input, "{initial_playtime}", session.InitialData.Playtime);
            input.StringData = ParsePartial(input, "{initial_pc}", session.InitialData.Playcount);

            return input.StringData;
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
