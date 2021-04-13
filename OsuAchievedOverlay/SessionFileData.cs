using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay
{
    public class SessionFileData
    {
        public string FileName { get; set; }
        public string FileLocation { get; set; }
        public string FileExtension { get; set; }
        public long FileDate { get; set; }

        [JsonIgnore]
        public string Identifier
        {
            get
            {
                return StringHelper.Base64Encode(FileLocation+FileName+FileExtension);
            }
        }

        public SessionFileData Clone()
        {
            return new SessionFileData()
            {
                FileName = FileName,
                FileLocation = FileLocation,
                FileExtension = FileExtension,
                FileDate = FileDate
            };
        }

        public static bool IsEqual(SessionFileData a, SessionFileData b)
        {
            if (a == null || b == null)
                return false;

            return (a.FileName == b.FileName && a.FileLocation == b.FileLocation && a.FileExtension == b.FileExtension);
        }
    }
}
