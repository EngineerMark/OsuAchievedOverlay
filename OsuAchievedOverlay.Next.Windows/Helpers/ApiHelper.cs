using HtmlAgilityPack;
using Newtonsoft.Json;
using osu_database_reader.Components.Beatmaps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Next.Helpers
{
    public static class ApiHelper
    {
        public static bool IsKeyValid(string key)
        {
            string testURL = "https://osu.ppy.sh/api/get_user?k=" + key + "&u=peppy";
            HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(testURL);
            try
            {
                HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static List<string> GetImages(string path){
            List<string> images = Directory.GetFiles(path).Where(file =>
            {
                return Path.GetExtension(file) == ".png" ||
                    Path.GetExtension(file) == ".jpg" ||
                    Path.GetExtension(file) == ".jpeg";
            }).ToList();
            return images;
        }

        public static bool IsUserValid(string key, string user)
        {
            string testURL = "https://osu.ppy.sh/api/get_user?k=" + key + "&u=" + user;
            string data = GetData(testURL);
            return data.Length > 200;
        }

        public static string GetData(string url)
        {
            using (WebClient client = new WebClient())
            {
                client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0) (compatible; MSIE 6.0; Windows NT 5.1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)");
                string s = "";
                try
                {
                    s = client.DownloadString(url);
                }catch(Exception){
                    s = string.Empty;
                }
                return s;
            }
        }

        public static string GetOsuUserHeaderUrl(string profileurl)
        {
            ApiOsuProfile cv = GetOsuUserProfile(profileurl);
            return cv!=null?cv.CoverURL:string.Empty;
        }

        public static ApiOsuProfile GetOsuUserProfile(string profileurl)
        {
            string data = GetData(profileurl);

            if (data.Length > 30)
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(data);

                try
                {
                    string jsonData = htmlDoc.DocumentNode.Descendants("script").FirstOrDefault(a => a.HasClass("js-current-user")).InnerText;

                    ApiOsuProfile cv = JsonConvert.DeserializeObject<ApiOsuProfile>(jsonData);
                    return cv;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }
    }

    public class ApiOsuProfile
    {
        [JsonProperty("cover_url")]
        public string CoverURL { get; set; }

        [JsonProperty("is_admin")]
        public bool IsAdmin { get; set; }

        [JsonProperty("is_bng")]
        public bool IsBNG { get; set; }

        [JsonProperty("is_full_bn")]
        public bool IsFullBN { get; set; }

        [JsonProperty("is_gmt")]
        public bool IsGMT { get; set; }

        [JsonProperty("is_limited_bn")]
        public bool IsLimitedBN { get; set; }

        [JsonProperty("is_moderator")]
        public bool IsModerator { get; set; }

        [JsonProperty("is_nat")]
        public bool IsNAT { get; set; }

        [JsonProperty("is_restricted")]
        public bool IsRestricted { get; set; }

        [JsonProperty("is_silenced")]
        public bool IsSilenced { get; set; }

        [JsonProperty("groups")]
        public List<ApiOsuProfileGroup> Groups { get; set; }

        [JsonProperty("monthly_playcounts")]
        public List<ApiOsuMonthlyCount> PlaycountData { get; set; }

        [JsonProperty("replays_watched_counts")]
        public List<ApiOsuMonthlyCount> ReplaycountData { get; set; }
    }

    public class ApiOsuMonthlyCount {
        [JsonProperty("start_date")]
        public string Date { get; set; }

        [JsonProperty("count")]
        public int Value { get; set; }
    }

    public class ApiOsuProfileGroup
    {
        [JsonProperty("colour")]
        public string Color { get; set; }

        [JsonProperty("has_listing")]
        public bool HasListing { get; set; }

        [JsonProperty("has_playmodes")]
        public bool HasPlaymodes { get; set; }

        [JsonProperty("id")]
        public bool ID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("short_name")]
        public string ShortName { get; set; }
    }
}
