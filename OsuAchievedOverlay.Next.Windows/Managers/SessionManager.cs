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
        public ExtendedThread ProgressThread { get; set; }

        private long lastIteration = -1;
        private long nextIteration = -1;

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
                    BrowserViewModel.Instance.SendNotification(NotificationType.Warning, StringStorage.Get("Message.UnreadableSession"));
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
                        BrowserViewModel.Instance.SendNotification(NotificationType.Warning, StringStorage.Get("Message.UnreadableSession"));
                    }
                    if (storedSessions != null || storedSessions.Count > 0)
                    {
                        SessionFiles.AddRange(storedSessions);
                        PopulateLoadList();
                    }
                }
            }
        }

        public void StartProgressHandler(){
            StopProgressHandler();
            ProgressThread = new ExtendedThread(() =>
            {
                float perc = 0;
                float seconds_left = 0;

                if(nextIteration!=-1 && lastIteration!=-1)
                {
                    long currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();

                    long normalizedTime = nextIteration - lastIteration;
                    long normalizedCurrent = nextIteration - currentTime;

                    if (normalizedTime < 0)
                        normalizedTime = 0;

                    if (normalizedCurrent < 0)
                        normalizedCurrent = 0;

                    if (normalizedTime == 0 || normalizedCurrent == 0)
                        perc = 0;
                    else
                        perc = (float)normalizedCurrent / (float)normalizedTime;

                    seconds_left = normalizedCurrent+1;
                }

                int actualPerc = 100-Convert.ToInt32(Math.Round(perc * 100));

                actualPerc = Math.Min(100, Math.Max(0, actualPerc));

                BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#progressbarSessionTimer').css('width', '" + actualPerc + "%');");
                BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#sessionProgressTimeText').html('"+ seconds_left + " seconds');");
            }, 1);
            ProgressThread.Start();
        }

        public void StopProgressHandler(){
            ProgressThread?.Join();
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
                CurrentSession = new Session
                {
                    Username = SettingsManager.Instance.Settings["api"]["user"]
                };
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
                    BrowserViewModel.Instance.SendNotification(NotificationType.Danger, StringStorage.Get("Message.UserDataError"));
                    return;
                }
                BrowserViewModel.Instance.ApplySession(CurrentSession);
                BrowserViewModel.Instance.ApplyUser(currentUserData);

                StopProgressHandler();
                BrowserViewModel.Instance.AttachedJavascriptWrapper.Hide("#sessionProgressTime");
                BrowserViewModel.Instance.AttachedJavascriptWrapper.Show("#sessionProgressReadonly");
            }
            else
            {
                SessionThread = new ExtendedThread(() => OnUpdate(), Convert.ToInt32(SettingsManager.Instance.Settings["api"]["updateRate"]));

                lastIteration = DateTimeOffset.Now.ToUnixTimeSeconds();
                nextIteration = DateTimeOffset.Now.ToUnixTimeSeconds() + SessionThread.SleepTime;

                StartProgressHandler();
                SessionThread.Start();

                BrowserViewModel.Instance.AttachedJavascriptWrapper.Show("#sessionProgressTime");
                BrowserViewModel.Instance.AttachedJavascriptWrapper.Hide("#sessionProgressReadonly");
            }
        }

        public void Stop()
        {
            StopProgressHandler();
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
                BrowserViewModel.Instance.SendNotification(NotificationType.Warning, StringStorage.Get("Message.NoInternet"));
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
                BrowserViewModel.Instance.SendNotification(NotificationType.Danger, StringStorage.Get("Message.UserDataError"));
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

            lastIteration = DateTimeOffset.Now.ToUnixTimeSeconds();
            nextIteration = DateTimeOffset.Now.ToUnixTimeSeconds() + SessionThread.SleepTime;
        }
    }
}
