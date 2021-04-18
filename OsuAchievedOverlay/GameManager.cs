using Humanizer;
using IniParser;
using IniParser.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;


namespace OsuAchievedOverlay
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

        private IniData DefaultSettings
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
                    ["display"] = {
                        ["labelColor"] = Colors.Black.ToString(),
                        ["background"] = Colors.White.ToString(),
                        ["chromakeyBackground"] = Colors.Green.ToString(),
                        ["useChromaKey"] = "1",
                        ["alwaysOnTop"] = "1",
                        ["labelFont"] = "Segoe UI",
                        ["useRankedScore"] = "0"
                    },
                    ["fileapi"] = {
                        ["prefixPositive"] = "+",
                        ["prefixNegative"] = "-",
                    }
                };
            }
        }

        public Session CurrentSession { get => currentSession; set => currentSession = value; }
        public IniData Settings { get => settings; set => settings = value; }

        public override void Start()
        {
            WindowManager.Instance.MainWin.dropdownGameMode.ItemsSource = Enum.GetValues(typeof(OsuApiHelper.OsuMode)).Cast<OsuApiHelper.OsuMode>();

            bool success = LoadSettings();
            if (success)
            {
                ApplySettingsToApp(Settings);

                if (OsuApiHelper.OsuApi.IsKeyValid() && OsuApiHelper.OsuApi.IsUserValid(Settings["api"]["user"]))
                {
                    if (osuUser == null)
                        osuUser = OsuApiHelper.OsuApi.GetUser(Settings["api"]["user"], (OsuApiHelper.OsuMode)WindowManager.Instance.MainWin.dropdownGameMode.SelectedIndex);

                    CurrentSession = new Session()
                    {
                        InitialData = SessionData.FromUser(osuUser)
                        //StartDataTotalScore = Convert.ToInt64(osuUser.TotalScore),
                        //StartDataRankedScore = Convert.ToInt64(osuUser.RankedScore),
                        //StartDataPlaycount = osuUser.Playcount,
                        //StartDataSSCount = osuUser.GetCountRankSS(),
                        //StartDataSCount = osuUser.GetCountRankS(),
                        //StartDataACount = osuUser.GetCountRankA()
                    };

                }

                RefreshTimer(null, null);
                //Update every minute
                timer = new DispatcherTimer(DispatcherPriority.SystemIdle);
                timer.Tick += new EventHandler(RefreshTimer);
                timer.Interval = TimeSpan.FromSeconds(30);
                timer.Start();

                lastTimerFire = DateTimeOffset.Now.ToUnixTimeSeconds();

                progressTimer = new DispatcherTimer(DispatcherPriority.SystemIdle);
                progressTimer.Tick += new EventHandler((object s, EventArgs e) =>
                {
                    double interval = timer.Interval.TotalSeconds;
                    double secondsPassed = DateTimeOffset.Now.ToUnixTimeSeconds() - lastTimerFire;
                    WindowManager.Instance.DisplayWin.NextUpdateProgress.SetPercent((lastTimerFire == -1 ? 0 : (secondsPassed / interval)));
                });
                progressTimer.Interval = new TimeSpan(0, 0, 1);
                progressTimer.Start();

                updateTimer = new DispatcherTimer(DispatcherPriority.SystemIdle);
                updateTimer.Tick += new EventHandler((object s, EventArgs e) => Update());
                updateTimer.Interval = TimeSpan.FromMilliseconds(1 / 60);
                updateTimer.Start();
            }
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

            WindowManager.Instance.DisplayWin?.Close();
            WindowManager.Instance.DisplayWin = null;
        }

        public void Update()
        {
            if (WindowManager.Instance.DisplayWin != null && CurrentSession != null)
            {
                WindowManager.Instance.DisplayWin.LabelTimeAgoStarted.Content = "Session started " +
                    DateTimeOffset.FromUnixTimeSeconds(CurrentSession.SessionDate).UtcDateTime.Humanize();
            }
        }

        public void RefreshTimer(object sender, EventArgs e)
        {
            if (WindowManager.Instance.DisplayWin != null && CurrentSession != null && osuUser != null)
            {
                bool apiReady = OsuApiHelper.APIHelper<string>.GetDataFromWeb("https://osu.ppy.sh/api/get_user?k=" + Settings["api"]["key"] + "&u=peppy") != "";
                if (!apiReady)
                {
                    WindowManager.Instance.DisplayWin.SetDisplay(DisplayWindow.DisplayType.BanchoDown);
                }
                else
                {
                    WindowManager.Instance.DisplayWin.SetDisplay(DisplayWindow.DisplayType.Stats);
                    bool _continue = true;

                    if (_continue)
                    {
                        osuUser = OsuApiHelper.OsuApi.GetUser(osuUser.Name, (OsuApiHelper.OsuMode)WindowManager.Instance.MainWin.dropdownGameMode.SelectedIndex);

                        CurrentSession.CurrentData = SessionData.FromUser(osuUser);
                        CurrentSession.DifferenceData = SessionData.CalculateDifference(CurrentSession.CurrentData, CurrentSession.InitialData);

                        WindowManager.Instance.DisplayWin.SetCurrentA(osuUser.GetCountRankA());
                        WindowManager.Instance.DisplayWin.SetCurrentS(osuUser.GetCountRankS());
                        WindowManager.Instance.DisplayWin.SetCurrentSS(osuUser.GetCountRankSS());

                        WindowManager.Instance.DisplayWin.SetCurrentScore((settings["display"]["useRankedScore"] == "0" ? Convert.ToInt64(osuUser.TotalScore) : Convert.ToInt64(osuUser.RankedScore)));
                        WindowManager.Instance.DisplayWin.SetCurrentPlaycount(osuUser.Playcount);

                        //int diffTotalSS = osuUser.GetCountRankSS() - CurrentSession.StartDataSSCount;
                        //int diffS = osuUser.GetCountRankS() - CurrentSession.StartDataSCount;
                        //int diffA = osuUser.GetCountRankA() - CurrentSession.StartDataACount;
                        //long diffScore = (settings["display"]["useRankedScore"] == "0" ?
                        //    Convert.ToInt64(osuUser.TotalScore) - CurrentSession.StartDataTotalScore :
                        //    Convert.ToInt64(osuUser.RankedScore) - CurrentSession.StartDataRankedScore);
                        //int diffPC = osuUser.Playcount - CurrentSession.StartDataPlaycount;

                        //DirectoryInfo di = Directory.CreateDirectory("api");
                        //FileStream fs = File.Create("api/ss.txt");
                        //fs.Close();
                        //File.WriteAllText("api/ss.txt", (diffSS>=0?"+":"-") + diffSS);

                        WindowManager.Instance.DisplayWin.SetNewSS(CurrentSession.DifferenceData.RankSilverSS+ CurrentSession.DifferenceData.RankGoldSS);
                        WindowManager.Instance.DisplayWin.SetNewS(CurrentSession.DifferenceData.RankSilverS + CurrentSession.DifferenceData.RankGoldS);
                        WindowManager.Instance.DisplayWin.SetNewA(CurrentSession.DifferenceData.RankA);
                        WindowManager.Instance.DisplayWin.SetNewScore((settings["display"]["useRankedScore"] == "0"? CurrentSession.DifferenceData.TotalScore: CurrentSession.DifferenceData.RankedScore));
                        WindowManager.Instance.DisplayWin.SetNewPlaycount(CurrentSession.DifferenceData.Playcount);

                        long averageScore = (CurrentSession.DifferenceData.Playcount > 0 ? ((settings["display"]["useRankedScore"] == "0" ? CurrentSession.DifferenceData.TotalScore : CurrentSession.DifferenceData.RankedScore) / CurrentSession.DifferenceData.Playcount) : 0);
                        WindowManager.Instance.DisplayWin.SetAverageScore(averageScore);
                    }

                    if (sender != null)
                    {
                        lastTimerFire = DateTimeOffset.Now.ToUnixTimeSeconds();
                    }
                }
            }
        }

        public bool LoadSettings()
        {
            FileIniDataParser parser = new FileIniDataParser();
            if (File.Exists("Settings.ini"))
            {
                IniData data = parser.ReadFile("Settings.ini");
                data = FixIniData(parser, data);
                OsuApiHelper.OsuApiKey.Key = data["api"]["key"];
                osuUser = OsuApiHelper.OsuApi.GetUser(data["api"]["user"], (OsuApiHelper.OsuMode)WindowManager.Instance.MainWin.dropdownGameMode.SelectedIndex);
                //labelColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(data["display"]["labelColor"]);
                //backgroundColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(data["display"]["background"]);
                WindowManager.Instance.MainWin.keyColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(data["display"]["chromakeyBackground"]);
                WindowManager.Instance.MainWin.labelColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(data["display"]["labelColor"]);
                WindowManager.Instance.MainWin.backgroundColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(data["display"]["background"]);
                WindowManager.Instance.MainWin.boolUseChromaKey.IsChecked = data["display"]["useChromaKey"] == "1";
                WindowManager.Instance.MainWin.boolAlwaysOnTop.IsChecked = data["display"]["alwaysOnTop"] == "1";
                WindowManager.Instance.MainWin.boolShowRankedScore.IsChecked = data["display"]["useRankedScore"] == "1";
                WindowManager.Instance.MainWin.labelFontDropdown.Text = data["display"]["labelFont"];

                WindowManager.Instance.MainWin.inputApiKey.Password = data["api"]["key"];
                WindowManager.Instance.MainWin.inputUserName.Text = data["api"]["user"];
                WindowManager.Instance.MainWin.dropdownGameMode.SelectedIndex = (int)Enum.Parse(typeof(OsuApiHelper.OsuMode),data["api"]["gamemode"]);

                Settings = data;
                return true;
            }
            else
            {
                IniData newData = new IniData();

                newData = FixIniData(parser, newData);
                parser.WriteFile("Settings.ini", newData);

                MessageBoxResult result = MessageBox.Show("No settings file was present yet. Generated one. Please enter leftover values in the file.\nPress 'OK' to open the settings location.", "No settings file", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        Arguments = Directory.GetCurrentDirectory(),
                        FileName = "explorer.exe"
                    });
                }
                WindowManager.Instance.MainWin.Close();
                return false;
            }
        }

        private IniData FixIniData(FileIniDataParser parser, IniData data)
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
            WindowManager.Instance.DisplayWin = new DisplayWindow();
            WindowManager.Instance.DisplayWin.Show();
            WindowManager.Instance.DisplayWin.Focus();
            //ApplySettingsToApp();
            RefreshTimer(null, null);
        }

        public void FocusDisplay()
        {
            WindowManager.Instance.DisplayWin.Focus();
        }

        public void CloseDisplay()
        {
            WindowManager.Instance.DisplayWin?.Close();
        }

        public void SettingsSave()
        {
            FileIniDataParser parser = new FileIniDataParser();
            IniData data = parser.ReadFile("Settings.ini");

            data["display"]["labelColor"] = WindowManager.Instance.MainWin.labelColorPicker.SelectedColor.ToString();
            data["display"]["background"] = WindowManager.Instance.MainWin.backgroundColorPicker.SelectedColor.ToString();
            data["display"]["labelFont"] = WindowManager.Instance.MainWin.labelFontDropdown.Text;
            data["display"]["chromakeyBackground"] = WindowManager.Instance.MainWin.keyColorPicker.SelectedColor.ToString();
            data["display"]["useChromaKey"] = (bool)WindowManager.Instance.MainWin.boolUseChromaKey.IsChecked ? "1" : "0";
            data["display"]["alwaysOnTop"] = (bool)WindowManager.Instance.MainWin.boolAlwaysOnTop.IsChecked ? "1" : "0";
            data["display"]["useRankedScore"] = (bool)WindowManager.Instance.MainWin.boolShowRankedScore.IsChecked ? "1" : "0";

            data["api"]["key"] = WindowManager.Instance.MainWin.inputApiKey.Password;
            data["api"]["user"] = WindowManager.Instance.MainWin.inputUserName.Text;
            data["api"]["gamemode"] = "" + ((OsuApiHelper.OsuMode)WindowManager.Instance.MainWin.dropdownGameMode.SelectedIndex);

            parser.WriteFile("Settings.ini", data);

            Settings = data;
            ApplySettingsToApp(Settings);
        }

        private void ApplySettingsToApp(IniData data)
        {
            WindowManager.Instance.DisplayWin?.Close();
            WindowManager.Instance.DisplayWin = new DisplayWindow();
            WindowManager.Instance.DisplayWin.AllowsTransparency = data["display"]["useChromaKey"] != "1";
            WindowManager.Instance.DisplayWin.Show();
            WindowManager.Instance.DisplayWin.Focus();

            if (data["display"]["useChromaKey"] == "1")
            {
                Brush keyColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(data["display"]["chromakeyBackground"]));
                WindowManager.Instance.DisplayWin.Background = keyColor;
            }
            else
            {
                WindowManager.Instance.DisplayWin.Background = new SolidColorBrush(Colors.Transparent);
            }

            if (OsuApiHelper.OsuApiKey.Key != WindowManager.Instance.MainWin.inputApiKey.Password || !osuUser.Name.Equals(WindowManager.Instance.MainWin.inputUserName.Text, StringComparison.CurrentCultureIgnoreCase))
            {
                OsuApiHelper.OsuApiKey.Key = WindowManager.Instance.MainWin.inputApiKey.Password;
                //osuUser = OsuApiHelper.OsuApi.GetUser(MainWin.inputUserName.Text, OsuApiHelper.OsuMode.Standard);

                RefreshSession();
            }
            else
                RefreshTimer(null, null);

            InterfaceManager.Instance.SetLabelFont((FontFamily)new FontFamilyConverter().ConvertFromString(data["display"]["labelFont"]));
            InterfaceManager.Instance.SetLabelColor((Color)ColorConverter.ConvertFromString(data["display"]["labelColor"]));

            WindowManager.Instance.DisplayWin.LabelUsername.Content = osuUser.Name;

            string profilePic = @"https://a.ppy.sh/" + osuUser.ID;
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(profilePic, UriKind.Absolute);
            bitmap.EndInit();

            WindowManager.Instance.DisplayWin.imageProfilePicture.Source = bitmap;

            WindowManager.Instance.DisplayWin.RoundedBackground.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(data["display"]["background"]));
        }

        public void RefreshSession()
        {
            if (OsuApiHelper.OsuApi.IsKeyValid() && OsuApiHelper.OsuApi.IsUserValid(Settings["api"]["user"]))
            {
                osuUser = OsuApiHelper.OsuApi.GetUser(Settings["api"]["user"], (OsuApiHelper.OsuMode)WindowManager.Instance.MainWin.dropdownGameMode.SelectedIndex);

                CurrentSession = new Session()
                {
                    InitialData = SessionData.FromUser(osuUser)
                    //StartDataTotalScore = Convert.ToInt64(osuUser.TotalScore),
                    //StartDataRankedScore = Convert.ToInt64(osuUser.RankedScore),
                    //StartDataPlaycount = osuUser.Playcount,
                    //StartDataSSCount = osuUser.GetCountRankSS(),
                    //StartDataSCount = osuUser.GetCountRankS(),
                    //StartDataACount = osuUser.GetCountRankA()
                };

                RefreshTimer(null, null);
            }
        }
    }
}
