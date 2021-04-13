using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay
{
    public class SessionManager : Manager<SessionManager>
    {
        public List<Session> sessions;
        public List<SessionFileData> SessionFiles { get; set; }

        private const string sessionListFilename = "stored_sessions.json";

        public SessionManager(){
            Start();
        }

        public List<SessionFileData> GetSortedList(){
            SessionFiles.Sort(delegate (SessionFileData a, SessionFileData b)
            {
                return b.FileDate.CompareTo(a.FileDate);
            });

            return SessionFiles;
        }

        public SessionFileData FindByIdentifier(string identifier){
            return SessionFiles.Find(a => a.Identifier == identifier);
        }

        public SessionFileData BuildSessionFile(string file){
            return new SessionFileData()
            {
                FileName = System.IO.Path.GetFileNameWithoutExtension(file),
                FileExtension = System.IO.Path.GetExtension(file),
                FileLocation = System.IO.Path.GetDirectoryName(file),
                FileDate = DateTimeOffset.Now.ToUnixTimeSeconds()
            };
        }

        public void AddFile(string file){
            AddFile(BuildSessionFile(file));
        }

        public void AddFile(SessionFileData fileData){
            if (SessionFiles == null)
                SessionFiles = new List<SessionFileData>();
            bool addFile = true;
            SessionFileData existing = null;
            foreach (SessionFileData fd in SessionFiles){
                if (SessionFileData.IsEqual(fd,fileData)){
                    existing = fd;
                    addFile = false;
                }
            }

            if(addFile){
                SessionFiles.Add(fileData.Clone());
            }else{
                if(existing!=null)
                    existing.FileDate = fileData.FileDate;
            }
        }

        public override void Start()
        {
            SessionFiles = new List<SessionFileData>();

            if (File.Exists(sessionListFilename))
            {
                string data = File.ReadAllText(sessionListFilename);
                SessionFiles = JsonConvert.DeserializeObject<List<SessionFileData>>(data);
            }
        }

        public override void Stop()
        {
            if(File.Exists(sessionListFilename)){
                File.Delete(sessionListFilename);
            }
            using(StreamWriter sw = new StreamWriter(sessionListFilename)){
                string data = JsonConvert.SerializeObject(SessionFiles);
                sw.Write(data);
                sw.Close();
            }
        }
    }
}
