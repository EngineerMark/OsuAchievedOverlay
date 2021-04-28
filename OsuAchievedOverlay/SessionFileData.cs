using Newtonsoft.Json;
using System.IO;

namespace OsuAchievedOverlay
{
    public class SessionFileData
    {
        public string FileName { get; set; } = "";
        public string FileLocation { get; set; } = "";
        public string FileExtension { get; set; } = "";
        public long FileDate { get; set; } = -1;

        [JsonIgnore]
        public string Identifier
        {
            get
            {
                return StringHelper.Base64Encode(Path.Combine(FileLocation, FileName+FileExtension));
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
