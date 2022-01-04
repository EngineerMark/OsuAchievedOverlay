using OsuAchievedOverlay.Next.Helpers;
using OsuApiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Next.Tools
{
    public class ToolUsers : Singleton<ToolUsers>
    {
        public OsuUser FindUser(string query){
            return OsuApi.GetUser(query);
        }
    }
}
