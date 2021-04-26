using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay
{
    public static class ApiHelper
    {
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
