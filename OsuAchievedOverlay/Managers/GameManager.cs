using Humanizer;
using IniParser.Model;
using OsuApiHelper;
using System;
using System.Windows;
using System.Windows.Threading;

namespace OsuAchievedOverlay.Managers
{
    //This is clearly not a game, but I work in such industry, its standard for me..
    public class GameManager : Manager<GameManager>
    {
        private OsuApiHelper.OsuUser osuUser = null;

        private Session currentSession;

        private DispatcherTimer timer;
        private DispatcherTimer progressTimer;
        private DispatcherTimer updateTimer;

        private long lastTimerFire = -1;
        private long lastRefresh = -1;

        public Session CurrentSession { get => currentSession; set => currentSession = value; }
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

                if (OsuApiHelper.OsuApi.IsKeyValid() && OsuApiHelper.OsuApi.IsUserValid(SettingsManager.Instance.Settings["api"]["user"]))
                {
                    if (OsuUser == null)
                        OsuUser = OsuApiHelper.OsuApi.GetUser(SettingsManager.Instance.Settings["api"]["user"], (OsuApiHelper.OsuMode)Enum.Parse(typeof(OsuApiHelper.OsuMode), SettingsManager.Instance.Settings["api"]["gamemode"]));

                    CurrentSession = new Session()
                    {
                        InitialData = SessionData.FromUser(OsuUser)
                    };

                }

                RefreshTimer(null, null);
                //Update every minute

                RestartTimers(Convert.ToInt32(SettingsManager.Instance.Settings["api"]["updateRate"]));

                updateTimer = new DispatcherTimer(DispatcherPriority.SystemIdle);
                updateTimer.Tick += new EventHandler((object s, EventArgs e) => Update());
                updateTimer.Interval = TimeSpan.FromMilliseconds(1 / 60);
                updateTimer.Start();

                LocalAPIManager.Instance.Start();
            }
        }

        private void ProgressTick(int updateRate)
        {
            double interval = updateRate;
            double secondsPassed = DateTimeOffset.Now.ToUnixTimeSeconds() - lastTimerFire;
            WindowManager.Instance.BetaDisplayWin.ProgressNextUpdate.SetPercent((lastTimerFire == -1 ? 0 : (secondsPassed.Map(0, updateRate, 0, updateRate + 1) / interval)));
        }

        public void RestartTimers(int updateRate)
        {
            timer?.Stop();
            if (timer == null)
            {
                timer = new DispatcherTimer(DispatcherPriority.SystemIdle);
                timer.Tick += new EventHandler(RefreshTimer);
            }
            timer.Interval = TimeSpan.FromSeconds(updateRate);
            timer.Start();
            RefreshTimer(null, null);

            lastTimerFire = DateTimeOffset.Now.ToUnixTimeSeconds();

            progressTimer?.Stop();
            if (progressTimer == null)
            {
                progressTimer = new DispatcherTimer(DispatcherPriority.SystemIdle);
                progressTimer.Tick += (object s, EventArgs e) => ProgressTick(updateRate);
                progressTimer.Interval = new TimeSpan(0, 0, 1);
            }
            progressTimer.Start();
            ProgressTick(updateRate);
        }

        public override void Stop()
        {
            timer?.Stop();
            timer = null;

            progressTimer?.Stop();
            progressTimer = null;

            updateTimer?.Stop();
            updateTimer = null;

            LocalAPIManager.Instance.Stop();
            SessionManager.Instance.Stop();
            FileManager.Instance.Stop();

            WindowManager.Instance.ApiWin?.Close();
            WindowManager.Instance.ApiWin = null;

            WindowManager.Instance.SessionWin?.Close();
            WindowManager.Instance.SessionWin = null;

            WindowManager.Instance.BetaDisplayWin?.Close();
            WindowManager.Instance.BetaDisplayWin = null;

            WindowManager.Instance.SettingsWin?.Close();
            WindowManager.Instance.SettingsWin = null;

        }

        public void Update()
        {
            if (WindowManager.Instance.BetaDisplayWin != null && CurrentSession != null && !CurrentSession.ReadOnly)
            {
                WindowManager.Instance.BetaDisplayWin.LabelSessionTime.Content = "Session started " +
                    DateTimeOffset.FromUnixTimeSeconds(CurrentSession.SessionDate).UtcDateTime.Humanize();
            }
        }

        public void RefreshTimer(object sender, EventArgs e)
        {
            if (WindowManager.Instance.BetaDisplayWin != null && CurrentSession != null && OsuUser != null)
            {
                bool apiReady = OsuApiHelper.APIHelper<string>.GetDataFromWeb("https://osu.ppy.sh/api/get_user?k=" + SettingsManager.Instance.Settings["api"]["key"] + "&u=peppy") != "";
                if (apiReady)
                {
                    bool _continue = true;

                    if (_continue)
                    {
                        UpdateSession();

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

        private void UpdateSession()
        {
            OsuUser = OsuApiHelper.OsuApi.GetUser(SettingsManager.Instance.Settings["api"]["user"], (OsuApiHelper.OsuMode)Enum.Parse(typeof(OsuApiHelper.OsuMode), SettingsManager.Instance.Settings["api"]["gamemode"]));

            if (!CurrentSession.ReadOnly)
            {
                if (CurrentSession.InitialData == null)
                    CurrentSession.InitialData = SessionData.FromUser(OsuUser);
                CurrentSession.CurrentData = SessionData.FromUser(OsuUser);

                //List<OsuApiHelper.OsuPlay> newPlays = OsuApiHelper.OsuApi.GetUserRecent(osuUser.Name, (OsuApiHelper.OsuMode)Enum.Parse(typeof(OsuApiHelper.OsuMode), Settings["api"]["gamemode"]), 20, false);
                //CurrentSession.AddNewPlays(newPlays);
                if (WindowManager.Instance.BetaDisplayWin.ButtonWarning.Visibility != Visibility.Hidden)
                    WindowManager.Instance.BetaDisplayWin.ButtonWarning.Visibility = Visibility.Hidden;

                if (WindowManager.Instance.BetaDisplayWin.GridNonReadonly.Visibility != Visibility.Visible)
                    WindowManager.Instance.BetaDisplayWin.GridNonReadonly.Visibility = Visibility.Visible;
                if (WindowManager.Instance.BetaDisplayWin.GridReadonly.Visibility != Visibility.Hidden)
                    WindowManager.Instance.BetaDisplayWin.GridReadonly.Visibility = Visibility.Hidden;
            }
            else
            {
                if (CurrentSession.CurrentData == null)
                    CurrentSession.CurrentData = (SessionData)CurrentSession.InitialData.Clone();

                if (WindowManager.Instance.BetaDisplayWin.ButtonWarning.Visibility != Visibility.Visible)
                    WindowManager.Instance.BetaDisplayWin.ButtonWarning.Visibility = Visibility.Visible;

                if (WindowManager.Instance.BetaDisplayWin.GridNonReadonly.Visibility != Visibility.Hidden)
                    WindowManager.Instance.BetaDisplayWin.GridNonReadonly.Visibility = Visibility.Hidden;
                if (WindowManager.Instance.BetaDisplayWin.GridReadonly.Visibility != Visibility.Visible)
                    WindowManager.Instance.BetaDisplayWin.GridReadonly.Visibility = Visibility.Visible;

                DateTime sessionStart = DateTimeOffset.FromUnixTimeSeconds(CurrentSession.SessionDate).UtcDateTime;
                DateTime sessionEnd = DateTimeOffset.FromUnixTimeSeconds(CurrentSession.SessionEndDate).UtcDateTime;

                WindowManager.Instance.BetaDisplayWin.LabelReadonlySessionDate.Content = sessionStart.ToString("g") + " - " + sessionEnd.ToString("g");
            }
            CurrentSession.DifferenceData = SessionData.CalculateDifference(CurrentSession.CurrentData, CurrentSession.InitialData);
            WindowManager.Instance.BetaDisplayWin.ApplyUser(OsuUser);
            WindowManager.Instance.BetaDisplayWin.ApplySession(CurrentSession);
        }

        public void OpenDisplay(bool closeCheck = true)
        {
            if (closeCheck)
                CloseDisplay();
            WindowManager.Instance.BetaDisplayWin = new BetaDisplayWindow();
            WindowManager.Instance.BetaDisplayWin.Show();
            WindowManager.Instance.BetaDisplayWin.Focus();
            //ApplySettingsToApp();
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
                CurrentSession = new Session();
                RestartTimers((int)timer.Interval.TotalSeconds);

                lastRefresh = DateTimeOffset.Now.ToUnixTimeSeconds();
            }
        }
    }
}
