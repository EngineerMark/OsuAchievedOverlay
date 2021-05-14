using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Helpers
{
    public static class ApiHelper
    {
        public static bool IsValidOsuInstallation(string path)
        {
            return (File.Exists(Path.Combine(path, "osu!.exe")) &&
                File.Exists(Path.Combine(path, "osu!.db")) &&
                File.Exists(Path.Combine(path, "scores.db")) &&
                Directory.Exists(Path.Combine(path, "Songs")));
        }

        public static bool IsKeyValid(string key)
        {
            string testURL = "https://osu.ppy.sh/api/get_user?k=" + key + "&u=peppy";
            HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(testURL);
            try
            {
                HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception){
                return false;
            }
        }

        public static string GetData(string url)
        {
            using (WebClient client = new WebClient())
            {
                client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0) (compatible; MSIE 6.0; Windows NT 5.1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)");
                string s = client.DownloadString(url);
                return s;
            }
        }

        public static string GetOsuUserHeaderUrl(string profileurl)
        {
            string data = GetData(profileurl);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(data);

            string jsonData = htmlDoc.DocumentNode.Descendants("script").FirstOrDefault(a => a.Id == "json-user").InnerText;

            ApiOsuProfile cv = JsonConvert.DeserializeObject<ApiOsuProfile>(jsonData);

            return cv.CoverURL;
        }
    }

    public struct ApiOsuProfile
    {
        [JsonProperty("cover_url")]
        public string CoverURL { get; set; }
    }
}
