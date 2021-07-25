using HtmlAgilityPack;
using Newtonsoft.Json;
using OsuAchievedOverlay.Next.OsuWeb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Next.Helpers
{
    public static class OsuWebHelper
    {
        public static List<WebUser> GetUserAchievements(OsuApiHelper.OsuUser user){
            return GetUserAchievements(user.ID);
        }

        public static List<WebUser> GetUserAchievements(int userID){
            string url = string.Format("https://osu.ppy.sh/users/{0}", userID);
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            HtmlNode node = doc.GetElementbyId("json-achievements");
            string data = node.InnerText;

            List<WebUser> result = JsonConvert.DeserializeObject<List<WebUser>>(data);
            return result;
        }

        public static List<WebUser> GetUserAchievements(string username)
        {
            string url = string.Format("https://osu.ppy.sh/users/{0}", username);
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            HtmlNode node = doc.GetElementbyId("json-achievements");
            string data = node.InnerText;

            List<WebUser> result = JsonConvert.DeserializeObject<List<WebUser>>(data);
            return result;
        }
    }
}
