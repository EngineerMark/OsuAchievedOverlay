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
        private DisplayWindow displayWin = null;
        private MainWindow mainWin = null;
        private LoadSessionWindow sessionWin = null;
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
                    }
                };
            }
        }

        public Session CurrentSession { get => currentSession; set => currentSession = value; }
        public MainWindow MainWin { get => mainWin; set => mainWin = value; }
        public DisplayWindow DisplayWin { get => displayWin; set => displayWin = value; }
        public IniData Settings { get => settings; set => settings = value; }
        public LoadSessionWindow SessionWin { get => sessionWin; set => sessionWin = value; }

        public override void Start()
        {
            MainWin.dropdownGameMode.ItemsSource = Enum.GetValues(typeof(OsuApiHelper.OsuMode)).Cast<OsuApiHelper.OsuMode>();

            bool success = LoadSettings();
            if (success)
            {
                ApplySettingsToApp(Settings);

                if (OsuApiHelper.OsuApi.IsKeyValid() && OsuApiHelper.OsuApi.IsUserValid(Settings["api"]["user"]))
                {
                    if (osuUser == null)
                        osuUser = OsuApiHelper.OsuApi.GetUser(Settings["api"]["user"], (OsuApiHelper.OsuMode)MainWin.dropdownGameMode.SelectedIndex);

                    CurrentSession = new Session()
                    {
                        StartDataTotalScore = Convert.ToInt64(osuUser.TotalScore),
                        StartDataRankedScore = Convert.ToInt64(osuUser.RankedScore),
                        StartDataPlaycount = osuUser.Playcount,
                        StartDataSSCount = osuUser.GetCountRankSS(),
                        StartDataSCount = osuUser.GetCountRankS(),
                        StartDataACount = osuUser.GetCountRankA()
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
                    DisplayWin.NextUpdateProgress.SetPercent((lastTimerFire == -1 ? 0 : (secondsPassed / interval)));
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

            DisplayWin?.Close();
            DisplayWin = null;
        }

        public void Update()
        {
            if (DisplayWin != null && CurrentSession != null)
            {
                DisplayWin.LabelTimeAgoStarted.Content = "Session started " +
                    DateTimeOffset.FromUnixTimeSeconds(CurrentSession.SessionDate).UtcDateTime.Humanize();
            }
        }

        public void RefreshTimer(object sender, EventArgs e)
        {
            if (DisplayWin != null && CurrentSession != null && osuUser != null)
            {
                bool apiReady = OsuApiHelper.APIHelper<string>.GetDataFromWeb("https://osu.ppy.sh/api/get_user?k=" + Settings["api"]["key"] + "&u=peppy") != "";
                if (!apiReady)
                {
                    DisplayWin.SetDisplay(DisplayWindow.DisplayType.BanchoDown);
                }
                else
                {
                    DisplayWin.SetDisplay(DisplayWindow.DisplayType.Stats);
                    bool _continue = true;

                    if (_continue)
                    {
                        osuUser = OsuApiHelper.OsuApi.GetUser(osuUser.Name, (OsuApiHelper.OsuMode)MainWin.dropdownGameMode.SelectedIndex);

                        DisplayWin.SetCurrentA(osuUser.GetCountRankA());
                        DisplayWin.SetCurrentS(osuUser.GetCountRankS());
                        DisplayWin.SetCurrentSS(osuUser.GetCountRankSS());

                        DisplayWin.SetCurrentScore((settings["display"]["useRankedScore"] == "0" ? Convert.ToInt64(osuUser.TotalScore) : Convert.ToInt64(osuUser.RankedScore)));
                        DisplayWin.SetCurrentPlaycount(osuUser.Playcount);

                        int diffSS = osuUser.GetCountRankSS() - CurrentSession.StartDataSSCount;
                        int diffS = osuUser.GetCountRankS() - CurrentSession.StartDataSCount;
                        int diffA = osuUser.GetCountRankA() - CurrentSession.StartDataACount;
                        long diffScore = (settings["display"]["useRankedScore"] == "0" ?
                            Convert.ToInt64(osuUser.TotalScore) - CurrentSession.StartDataTotalScore :
                            Convert.ToInt64(osuUser.RankedScore) - CurrentSession.StartDataRankedScore);
                        int diffPC = osuUser.Playcount - CurrentSession.StartDataPlaycount;

                        DisplayWin.SetNewSS(diffSS);
                        DisplayWin.SetNewS(diffS);
                        DisplayWin.SetNewA(diffA);
                        DisplayWin.SetNewScore(diffScore);
                        DisplayWin.SetNewPlaycount(diffPC);

                        long averageScore = (diffPC > 0 ? (diffScore / diffPC) : 0);
                        DisplayWin.SetAverageScore(averageScore);
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
                osuUser = OsuApiHelper.OsuApi.GetUser(data["api"]["user"], (OsuApiHelper.OsuMode)MainWin.dropdownGameMode.SelectedIndex);
                //labelColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(data["display"]["labelColor"]);
                //backgroundColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(data["display"]["background"]);
                MainWin.keyColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(data["display"]["chromakeyBackground"]);
                MainWin.labelColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(data["display"]["labelColor"]);
                MainWin.backgroundColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(data["display"]["background"]);
                MainWin.boolUseChromaKey.IsChecked = data["display"]["useChromaKey"] == "1";
                MainWin.boolAlwaysOnTop.IsChecked = data["display"]["alwaysOnTop"] == "1";
                MainWin.boolShowRankedScore.IsChecked = data["display"]["useRankedScore"] == "1";
                MainWin.labelFontDropdown.Text = data["display"]["labelFont"];

                MainWin.inputApiKey.Password = data["api"]["key"];
                MainWin.inputUserName.Text = data["api"]["user"];
                MainWin.dropdownGameMode.SelectedIndex = (int)Enum.Parse(typeof(OsuApiHelper.OsuMode),data["api"]["gamemode"]);

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
                MainWin.Close();
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
            DisplayWin = new DisplayWindow();
            DisplayWin.Show();
            DisplayWin.Focus();
            //ApplySettingsToApp();
            RefreshTimer(null, null);
        }

        public void FocusDisplay()
        {
            DisplayWin.Focus();
        }

        public void CloseDisplay()
        {
            DisplayWin?.Close();
        }

        public void SettingsSave()
        {
            FileIniDataParser parser = new FileIniDataParser();
            IniData data = parser.ReadFile("Settings.ini");

            data["display"]["labelColor"] = MainWin.labelColorPicker.SelectedColor.ToString();
            data["display"]["background"] = MainWin.backgroundColorPicker.SelectedColor.ToString();
            data["display"]["labelFont"] = MainWin.labelFontDropdown.Text;
            data["display"]["chromakeyBackground"] = MainWin.keyColorPicker.SelectedColor.ToString();
            data["display"]["useChromaKey"] = (bool)MainWin.boolUseChromaKey.IsChecked ? "1" : "0";
            data["display"]["alwaysOnTop"] = (bool)MainWin.boolAlwaysOnTop.IsChecked ? "1" : "0";
            data["display"]["useRankedScore"] = (bool)MainWin.boolShowRankedScore.IsChecked ? "1" : "0";

            data["api"]["key"] = MainWin.inputApiKey.Password;
            data["api"]["user"] = MainWin.inputUserName.Text;
            data["api"]["gamemode"] = "" + ((OsuApiHelper.OsuMode)MainWin.dropdownGameMode.SelectedIndex);

            parser.WriteFile("Settings.ini", data);

            Settings = data;
            ApplySettingsToApp(Settings);
        }

        private void ApplySettingsToApp(IniData data)
        {
            DisplayWin?.Close();
            DisplayWin = new DisplayWindow();
            DisplayWin.AllowsTransparency = data["display"]["useChromaKey"] != "1";
            DisplayWin.Show();
            DisplayWin.Focus();

            if (data["display"]["useChromaKey"] == "1")
            {
                Brush keyColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(data["display"]["chromakeyBackground"]));
                DisplayWin.Background = keyColor;
            }
            else
            {
                DisplayWin.Background = new SolidColorBrush(Colors.Transparent);
            }

            if (OsuApiHelper.OsuApiKey.Key != MainWin.inputApiKey.Password || !osuUser.Name.Equals(MainWin.inputUserName.Text, StringComparison.CurrentCultureIgnoreCase))
            {
                OsuApiHelper.OsuApiKey.Key = MainWin.inputApiKey.Password;
                //osuUser = OsuApiHelper.OsuApi.GetUser(MainWin.inputUserName.Text, OsuApiHelper.OsuMode.Standard);

                RefreshSession();
            }
            else
                RefreshTimer(null, null);

            InterfaceManager.Instance.SetLabelFont((FontFamily)new FontFamilyConverter().ConvertFromString(data["display"]["labelFont"]));
            InterfaceManager.Instance.SetLabelColor((Color)ColorConverter.ConvertFromString(data["display"]["labelColor"]));

            DisplayWin.LabelUsername.Content = osuUser.Name;

            string profilePic = @"https://a.ppy.sh/" + osuUser.ID;
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(profilePic, UriKind.Absolute);
            bitmap.EndInit();

            DisplayWin.imageProfilePicture.Source = bitmap;

            DisplayWin.RoundedBackground.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(data["display"]["background"]));
        }

        public void RefreshSession()
        {
            if (OsuApiHelper.OsuApi.IsKeyValid() && OsuApiHelper.OsuApi.IsUserValid(Settings["api"]["user"]))
            {
                osuUser = OsuApiHelper.OsuApi.GetUser(Settings["api"]["user"], (OsuApiHelper.OsuMode)MainWin.dropdownGameMode.SelectedIndex);

                CurrentSession = new Session()
                {
                    StartDataTotalScore = Convert.ToInt64(osuUser.TotalScore),
                    StartDataRankedScore = Convert.ToInt64(osuUser.RankedScore),
                    StartDataPlaycount = osuUser.Playcount,
                    StartDataSSCount = osuUser.GetCountRankSS(),
                    StartDataSCount = osuUser.GetCountRankS(),
                    StartDataACount = osuUser.GetCountRankA()
                };

                RefreshTimer(null, null);
            }
        }
    }
}
