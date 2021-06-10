using Newtonsoft.Json;
using OsuAchievedOverlay.Next.Github;
using OsuAchievedOverlay.Next.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Next.Managers
{
    public class UpdateManager : Singleton<UpdateManager>
    {
        public const string version = "2.0.0";

        public Queue<Release> Updates { get; set; }

        public void Start(){
            Updates = new Queue<Release>();
        }

        public List<Release> GetAvailableUpdates()
        {
            string data = "";
            try
            {
                data = ApiHelper.GetData("https://api.github.com/repos/EngineerMark/OsuAchievedOverlay/releases");
            }
            catch (Exception)
            {
                data = "[]";
            }

            List<Release> releases = JsonConvert.DeserializeObject<List<Release>>(data);

            Version currentVersion = new Version(version);
            releases.RemoveAll(delegate (Release release)
            {
                string _v = release.Version;
                Version v = new Version(_v);
                bool res = currentVersion >= v;
                return res;
            });

            return releases;
        }
    }
}
