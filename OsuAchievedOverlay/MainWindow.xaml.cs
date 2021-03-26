using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using IniParser;
using IniParser.Model;
using Microsoft.Win32;
using Newtonsoft.Json;
using TimeAgo;

namespace OsuAchievedOverlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Display displayWin = null;
        private OsuApiHelper.OsuUser osuUser = null;

        private Session currentSession;
        private IniData settings;

        private DispatcherTimer timer;
        private DispatcherTimer progressTimer;
        private DispatcherTimer updateTimer;

        private long lastTimerFire = -1;

        private IniData DefaultSettings{
            get{
                return new IniData()
                {
                    ["api"] = {
                        ["key"] = "No key inserted",
                        ["user"] = "Username here",
                        ["updateRate"] = "60"
                    },
                    ["display"] = {
                        ["labelColor"] = Colors.Black.ToString(),
                        ["background"] = Colors.White.ToString(),
                        ["chromakeyBackground"] = Colors.Green.ToString(),
                        ["useChromaKey"] = "1"
                    }
                };
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            Closed += (object sender, EventArgs e) =>
            {
                timer?.Stop();
                timer = null;

                progressTimer?.Stop();
                progressTimer = null;

                updateTimer?.Stop();
                updateTimer = null;

                displayWin?.Close();
                displayWin = null;

                Environment.Exit(0);
            };

            Start();
        }

        public void Start()
        {
            bool success = LoadSettings();

            if (success)
            {
                ApplySettingsToApp(settings);

                if (OsuApiHelper.OsuApi.IsKeyValid() && OsuApiHelper.OsuApi.IsUserValid(settings["api"]["user"]))
                {
                    if (osuUser == null)
                        osuUser = OsuApiHelper.OsuApi.GetUser(settings["api"]["user"], OsuApiHelper.OsuMode.Standard);

                    currentSession = new Session()
                    {
                        StartDataScore = Convert.ToInt64(osuUser.TotalScore),
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
                    displayWin.NextUpdateProgress.SetPercent((lastTimerFire==-1?0:(secondsPassed / interval)));
                });
                progressTimer.Interval = new TimeSpan(0, 0, 1);
                progressTimer.Start();

                updateTimer = new DispatcherTimer(DispatcherPriority.SystemIdle);
                updateTimer.Tick += new EventHandler((object s, EventArgs e) => Update());
                updateTimer.Interval = TimeSpan.FromMilliseconds(1/60);
                updateTimer.Start();
            }
        }

        public void Update(){
            if (displayWin != null && currentSession!=null){
                displayWin.LabelTimeAgoStarted.Content = "Session started "+DateTimeOffset.FromUnixTimeSeconds(currentSession.SessionDate).UtcDateTime.AddHours(1).TimeAgo();
            }
        }
        private void RefreshTimer(object sender, EventArgs e)
        {
            if (displayWin != null && currentSession!=null && osuUser!=null)
            {
                bool _continue = true;
                //if (!OsuApiHelper.OsuApi.IsKeyValid())
                //{
                //    CloseDisplay();
                //    _continue = false;
                //    //MessageBoxResult result = MessageBox.Show("Provided API key is invalid", "Error", MessageBoxButton.OK);
                //}

                //if (!OsuApiHelper.OsuApi.IsUserValid(settings["api"]["user"]))
                //{
                //    CloseDisplay();
                //    _continue = false;
                //    //MessageBoxResult result = MessageBox.Show("No account with that username exists", "Error", MessageBoxButton.OK);
                //}

                if (_continue)
                {
                    osuUser = OsuApiHelper.OsuApi.GetUser(osuUser.Name, OsuApiHelper.OsuMode.Standard);

                    displayWin.SetCurrentA(osuUser.GetCountRankA());
                    displayWin.SetCurrentS(osuUser.GetCountRankS());
                    displayWin.SetCurrentSS(osuUser.GetCountRankSS());

                    displayWin.SetCurrentScore(Convert.ToInt64(osuUser.TotalScore));
                    displayWin.SetCurrentPlaycount(osuUser.Playcount);

                    int diffSS = osuUser.GetCountRankSS() - currentSession.StartDataSSCount;
                    int diffS = osuUser.GetCountRankS() - currentSession.StartDataSCount;
                    int diffA = osuUser.GetCountRankA() - currentSession.StartDataACount;
                    long diffScore = Convert.ToInt64(osuUser.TotalScore) - currentSession.StartDataScore;
                    int diffPC = osuUser.Playcount - currentSession.StartDataPlaycount;

                    displayWin.SetNewSS(diffSS);
                    displayWin.SetNewS(diffS);
                    displayWin.SetNewA(diffA);
                    displayWin.SetNewScore(diffScore);
                    displayWin.SetNewPlaycount(diffPC);
                }

                if (sender != null)
                {
                    lastTimerFire = DateTimeOffset.Now.ToUnixTimeSeconds();
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
                osuUser = OsuApiHelper.OsuApi.GetUser(data["api"]["user"], OsuApiHelper.OsuMode.Standard);
                //labelColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(data["display"]["labelColor"]);
                //backgroundColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(data["display"]["background"]);
                keyColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(data["display"]["chromakeyBackground"]);
                labelColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(data["display"]["labelColor"]);
                backgroundColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(data["display"]["background"]);
                boolUseChromaKey.IsChecked = data["display"]["useChromaKey"] == "1";

                inputApiKey.Password = data["api"]["key"];
                inputUserName.Text = data["api"]["user"];

                settings = data;
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
                this.Close();
                return false;
            }
        }

        private IniData FixIniData(FileIniDataParser parser, IniData data){
            foreach(SectionData section in DefaultSettings.Sections){
                foreach(KeyData key in section.Keys){
                    if (data[section.SectionName][key.KeyName] == null)
                        data[section.SectionName][key.KeyName] = key.Value;
                }
            }

            parser.WriteFile("Settings.ini", data);
            return data;
        }

        public void OpenDisplay(bool closeCheck = true)
        {
            if(closeCheck)
                CloseDisplay();
            displayWin = new Display();
            if (Display.displayPosition != null)
            {
                displayWin.WindowStartupLocation = WindowStartupLocation.Manual;
                displayWin.Left = ((Vector)Display.displayPosition).X;
                displayWin.Top = ((Vector)Display.displayPosition).Y;
            }
            displayWin.Show();
            displayWin.Focus();
            //ApplySettingsToApp();
            RefreshTimer(null, null);
        }

        public void CloseDisplay()
        {
            displayWin?.Close();
        }

        private void ButtonHandler_OpenDisplay(object sender, RoutedEventArgs e)
        {
            OpenDisplay();
        }

        private void SettingsSave(object sender, RoutedEventArgs e)
        {
            FileIniDataParser parser = new FileIniDataParser();
            IniData data = parser.ReadFile("Settings.ini");

            data["display"]["labelColor"] = labelColorPicker.SelectedColor.ToString();
            data["display"]["background"] = backgroundColorPicker.SelectedColor.ToString();
            data["display"]["chromakeyBackground"] = keyColorPicker.SelectedColor.ToString();
            data["display"]["useChromaKey"] = (bool)boolUseChromaKey.IsChecked ? "1" : "0";

            data["api"]["key"] = inputApiKey.Password;
            data["api"]["user"] = inputUserName.Text;

            parser.WriteFile("Settings.ini", data);

            settings = data;
            ApplySettingsToApp(settings);
        }

        private void ApplySettingsToApp(IniData data){
            Brush labelColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(data["display"]["labelColor"]));

            displayWin?.Close();
            displayWin = new Display();
            displayWin.AllowsTransparency = data["display"]["useChromaKey"] != "1";
            displayWin.Show();
            displayWin.Focus();
            RefreshTimer(null, null);

            displayWin.LabelACurrent.Foreground = labelColor;
            displayWin.LabelSCurrent.Foreground = labelColor;
            displayWin.LabelSSCurrent.Foreground = labelColor;

            displayWin.LabelANew.Foreground = labelColor;
            displayWin.LabelSNew.Foreground = labelColor;
            displayWin.LabelSSNew.Foreground = labelColor;

            displayWin.LabelPlaycountCurrent.Foreground = labelColor;
            displayWin.LabelPlaycountNew.Foreground = labelColor;
            displayWin.LabelScoreCurrent.Foreground = labelColor;
            displayWin.LabelScoreNew.Foreground = labelColor;

            displayWin.LabelTimeAgoStarted.Foreground = labelColor;

            displayWin.labelUsername.Content = osuUser.Name;

            string profilePic = @"https://a.ppy.sh/"+osuUser.ID;
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(profilePic, UriKind.Absolute);
            bitmap.EndInit();

            displayWin.imageProfilePicture.Source = bitmap;

            displayWin.RoundedBackground.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(data["display"]["background"]));

            if (data["display"]["useChromaKey"] == "1"){
                Brush keyColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(data["display"]["chromakeyBackground"]));
                displayWin.Background = keyColor;
            }
            else
            {
                displayWin.Background = new SolidColorBrush(Colors.Transparent);
            }

            OsuApiHelper.OsuApiKey.Key = inputApiKey.Password;
            osuUser = OsuApiHelper.OsuApi.GetUser(inputUserName.Text, OsuApiHelper.OsuMode.Standard);

        }

        private void RefreshSession(object sender, RoutedEventArgs e)
        {
            if (OsuApiHelper.OsuApi.IsKeyValid() && OsuApiHelper.OsuApi.IsUserValid(settings["api"]["user"]))
            {
                osuUser = OsuApiHelper.OsuApi.GetUser(settings["api"]["user"], OsuApiHelper.OsuMode.Standard);

                currentSession = new Session()
                {
                    StartDataScore = Convert.ToInt64(osuUser.TotalScore),
                    StartDataPlaycount = osuUser.Playcount,
                    StartDataSSCount = osuUser.GetCountRankSS(),
                    StartDataSCount = osuUser.GetCountRankS(),
                    StartDataACount = osuUser.GetCountRankA()
                };

                RefreshTimer(null, null);
            }
        }

        private void btnSaveSession_Click(object sender, RoutedEventArgs e)
        {
            string json = currentSession.ConvertToJson();

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt";
            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, json);
        }

        private void btnLoadSession_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true){
                //File.WriteAllText(saveFileDialog.FileName, json);
                var fileStream = openFileDialog.OpenFile();

                using (StreamReader reader = new StreamReader(fileStream))
                {
                    string data = reader.ReadToEnd();
                    if(data.Length>0){
                        Session newSession = null;
                        bool validSession = true;
                        try{
                            newSession = JsonConvert.DeserializeObject<Session>(data);
                        }catch (Exception)
                        {
                            validSession = false;
                            MessageBox.Show("Seems like the opened file is an invalid session file.", "Error opening session", MessageBoxButton.OK);
                        }
                        if (validSession)
                            currentSession = newSession;
                    }
                }
            }
        }
    }
}
