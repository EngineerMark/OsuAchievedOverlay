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

namespace OsuAchievedOverlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Display displayWin = null;
        private OsuApiHelper.OsuUser osuUser = null;

        private long StartDataScore = -1;
        private int StartDataPlaycount = -1;
        private int StartDataSSCount = -1;
        private int StartDataSCount = -1;
        private int StartDataACount = -1;

        private DispatcherTimer timer;
        private DispatcherTimer progressTimer;

        private long lastTimerFire = -1;

        public MainWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Minimized;

            Closed += (object sender, EventArgs e) =>
            {
                timer.Stop();
                timer = null;

                progressTimer.Stop();
                progressTimer = null;

                CloseDisplay();
            };

            Start();
        }

        public void Start()
        {
            bool success = LoadSettings();

            if (success)
            {
                OpenDisplay();

                StartDataScore = Convert.ToInt64(osuUser.TotalScore);
                StartDataPlaycount = osuUser.Playcount;
                StartDataSSCount = osuUser.GetCountRankSS();
                StartDataSCount = osuUser.GetCountRankS();
                StartDataACount = osuUser.GetCountRankA();

                RefreshTimer(null, null);

                //Update every minute
                timer = new DispatcherTimer(DispatcherPriority.SystemIdle);
                timer.Tick += new EventHandler(RefreshTimer);
                timer.Interval = TimeSpan.FromSeconds(60);
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
            }
        }

        private void RefreshTimer(object sender, EventArgs e)
        {
            if (displayWin != null)
            {
                osuUser = OsuApiHelper.OsuApi.GetUser(osuUser.Name, OsuApiHelper.OsuMode.Standard);

                displayWin.SetCurrentA(osuUser.GetCountRankA().ToString("#,##0.###"));
                displayWin.SetCurrentS(osuUser.GetCountRankS().ToString("#,##0.###"));
                displayWin.SetCurrentSS(osuUser.GetCountRankSS().ToString("#,##0.###"));

                displayWin.SetCurrentScore(Convert.ToInt64(osuUser.TotalScore).ToString("#,##0.###"));
                displayWin.SetCurrentPlaycount(osuUser.Playcount.ToString("#,##0.###"));

                int diffSS = osuUser.GetCountRankSS() - StartDataSSCount;
                int diffS = osuUser.GetCountRankS() - StartDataSCount;
                int diffA = osuUser.GetCountRankA() - StartDataACount;
                long diffScore = Convert.ToInt64(osuUser.TotalScore) - StartDataScore;
                int diffPC = osuUser.Playcount - StartDataPlaycount;

                displayWin.SetNewSS(diffSS.ToString("#,##0.###"));
                displayWin.SetNewS(diffS.ToString("#,##0.###"));
                displayWin.SetNewA(diffA.ToString("#,##0.###"));
                displayWin.SetNewScore(diffScore.ToString("#,##0.###"));
                displayWin.SetNewPlaycount(diffPC.ToString("#,##0.###"));

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
                labelColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(data["display"]["labelColor"]);
                backgroundColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(data["display"]["background"]);

                return true;
            }
            else
            {
                IniData newData = new IniData();
                newData["api"]["key"] = "No key inserted";
                newData["api"]["user"] = "Username here";

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
            if (data["display"]["labelColor"] == null)
                data["display"]["labelColor"] = Colors.Black.ToString();
            if (data["display"]["background"] == null)
                data["display"]["background"] = Colors.White.ToString();

            parser.WriteFile("Settings.ini", data);
            return data;
        }

        public void OpenDisplay()
        {
            if (displayWin == null || !displayWin.IsLoaded)
            {
                CloseDisplay();
                displayWin = new Display();
                displayWin.Show();
            }
            else if (displayWin.WindowState == WindowState.Minimized)
            {
                displayWin.WindowState = WindowState.Normal;
            }
            displayWin.Focus();
            ApplySettingsToApp();
            RefreshTimer(null, null);
        }

        public void CloseDisplay()
        {
            if (displayWin != null)
                displayWin.Close();
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

            parser.WriteFile("Settings.ini", data);

            ApplySettingsToApp();
        }

        private void ApplySettingsToApp(){
            FileIniDataParser parser = new FileIniDataParser();
            IniData data = parser.ReadFile("Settings.ini");

            Brush labelColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(data["display"]["labelColor"]));

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

            displayWin.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(data["display"]["background"]));
        }
    }
}
