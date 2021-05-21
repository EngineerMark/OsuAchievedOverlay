using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Next.Managers
{
    public class FileManager : StaticManager
    {
        public static string GetExecutablePath(){
            return System.Reflection.Assembly.GetEntryAssembly().Location;
        }

        public static string GetExecutableDirectory(){
            return Path.GetDirectoryName(GetExecutablePath());
        }
    }
}
