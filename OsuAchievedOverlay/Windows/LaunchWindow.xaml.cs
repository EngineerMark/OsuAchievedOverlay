using IniParser;
using IniParser.Model;
using Newtonsoft.Json;
using OsuAchievedOverlay.Github;
using OsuAchievedOverlay.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Resources;

namespace OsuAchievedOverlay
{
    public partial class LaunchWindow : Window
    {
        public LaunchWindow(StartupEventArgs e)
        {
            InitializeComponent();
            Show();
            Focus();

#if DEBUG
            TestSettings();
#else

            if(e.Args.Contains("-osufinishupdate")){
                if (Directory.Exists("temp")){
                    bool deleted = false;
                    while(!deleted){
                        try{
                            Directory.Delete("temp", true);
                            deleted = true;
                        }
                        catch(IOException _e){
                            deleted = false;
                        }
                    }
                }
            }

            if (e.Args.Contains("-osustartupdate"))
            {
                WindowManager.Instance.UpdateWin = new Windows.UpdateWindow(true);
                WindowManager.Instance.UpdateWin.Show();
            }
            else if (e.Args.Contains("-osunoupdate"))
                TestSettings();
            else
                CheckForUpdate();
#endif
            //TestSettings();
        }

        public void CheckForUpdate()
        {
            ThreadPool.QueueUserWorkItem((Object stateInfo) =>
            {
                List<Release> updates = UpdateManager.Instance.GetAvailableUpdates();
                bool updateReady = updates.Count > 0;

                Dispatcher.Invoke(new Action(() =>
                {
                    if (updateReady)
                    {
                        WindowManager.Instance.UpdateWin = new Windows.UpdateWindow();
                        WindowManager.Instance.UpdateWin.Show();
                        WindowManager.Instance.UpdateWin.SetReleaseData(updates);
                    }
                    else
                        TestSettings();
                }));
            });
        }

        public void TestSettings()
        {
            FileIniDataParser parser = new FileIniDataParser();
            if (File.Exists("Settings.ini"))
            {
                IniData data = parser.ReadFile("Settings.ini");
                string key = data["api"]["key"];
                OsuApiHelper.OsuApiKey.Key = key;
                if (!OsuApiHelper.OsuApi.IsKeyValid())
                {
                    OpenPopup();
                }
                else
                {
                    // API key is valid, lets head on further

                    GameManager.Instance.Start();

                    Close();
                }
            }
            else
            {
                IniData newData = new IniData();

                newData = SettingsManager.FixIniData(parser, newData);
                parser.WriteFile("Settings.ini", newData);

                OpenPopup();
            }
        }

        private void OpenPopup()
        {
            PopupSetAPI popup = new PopupSetAPI();
            popup.Show();
            popup.Focus();
        }
    }
}
