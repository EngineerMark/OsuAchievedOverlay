using Newtonsoft.Json;
using OsuApiHelper;
using System;
using System.Collections.Generic;
using System.IO;
using CefSharp;
using System.Web;

namespace OsuAchievedOverlay.Next.Managers
{
    public class SessionManager : Singleton<SessionManager>
    {
        public List<SessionFileData> SessionFiles { get; set; }
        public Session CurrentSession { get; set; }
        public ExtendedThread SessionThread { get; set; }

        public SessionManager()
        {
            SessionFiles = new List<SessionFileData>();
            if (File.Exists("Data/stored_sessions.json"))
            {
                string data = string.Empty;
                try
                {
                    data = File.ReadAllText("Data/stored_sessions.json");
                }
                catch (Exception)
                {
                    BrowserViewModel.Instance.SendNotification(NotificationType.Warning, "Session list file is present, but unable to read it");
                }

                if (!string.IsNullOrEmpty(data))
                {
                    List<SessionFileData> storedSessions = null;
                    try
                    {
                        storedSessions = JsonConvert.DeserializeObject<List<SessionFileData>>(data);
                    }
                    catch (Exception)
                    {
                        BrowserViewModel.Instance.SendNotification(NotificationType.Warning, "Session list file is present, but unable to read it");
                    }
                    if (storedSessions != null || storedSessions.Count > 0)
                    {
                        SessionFiles.AddRange(storedSessions);
                        PopulateLoadList();
                    }
                }
            }
        }

        public void PopulateLoadList()
        {
            if (SessionFiles != null)
            {
                string data = JsonConvert.SerializeObject(SessionFiles);
                BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("FillSessionDataList('" + HttpUtility.JavaScriptStringEncode(data) + "');");
            }
        }

        public List<SessionFileData> GetSortedList()
        {
            SessionFiles.Sort(delegate (SessionFileData a, SessionFileData b)
            {
                return b.FileDate.CompareTo(a.FileDate);
            });

            return SessionFiles;
        }

        public SessionFileData FindByIdentifier(string identifier)
        {
            return SessionFiles.Find(a => a.Identifier == identifier);
        }

        public SessionFileData BuildSessionFile(string file)
        {
            return new SessionFileData()
            {
                FileName = System.IO.Path.GetFileNameWithoutExtension(file),
                FileExtension = System.IO.Path.GetExtension(file),
                FileLocation = System.IO.Path.GetDirectoryName(file),
                FileDate = DateTimeOffset.Now.ToUnixTimeSeconds()
            };
        }

        public void AddFile(string file)
        {
            AddFile(BuildSessionFile(file));
        }

        public void AddFile(SessionFileData fileData)
        {
            if (SessionFiles == null)
                SessionFiles = new List<SessionFileData>();
            bool addFile = true;
            SessionFileData existing = null;
            foreach (SessionFileData fd in SessionFiles)
            {
                if (SessionFileData.IsEqual(fd, fileData))
                {
                    existing = fd;
                    addFile = false;
                }
            }

            if (addFile)
            {
                SessionFiles.Add(fileData.Clone());
            }
            else
            {
                if (existing != null)
                    existing.FileDate = fileData.FileDate;
            }
            PopulateLoadList();
        }

        //Prepare a new session
        public void PrepareSession(Session session = null)
        {
            SessionThread?.Join();

            if (session == null)
            {
                CurrentSession = new Session();
                CurrentSession.Username = SettingsManager.Instance.Settings["api"]["user"];
            }
            else
                CurrentSession = session;

            if (CurrentSession.ReadOnly)
            {
                OsuUser currentUserData = null;
                try
                {
                    currentUserData = OsuApi.GetUser(CurrentSession.Username, (OsuMode)Enum.Parse(typeof(OsuMode), SettingsManager.Instance.Settings["api"]["gamemode"]));
                }
                catch (Exception)
                {
                    BrowserViewModel.Instance.SendNotification(NotificationType.Danger, "Error retrieving user data");
                    return;
                }
                BrowserViewModel.Instance.ApplySession(CurrentSession);
                BrowserViewModel.Instance.ApplyUser(currentUserData);
            }
            else
            {
                SessionThread = new ExtendedThread(() => OnUpdate(), Convert.ToInt32(SettingsManager.Instance.Settings["api"]["updateRate"]));
                SessionThread.Start();
            }

        }

        public void Stop()
        {
            SessionThread?.Join();

            if (File.Exists("Data/stored_sessions.json"))
            {
                File.Delete("Data/stored_sessions.json");
            }
            using (StreamWriter sw = new StreamWriter("Data/stored_sessions.json"))
            {
                string data = JsonConvert.SerializeObject(SessionFiles);
                sw.Write(data);
                sw.Close();
            }
        }

        private void OnUpdate()
        {
            if (!NetworkManager.Instance.HasConnection())
            {
                BrowserViewModel.Instance.SendNotification(NotificationType.Warning, "You are not connected to the internet");
                return;
            }

            //BrowserViewModel.Instance.SendNotification(NotificationType.Success, "Test: session iteration");
            OsuUser currentUserData = null;
            try
            {
                currentUserData = OsuApi.GetUser(CurrentSession.Username, (OsuMode)Enum.Parse(typeof(OsuMode), SettingsManager.Instance.Settings["api"]["gamemode"]));
            }
            catch (Exception)
            {
                BrowserViewModel.Instance.SendNotification(NotificationType.Danger, "Error retrieving user data");
            }

            if (currentUserData != null)
            {
                if (CurrentSession.InitialData == null)
                    CurrentSession.InitialData = SessionData.FromUser(currentUserData);

                CurrentSession.CurrentData = SessionData.FromUser(currentUserData);
                CurrentSession.DifferenceData = CurrentSession.CurrentData - CurrentSession.InitialData;

                BrowserViewModel.Instance.ApplySession(CurrentSession);
                BrowserViewModel.Instance.ApplyUser(currentUserData);

                int updateRate = Convert.ToInt32(SettingsManager.Instance.Settings["api"]["updateRate"]);
                if (updateRate != SessionThread.SleepTime)
                    SessionThread.SleepTime = updateRate;
            }
        }
    }
}
