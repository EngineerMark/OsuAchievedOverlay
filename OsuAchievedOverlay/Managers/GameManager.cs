using Humanizer;
using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace OsuAchievedOverlay.Managers
{
    //This is clearly not a game, but I work in such industry, its standard for me..
    public class GameManager : Manager<GameManager>
    {
        private OsuApiHelper.OsuUser osuUser = null;

        private Session currentSession;
        private IniData settings;

        private DispatcherTimer timer;
        private DispatcherTimer progressTimer;
        private DispatcherTimer updateTimer;

        private long lastTimerFire = -1;
        private long lastRefresh = -1;

        public static IniData DefaultSettings
        {
            get
            {
                return new IniData()
                {
                    ["api"] = {
                        ["key"] = "No key inserted",
                        ["user"] = "Username here",
                        ["updateRate"] = "60",
                        ["gamemode"] = ""+OsuApiHelper.OsuMode.Standard
                    },
                    ["rpc"] = {
                        ["enabled"] = "0",
                        ["status"] = ""+DiscordManager.DiscordDisplay.GainedSS
                    }
                };
            }
        }

        public Session CurrentSession { get => currentSession; set => currentSession = value; }
        public IniData Settings { get => settings; set => settings = value; }

        public override void Start()
        {
            bool success = LoadSettings();
            if (success)
            {
                WindowManager.Instance.BetaDisplayWin = new BetaDisplayWindow();
                WindowManager.Instance.BetaDisplayWin.Show();
                WindowManager.Instance.BetaDisplayWin.Focus();
                ApplySettingsToApp(Settings);

                if (OsuApiHelper.OsuApi.IsKeyValid() && OsuApiHelper.OsuApi.IsUserValid(Settings["api"]["user"]))
                {
                    if (osuUser == null)
                        osuUser = OsuApiHelper.OsuApi.GetUser(Settings["api"]["user"], (OsuApiHelper.OsuMode)Enum.Parse(typeof(OsuApiHelper.OsuMode), Settings["api"]["gamemode"]));

                    CurrentSession = new Session()
                    {
                        InitialData = SessionData.FromUser(osuUser)
                    };

                }

                RefreshTimer(null, null);
                //Update every minute

                RestartTimers(Convert.ToInt32(Settings["api"]["updateRate"]));

                updateTimer = new DispatcherTimer(DispatcherPriority.SystemIdle);
                updateTimer.Tick += new EventHandler((object s, EventArgs e) => Update());
                updateTimer.Interval = TimeSpan.FromMilliseconds(1 / 60);
                updateTimer.Start();

                LocalAPIManager.Instance.Start();

                if(Settings["rpc"]["enabled"]=="1")
                    DiscordManager.Instance.Start();
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
            DiscordManager.Instance.Stop();
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
            if (WindowManager.Instance.BetaDisplayWin != null && CurrentSession != null)
            {
                WindowManager.Instance.BetaDisplayWin.LabelSessionTime.Content = "Session started " +
                    DateTimeOffset.FromUnixTimeSeconds(CurrentSession.SessionDate).UtcDateTime.Humanize();
            }
        }

        public void RefreshTimer(object sender, EventArgs e)
        {
            if (WindowManager.Instance.BetaDisplayWin != null && CurrentSession != null && osuUser != null)
            {
                bool apiReady = OsuApiHelper.APIHelper<string>.GetDataFromWeb("https://osu.ppy.sh/api/get_user?k=" + Settings["api"]["key"] + "&u=peppy") != "";
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
            osuUser = OsuApiHelper.OsuApi.GetUser(settings["api"]["user"], (OsuApiHelper.OsuMode)Enum.Parse(typeof(OsuApiHelper.OsuMode), Settings["api"]["gamemode"]));

            if (!CurrentSession.ReadOnly)
            {
                if (CurrentSession.InitialData == null)
                    CurrentSession.InitialData = SessionData.FromUser(osuUser);
                CurrentSession.CurrentData = SessionData.FromUser(osuUser);

                //List<OsuApiHelper.OsuPlay> newPlays = OsuApiHelper.OsuApi.GetUserRecent(osuUser.Name, (OsuApiHelper.OsuMode)Enum.Parse(typeof(OsuApiHelper.OsuMode), Settings["api"]["gamemode"]), 20, false);
                //CurrentSession.AddNewPlays(newPlays);
                WindowManager.Instance.BetaDisplayWin.ButtonWarning.Visibility = Visibility.Hidden;
            }
            else{
                if (CurrentSession.CurrentData == null)
                    CurrentSession.CurrentData = (SessionData)CurrentSession.InitialData.Clone();

                WindowManager.Instance.BetaDisplayWin.ButtonWarning.Visibility = Visibility.Visible;
            }
            CurrentSession.DifferenceData = SessionData.CalculateDifference(CurrentSession.CurrentData, CurrentSession.InitialData);
            WindowManager.Instance.BetaDisplayWin.ApplyUser(osuUser);
            WindowManager.Instance.BetaDisplayWin.ApplySession(CurrentSession);
        }

        public bool LoadSettings()
        {
            FileIniDataParser parser = new FileIniDataParser();
            if (File.Exists("Settings.ini"))
            {
                IniData data = parser.ReadFile("Settings.ini");
                data = FixIniData(parser, data);
                OsuApiHelper.OsuApiKey.Key = data["api"]["key"];
                osuUser = OsuApiHelper.OsuApi.GetUser(data["api"]["user"], (OsuApiHelper.OsuMode)Enum.Parse(typeof(OsuApiHelper.OsuMode), data["api"]["gamemode"]));

                int updateRate = int.Parse(data["api"]["updateRate"]);
                updateRate = Math.Min(120, Math.Max(5, updateRate));
                data["api"]["updateRate"] = "" + updateRate;

                Settings = data;
                return true;
            }
            else
            {
                //IniData newData = new IniData();

                //newData = FixIniData(parser, newData);
                //parser.WriteFile("Settings.ini", newData);

                //MessageBoxResult result = MessageBox.Show("No settings file was present yet. Generated one. Please enter leftover values in the file.\nPress 'OK' to open the settings location.", "No settings file", MessageBoxButton.OKCancel);
                //if (result == MessageBoxResult.OK)
                //{
                //    Process.Start(new ProcessStartInfo()
                //    {
                //        Arguments = Directory.GetCurrentDirectory(),
                //        FileName = "explorer.exe"
                //    });
                //}
                //WindowManager.Instance.MainWin.Close();
                return false;
            }
        }

        public static IniData FixIniData(FileIniDataParser parser, IniData data)
        {
            foreach (SectionData section in DefaultSettings.Sections)
            {
                foreach (KeyData key in section.Keys)
                {
                    if (data[section.SectionName][key.KeyName] == null)
                        data[section.SectionName][key.KeyName] = key.Value;
                }
            }

            parser.WriteFile("Settings.ini", data);
            return data;
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

        public void SettingsSave()
        {
            //FileIniDataParser parser = new FileIniDataParser();
            //IniData data = parser.ReadFile("Settings.ini");

            //parser.WriteFile("Settings.ini", data);

            //Settings = data;
            //ApplySettingsToApp(Settings);
            FileIniDataParser parser = new FileIniDataParser();
            parser.WriteFile("Settings.ini", Settings);

            ApplySettingsToApp(Settings);
        }

        private void ApplySettingsToApp(IniData data)
        {
            bool cv = int.TryParse(data["api"]["updateRate"], out int updateRate);
            if (!cv)
                updateRate = 60;

            if (timer?.Interval.TotalSeconds != updateRate)
                RestartTimers(updateRate);

            if (OsuApiHelper.OsuApiKey.Key != data["api"]["key"])
            {
                OsuApiHelper.OsuApiKey.Key = data["api"]["key"];
                osuUser = OsuApiHelper.OsuApi.GetUser(data["api"]["user"], (OsuApiHelper.OsuMode)Enum.Parse(typeof(OsuApiHelper.OsuMode), data["api"]["gamemode"]));

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
