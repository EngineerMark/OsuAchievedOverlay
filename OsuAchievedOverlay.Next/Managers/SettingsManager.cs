using IniParser;
using IniParser.Model;
using OsuAchievedOverlay.Next.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Next.Managers
{
    public class SettingsManager : Singleton<SettingsManager>
    {
        private IniData settings;
        public IniData Settings { get => settings; set => settings = value; }

        public static int RefreshTimeMin = 5;
        public static int RefreshTimeMax = 120;

        public static int RoundingMin = 0;
        public static int RoundingMax = 10;

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
                    ["misc"] = {
                        ["osuFolder"] = "C:\\"
                    },
                    ["display"] = {
                        ["roundingValue"] = "1"
                    },
                    ["showingItems"] = {
                        ["ranks"] = "true",
                        ["level"] = "true",
                        ["totalscore"] = "true",
                        ["rankedscore"] = "true",
                        ["worldrank"] = "true",
                        ["countryrank"] = "true",
                        ["playcount"] = "true",
                        ["playtime"] = "true",
                        ["accuracy"] = "true",
                        ["performance"] = "true",
                    }
                };
            }
        }
        public const string SettingsLocation = "Data/Settings.ini";

        public void FixSettingsPath()
        {
            if (File.Exists("Settings.ini"))
            {
                while (!FileManager.IsFileReady("Settings.ini"))
                {
                    Thread.Sleep(1);
                }
                FileManager.MoveFile("Settings.ini", SettingsLocation);
            }
        }

        public bool LoadOrCreateSettings()
        {
            if (!LoadSettings())
            {
                FileIniDataParser parser = new FileIniDataParser();
                parser.WriteFile(SettingsLocation, DefaultSettings);
                Settings = DefaultSettings;
                return false;
            }
            return true;
        }

        public bool LoadSettings()
        {
            FileIniDataParser parser = new FileIniDataParser();
            if (File.Exists(SettingsLocation))
            {
                IniData data = parser.ReadFile(SettingsLocation);
                data = FixIniData(parser, data);
                OsuApiHelper.OsuApiKey.Key = data["api"]["key"];
                //GameManager.Instance.OsuUser = OsuApiHelper.OsuApi.GetUser(data["api"]["user"], (OsuApiHelper.OsuMode)Enum.Parse(typeof(OsuApiHelper.OsuMode), data["api"]["gamemode"]));

                int updateRate = int.Parse(data["api"]["updateRate"]);
                updateRate = Math.Min(RefreshTimeMax, Math.Max(RefreshTimeMin, updateRate));
                data["api"]["updateRate"] = "" + updateRate;

                Settings = data;
                return true;
            }
            return false;
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

            parser.WriteFile(SettingsLocation, data);
            return data;
        }

        public void SettingsApply()
        {
            KeyDataCollection displayOptions = Settings["showingItems"];
            foreach (KeyData keyData in displayOptions)
            {
                string key = keyData.KeyName;
                bool state = keyData.Value == "true";
                if(state){
                    BrowserViewModel.Instance.AttachedJavascriptWrapper.Show("#sessionDisplay" + (key.FirstCharToUpper()) + "");
                }else{
                    BrowserViewModel.Instance.AttachedJavascriptWrapper.Hide("#sessionDisplay" + (key.FirstCharToUpper()) + "");
                }
                //BrowserViewModel.Instance.AttachedJavascriptWrapper.Checkbox.SetChecked("#settingsInputDisplay" + (key.FirstCharToUpper()) + "", keyData.Value == "true");
            }
        }

        public void SettingsSave()
        {
            FileIniDataParser parser = new FileIniDataParser();
            parser.WriteFile(SettingsLocation, Settings);

            //GameManager.Instance.ApplySettingsToApp(Settings);
        }
    }
}
