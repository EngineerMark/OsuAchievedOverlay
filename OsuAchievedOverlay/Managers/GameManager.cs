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

        private DispatcherTimer timer;
        private DispatcherTimer progressTimer;
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

                RefreshTimer(null, null);
                //Update every minute

                RestartTimers(Convert.ToInt32(SettingsManager.Instance.Settings["api"]["updateRate"]));

                updateThread = new ExtendedThread(()=>
                {
                    if (WindowManager.Instance.BetaDisplayWin != null && SessionManager.Instance.CurrentSession != null && !SessionManager.Instance.CurrentSession.ReadOnly)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            WindowManager.Instance.BetaDisplayWin.LabelSessionTime.Content = "Session started " +
                                HumanizerExtensions.Humanize(DateTimeOffset.FromUnixTimeSeconds(SessionManager.Instance.CurrentSession.SessionDate).UtcDateTime);
                        }));
                    }
                }, 1000);
                updateThread.Start();

                LocalAPIManager.Instance.Start();
            }
        }

        private void ProgressTick(float updateRate){
            double interval = updateRate;
            double secondsPassed = DateTimeOffset.Now.ToUnixTimeSeconds() - lastTimerFire;
            WindowManager.Instance.BetaDisplayWin.ProgressNextUpdate.SetPercent((lastTimerFire == -1 ? 0 : (secondsPassed.Map(0, updateRate, 0, updateRate + 1) / interval)));
        }

        public void RestartTimers(int updateRate)
        {
            //timer?.Stop();
            //timer = new DispatcherTimer(DispatcherPriority.SystemIdle);
            //timer.Tick += new EventHandler(RefreshTimer);
            //timer.Interval = TimeSpan.FromSeconds(updateRate);
            //timer.Start();

            fetchThread?.Join();
            fetchThread = new ExtendedThread(() =>
            {
                RefreshTimer(null, null);
                lastTimerFire = DateTimeOffset.Now.ToUnixTimeSeconds();
            }, updateRate * 1000);
            fetchThread.Start();
            RefreshTimer(null, null);

            lastTimerFire = DateTimeOffset.Now.ToUnixTimeSeconds();

            progressThread?.Join();
            progressThread = new ExtendedThread(() =>
            {
                double interval = updateRate;
                double secondsPassed = DateTimeOffset.Now.ToUnixTimeSeconds() - lastTimerFire;

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    WindowManager.Instance.BetaDisplayWin.ProgressNextUpdate.SetPercent((lastTimerFire == -1 ? 0 : (secondsPassed.Map(0, updateRate, 0, updateRate + 1) / interval)));
                }));
            }, 1000);
            progressThread.Start();

            //progressTimer?.Stop();
            //progressTimer = new DispatcherTimer(DispatcherPriority.SystemIdle);
            //progressTimer.Tick += (object s, EventArgs e) => ProgressTick(updateRate);
            //progressTimer.Interval = new TimeSpan(0, 0, 1);
            //progressTimer.Start();
            //ProgressTick(updateRate);
        }

        public override void Stop()
        {
            timer?.Stop();
            timer = null;

            progressTimer?.Stop();
            progressTimer = null;

            fetchThread?.Join();
            progressThread?.Join();
            updateThread?.Join();

            LocalAPIManager.Instance.Stop();
            SessionManager.Instance.Stop();
            FileManager.Instance.Stop();

            WindowManager.Instance.CloseAll();
        }

        public void RefreshTimer(object sender, EventArgs e)
        {
            if (WindowManager.Instance.BetaDisplayWin != null && SessionManager.Instance.CurrentSession != null && OsuUser != null)
            {
                bool apiReady = OsuApiHelper.APIHelper<string>.GetDataFromWeb("https://osu.ppy.sh/api/get_user?k=" + SettingsManager.Instance.Settings["api"]["key"] + "&u=peppy") != "";
                if (apiReady)
                {
                    bool _continue = true;

                    if (_continue)
                    {
                        SessionManager.Instance.UpdateSession();

                        foreach (LocalApiFile apiFile in LocalAPIManager.Instance.ApiDataList)
                            LocalAPIManager.Instance.SaveData(apiFile);
                    }

                    if (sender != null)
                    {
                        lastTimerFire = DateTimeOffset.Now.ToUnixTimeSeconds();
                    }
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
            RefreshTimer(null, null);
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

            if (timer?.Interval.TotalSeconds != updateRate)
                RestartTimers(updateRate);

            if (OsuApiHelper.OsuApiKey.Key != data["api"]["key"])
            {
                OsuApiHelper.OsuApiKey.Key = data["api"]["key"];
                OsuUser = OsuApiHelper.OsuApi.GetUser(data["api"]["user"], (OsuApiHelper.OsuMode)Enum.Parse(typeof(OsuApiHelper.OsuMode), data["api"]["gamemode"]));

                RefreshSession();
            }
            else
                RefreshTimer(null, null);
        }

        public void RefreshSession()
        {
            if (lastRefresh == -1 || DateTimeOffset.Now.ToUnixTimeSeconds() - lastRefresh > 15)
            {
                SessionManager.Instance.CurrentSession = new Session();
                RestartTimers((int)timer.Interval.TotalSeconds);

                lastRefresh = DateTimeOffset.Now.ToUnixTimeSeconds();
            }
        }
    }
}
