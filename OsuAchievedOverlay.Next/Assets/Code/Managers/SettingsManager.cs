using IniParser;
using IniParser.Model;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OsuAchievedOverlay.Managers
{
    public class SettingsManager : Manager
    {
        private static IniData settings;
        public static IniData Settings { get => settings; set => settings = value; }

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

        public static void SaveSettings(){
            FileIniDataParser parser = new FileIniDataParser();
            parser.WriteFile(Path.Combine(FileManager.GetExecutableDirectory(), "settings.ini"), Settings);
        }

        public bool LoadSettings()
        {
            FileIniDataParser parser = new FileIniDataParser();
            if (File.Exists(Path.Combine(FileManager.GetExecutableDirectory(), "settings.ini")))
            {
                IniData data = parser.ReadFile(Path.Combine(FileManager.GetExecutableDirectory(), "settings.ini"));
                data = FixIniData(parser, data);
                OsuApiHelper.OsuApiKey.Key = data["api"]["key"];
                //GameManager.Instance.OsuUser = OsuApiHelper.OsuApi.GetUser(data["api"]["user"], (OsuApiHelper.OsuMode)Enum.Parse(typeof(OsuApiHelper.OsuMode), data["api"]["gamemode"]));

                int updateRate = int.Parse(data["api"]["updateRate"]);
                updateRate = Mathf.Min(120, Mathf.Max(5, updateRate));
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

            parser.WriteFile(Path.Combine(FileManager.GetExecutableDirectory(), "settings.ini"), data);
            return data;
        }

        public override void Start()
        {
        }

        public override void Stop()
        {
        }
    }
}
