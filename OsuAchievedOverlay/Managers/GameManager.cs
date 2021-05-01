using Humanizer;
using IniParser.Model;
using OsuApiHelper;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace OsuAchievedOverlay.Managers
{
    //This is clearly not a game, but I work in such industry, its standard for me..
    public class GameManager : Manager<GameManager>
    {
        private OsuApiHelper.OsuUser osuUser = null;

        private ExtendedThread fetchThread;
        private ExtendedThread progressThread;
        private ExtendedThread updateThread;

        private long lastTimerFire = -1;
        private long lastRefresh = -1;

        public OsuUser OsuUser { get => osuUser; set => osuUser = value; }

        public override void Start()
        {
            bool success = SettingsManager.Instance.LoadSettings();
            if (success)
            {
                WindowManager.Instance.BetaDisplayWin = new BetaDisplayWindow();
                WindowManager.Instance.BetaDisplayWin.Show();
                WindowManager.Instance.BetaDisplayWin.Focus();
                ApplySettingsToApp(SettingsManager.Instance.Settings);

                if (OsuUser == null)
                    OsuUser = OsuApiHelper.OsuApi.GetUser(SettingsManager.Instance.Settings["api"]["user"], (OsuApiHelper.OsuMode)Enum.Parse(typeof(OsuApiHelper.OsuMode), SettingsManager.Instance.Settings["api"]["gamemode"]));

                SessionManager.Instance.CurrentSession = new Session()
                {
                    InitialData = SessionData.FromUser(OsuUser)
                };

                RefreshTimer();

                RestartTimers(Convert.ToInt32(SettingsManager.Instance.Settings["api"]["updateRate"]));

                updateThread = new ExtendedThread(()=>
                {
                    if (WindowManager.Instance.BetaDisplayWin != null && SessionManager.Instance.CurrentSession != null && !SessionManager.Instance.CurrentSession.ReadOnly)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            if(WindowManager.Instance!=null && WindowManager.Instance.BetaDisplayWin!=null)
                                WindowManager.Instance.BetaDisplayWin.DisplaySession.LabelSessionTime.Content = "Session started " +
                                    HumanizerExtensions.Humanize(DateTimeOffset.FromUnixTimeSeconds(SessionManager.Instance.CurrentSession.SessionDate).UtcDateTime);
                        }));
                    }
                }, 1000);
                updateThread.Start();

                LocalAPIManager.Instance.Start();
            }
        }

        public void RestartTimers(int updateRate)
        {
            fetchThread?.Join();
            fetchThread = new ExtendedThread(() =>
            {
                RefreshTimer();
                lastTimerFire = DateTimeOffset.Now.ToUnixTimeSeconds();
            }, updateRate * 1000);
            fetchThread.Start();
            RefreshTimer();

            lastTimerFire = DateTimeOffset.Now.ToUnixTimeSeconds();

            progressThread?.Join();
            int progressbarSpeed = 2;
            progressThread = new ExtendedThread(() =>
            {
                double interval = updateRate;
                double secondsPassed = DateTimeOffset.Now.ToUnixTimeSeconds() - lastTimerFire;

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if(WindowManager.Instance!=null && WindowManager.Instance.BetaDisplayWin!=null)
                        WindowManager.Instance.BetaDisplayWin.DisplaySession.ProgressNextUpdate.SetPercent((lastTimerFire == -1 ? 0 : (secondsPassed.Map(0, updateRate, 0, updateRate + 1) / interval)), TimeSpan.FromSeconds(progressbarSpeed));
                }));
            }, progressbarSpeed*1000);
            progressThread.Start();
        }

        public override void Stop()
        {
            fetchThread?.Join();
            progressThread?.Join();
            updateThread?.Join();

            LocalAPIManager.Instance.Stop();
            SessionManager.Instance.Stop();
            FileManager.Instance.Stop();

            WindowManager.Instance.CloseAll();
        }

        public void RefreshTimer()
        {
            if (WindowManager.Instance.BetaDisplayWin != null && SessionManager.Instance.CurrentSession != null && OsuUser != null)
            {
                bool apiReady = OsuApiHelper.APIHelper<string>.GetDataFromWeb("https://osu.ppy.sh/api/get_user?k=" + SettingsManager.Instance.Settings["api"]["key"] + "&u=peppy") != "";
                if (apiReady)
                {
                    SessionManager.Instance.UpdateSession();

                    foreach (LocalApiFile apiFile in LocalAPIManager.Instance.ApiDataList)
                        LocalAPIManager.Instance.SaveData(apiFile);
                }
            }
        }

        public void OpenDisplay(bool closeCheck = true)
        {
            if (closeCheck)
                CloseDisplay();
            WindowManager.Instance.BetaDisplayWin = new BetaDisplayWindow();
            WindowManager.Instance.BetaDisplayWin.Show();
            WindowManager.Instance.BetaDisplayWin.Focus();
            RefreshTimer();
        }

        public void FocusDisplay()
        {
            WindowManager.Instance.BetaDisplayWin.Focus();
        }

        public void CloseDisplay()
        {
            WindowManager.Instance.BetaDisplayWin?.Close();
        }

        public void ApplySettingsToApp(IniData data)
        {
            bool cv = int.TryParse(data["api"]["updateRate"], out int updateRate);
            if (!cv)
                updateRate = 60;

            if (fetchThread?.SleepTime != updateRate*1000)
                RestartTimers(updateRate);

            if (OsuApiHelper.OsuApiKey.Key != data["api"]["key"])
            {
                OsuApiHelper.OsuApiKey.Key = data["api"]["key"];
                OsuUser = OsuApiHelper.OsuApi.GetUser(data["api"]["user"], (OsuApiHelper.OsuMode)Enum.Parse(typeof(OsuApiHelper.OsuMode), data["api"]["gamemode"]));

                RefreshSession();
            }
            else
                RefreshTimer();
        }

        public void RefreshSession()
        {
            if (lastRefresh == -1 || DateTimeOffset.Now.ToUnixTimeSeconds() - lastRefresh > 15)
            {
                SessionManager.Instance.CurrentSession = new Session();
                RestartTimers((int)fetchThread?.SleepTime/1000);

                lastRefresh = DateTimeOffset.Now.ToUnixTimeSeconds();
            }
        }
    }
}
