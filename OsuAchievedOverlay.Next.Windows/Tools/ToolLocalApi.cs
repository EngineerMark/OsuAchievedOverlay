using CefSharp;
using Newtonsoft.Json;
using OsuAchievedOverlay.Next.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OsuAchievedOverlay.Next.Tools
{
    public class ToolLocalApi : Singleton<ToolLocalApi>
    {
        public static string APILocation = Path.Combine(SettingsManager.DefaultDataFolder, "API.json");

        private List<LocalApiFile> apiDataList = new List<LocalApiFile>();

        public List<LocalApiFile> ApiDataList { get => apiDataList; set => apiDataList = value; }

        public void Start(){
            if (File.Exists(APILocation))
            {
                string data = File.ReadAllText(APILocation);
                List<LocalApiFile> tempApiDataList = null;
                try
                {
                    tempApiDataList = JsonConvert.DeserializeObject<List<LocalApiFile>>(data);
                }
                catch (Exception) { }

                if (tempApiDataList != null)
                    apiDataList = new List<LocalApiFile>(tempApiDataList);
            }else{
                apiDataList = new List<LocalApiFile>();
                Save();
            }

            PopulateList();
        }

        public void Stop(){
            Save();
        }

        public void Save(){
            try
            {
                if (File.Exists(APILocation))
                {
                    File.Delete(APILocation);
                }
                FileStream fs = File.Create(APILocation);
                fs.Close();
                File.WriteAllText(APILocation, JsonConvert.SerializeObject(apiDataList));
            }
            catch(Exception){
                BrowserViewModel.Instance.SendNotification(NotificationType.Danger, StringStorage.Get("Message.SomethingWrong"));
            }
        }

        public void PopulateList(){
            if(apiDataList.Count > 0){
                foreach(LocalApiFile apiData in apiDataList){
                    string data = HttpUtility.JavaScriptStringEncode(JsonConvert.SerializeObject(apiData));
                    BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("addApiField(\""+ data + "\", "+apiData.GetHashCode()+");");
                }
            }
        }

        public void ProcessApi(){
            
        }
    }

    public class LocalApiFile : ICloneable
    {
        [JsonProperty("FileName")]
        public string FileName { get; set; } = "";

        [JsonProperty("DisplayName")]
        public string DisplayName { get; set; } = "";

        [JsonProperty("StringData")]
        public string StringData { get; set; } = "";

        [JsonProperty("PositivePrefix")]
        public string PositivePrefix { get; set; } = "+";

        [JsonProperty("NegativePrefix")]
        public string NegativePrefix { get; set; } = "-";

        [JsonProperty("Active")]
        public bool Active { get; set; } = true;

        public object Clone()
        {
            return new LocalApiFile()
            {
                FileName = FileName,
                DisplayName = DisplayName,
                StringData = StringData,
                PositivePrefix = PositivePrefix,
                NegativePrefix = NegativePrefix,
                Active = Active
            };
        }
    }
}
