using IniParser;
using IniParser.Model;
using System;
using System.IO;

namespace OsuAchievedOverlay.Managers
{
    public class SettingsManager : Manager<SettingsManager>
    {
        private IniData settings;
        public IniData Settings { get => settings; set => settings = value; }

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
                    }
                };
            }
        }

        public override void Start()
        {
        }

        public override void Stop()
        {
        }

        public bool LoadSettings()
        {
            FileIniDataParser parser = new FileIniDataParser();
            if (File.Exists("Settings.ini"))
            {
                IniData data = parser.ReadFile("Settings.ini");
                data = FixIniData(parser, data);
                OsuApiHelper.OsuApiKey.Key = data["api"]["key"];
                GameManager.Instance.OsuUser = OsuApiHelper.OsuApi.GetUser(data["api"]["user"], (OsuApiHelper.OsuMode)Enum.Parse(typeof(OsuApiHelper.OsuMode), data["api"]["gamemode"]));

                int updateRate = int.Parse(data["api"]["updateRate"]);
                updateRate = Math.Min(120, Math.Max(5, updateRate));
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

            parser.WriteFile("Settings.ini", data);
            return data;
        }

        public void SettingsSave()
        {
            FileIniDataParser parser = new FileIniDataParser();
            parser.WriteFile("Settings.ini", Settings);

            GameManager.Instance.ApplySettingsToApp(Settings);
        }
    }
}
