using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay
{
    public class Version
    {
        public int MainVersion { get; set; }
        public int BetaVersion { get; set; }
        public int Release { get; set; }
        public int Hotfix { get; set; }

        public Version(string s){
            string[] splitString = s.Split('.');
            MainVersion = Convert.ToInt32(splitString[0]);
            BetaVersion = Convert.ToInt32(splitString[1]);
            Release = Convert.ToInt32(splitString[2]);
            if (splitString.Length > 3)
                Hotfix = Convert.ToInt32(splitString[3]);
            else
                Hotfix = 0;
        }

        /// <summary>
        /// Combines version integers together for comparison purposes
        /// </summary>
        private int Combine(){
            int com = MainVersion * 1000 + BetaVersion * 100 + Release * 10 + Hotfix;
            return com;
        }

        public static bool operator >(Version a, Version b){
            return a.Combine() > b.Combine();
        }

        public static bool operator <(Version a, Version b)
        {
            return a.Combine() < b.Combine();
        }

        public static bool operator >=(Version a, Version b)
        {
            return a.Combine() >= b.Combine();
        }

        public static bool operator <=(Version a, Version b)
        {
            return a.Combine() <= b.Combine();
        }

        public static bool operator ==(Version a, Version b)
        {
            return a.Combine() == b.Combine();
        }

        public static bool operator !=(Version a, Version b)
        {
            return a.Combine() != b.Combine();
        }
    }
}
