using Humanizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay
{
    public static class HumanizerExtensions
    {
        public static string Humanize(DateTime dateTime, bool utcDate = true, DateTime? dateToCompareAgainst = null){
            return dateTime.Humanize(utcDate, dateToCompareAgainst, new System.Globalization.CultureInfo("en-US"));
        }
    }
}
