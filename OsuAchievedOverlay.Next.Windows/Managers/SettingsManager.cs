using CefSharp;
using IniParser;
using IniParser.Model;
using OsuAchievedOverlay.Next.Helpers;
using OsuApiHelper;
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
                        ["roundingValue"] = "1",
                        ["nsfwMode"] = "false",
                        ["theme"] = "theme_Default"
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
                        ["300x"] = "false",
                        ["100x"] = "false",
                        ["50x"] = "false",
                    }
                };
            }
        }
        public const string DefaultDataFolder = "Data";
        public static string SettingsLocation = Path.Combine(DefaultDataFolder, "Settings.ini");

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

            if (File.Exists("stored_sessions.json"))
            {
                while (!FileManager.IsFileReady("stored_sessions.json"))
                {
                    Thread.Sleep(1);
                }
                FileManager.MoveFile("stored_sessions.json", Path.Combine(DefaultDataFolder, "stored_sessions.json"));
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

        public void SaveChangedSettings(bool checkUsername = true, bool newSession = false, bool checkApiMaterial = true)
        {
            BrowserViewModel.Instance.AttachedJavascriptWrapper.Modal.Hide("#modalSettingsUsernameChanged");
            cefOsuApp.GetWindow().Dispatcher.Invoke(() =>
            {
                Task task = Task.Run(async () =>
                {
                    cefOsuApp.JsExecuter.SetElementDisabled("#settingsConfirmButton", true);
                    cefOsuApp.JsExecuter.SetHtml("#settingsConfirmButton", "<span class=\"spinner-border spinner-border-sm\" role=\"status\" aria-hidden=\"true\"></span> saving");

                    string apiKey = await BrowserViewModel.Instance.SettingsGetApiKey();
                    string username = await BrowserViewModel.Instance.SettingsGetUsername();
                    string osudir = await BrowserViewModel.Instance.SettingsGetOsuDirectory();
                    OsuMode gamemode = await BrowserViewModel.Instance.SettingsGetGamemode();
                    int updateRateInteger = -1;
                    int roundingDigit = -1;

                    bool processSettings = true;
                    if (checkApiMaterial)
                    {
                        if (!ApiHelper.IsUserValid(apiKey, username))
                        {
                            BrowserViewModel.Instance.SendNotification(NotificationType.Danger, "API Key or username is invalid");
                            processSettings = false;
                        }

                        if (!string.IsNullOrEmpty(osudir) && processSettings)
                        {
                            if (!ApiHelper.IsValidOsuInstallation(osudir))
                            {
                                BrowserViewModel.Instance.SendNotification(NotificationType.Danger, "Selected osu directory is invalid");
                                processSettings = false;
                            }
                        }
                    }

                    if (processSettings)
                    {
                        string updateRate = await BrowserViewModel.Instance.SettingsGetUpdaterate();
                        try
                        {
                            updateRateInteger = Convert.ToInt32(updateRate);
                            updateRateInteger = Math.Min(SettingsManager.RefreshTimeMax, Math.Max(SettingsManager.RefreshTimeMin, updateRateInteger));
                        }
                        catch (Exception)
                        {
                            BrowserViewModel.Instance.SendNotification(NotificationType.Danger, "Update rate value seems to be invalid");
                            processSettings = false;
                        }
                    }

                    if (processSettings)
                    {
                        string roundingDigitVal = await BrowserViewModel.Instance.SettingsGetRoundingValue();
                        try
                        {
                            roundingDigit = Convert.ToInt32(roundingDigitVal);
                            roundingDigit = Math.Min(SettingsManager.RoundingMax, Math.Max(SettingsManager.RoundingMin, roundingDigit));
                        }
                        catch (Exception)
                        {
                            BrowserViewModel.Instance.SendNotification(NotificationType.Danger, "Rounding value seems to be invalid");
                            processSettings = false;
                        }
                    }

                    if (processSettings)
                    {
                        KeyDataCollection displayOptions = SettingsManager.DefaultSettings["showingItems"];
                        foreach (KeyData keyData in displayOptions)
                        {
                            string key = keyData.KeyName;
                            //bool state = await BrowserViewModel.Instance.AttachedJavascriptWrapper.Checkbox.IsChecked("#settingsInputDisplay" + (key.FirstCharToUpper()) + "");
                            bool state = await BrowserViewModel.Instance.AttachedJavascriptWrapper.GetProp("#settingsInputDisplay" + (key.FirstCharToUpper()) + "", "selected");
                            SettingsManager.Instance.Settings["showingItems"][key] = state ? "true" : "false";
                        }
                    }

                    if (processSettings)
                    {
                        bool state = await BrowserViewModel.Instance.AttachedJavascriptWrapper.GetProp("#settingsNsfwMode", "checked");
                        SettingsManager.Instance.Settings["display"]["nsfwMode"] = state ? "true" : "false";

                        //string task = "$('" + obj + "').prop('" + prop + "')";
                        JavascriptResponse res = await BrowserViewModel.Instance.AttachedBrowser.EvaluateScriptAsync("$('input[name=groupRadioThemes]:checked').attr('theme_name');");
                        string usedTheme = (string)res.Result;

                        Theme theme = ThemeManager.Instance.GetThemeFromInternalName(usedTheme);
                        if(theme != null){
                            SettingsManager.Instance.Settings["display"]["theme"] = usedTheme;
                        }else{
                            BrowserViewModel.Instance.SendNotification(NotificationType.Danger, "Settings saved but selected theme could not be found. Did you delete the file?");
                        }
                    }

                    if (processSettings)
                    {
                        if (checkUsername)
                        {
                            if (SettingsManager.Instance.Settings["api"]["user"] != username)
                            {
                                BrowserViewModel.Instance.AttachedJavascriptWrapper.Modal.Show("#modalSettingsUsernameChanged");
                                return;
                            }
                        }

                        SettingsManager.Instance.Settings["api"]["key"] = apiKey;
                        SettingsManager.Instance.Settings["api"]["user"] = username;
                        SettingsManager.Instance.Settings["api"]["updateRate"] = updateRateInteger + "";
                        SettingsManager.Instance.Settings["api"]["gamemode"] = gamemode + "";
                        SettingsManager.Instance.Settings["misc"]["osuFolder"] = osudir;

                        SettingsManager.Instance.Settings["display"]["roundingValue"] = roundingDigit + "";
                        //SettingsManager.Instance.Settings["display"]["nsfwMode"] = await BrowserViewModel.Instance.AttachedJavascriptWrapper.Get;

                        // Save stuff
                        SettingsManager.Instance.SettingsSave();
                        SettingsManager.Instance.SettingsApply();
                        BrowserViewModel.Instance.SendNotification(NotificationType.Success, "Saved settings");

                        if (newSession)
                        {
                            SessionManager.Instance.PrepareSession();
                            BrowserViewModel.Instance.SendNotification(NotificationType.Info, "Started new session");
                        }
                    }

                    //MessageBox.Show(apiKey);
                    cefOsuApp.JsExecuter.SetElementDisabled("#settingsConfirmButton", false);
                    cefOsuApp.JsExecuter.SetHtml("#settingsConfirmButton", "Save and apply");
                });
            });
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
                if (state) {
                    BrowserViewModel.Instance.AttachedJavascriptWrapper.Show("#sessionDisplay" + (key.FirstCharToUpper()) + "");
                } else {
                    BrowserViewModel.Instance.AttachedJavascriptWrapper.Hide("#sessionDisplay" + (key.FirstCharToUpper()) + "");
                }
                //BrowserViewModel.Instance.AttachedJavascriptWrapper.Checkbox.SetChecked("#settingsInputDisplay" + (key.FirstCharToUpper()) + "", keyData.Value == "true");
            }

            if(Settings["display"]["nsfwMode"]=="true")
                ThemeManager.Instance.ApplyTheme(ThemeManager.Instance.GetThemeFromInternalName(Settings["display"]["theme"]));

            //JavascriptResponse res = await BrowserViewModel.Instance.AttachedBrowser.EvaluateScriptAsync("$('" + obj + "').val()");
            //return res.Result.ToString();
            //string apiKey = await BrowserViewModel.Instance.SettingsGetApiKey();
        }

        public void SettingsSave()
        {
            FileIniDataParser parser = new FileIniDataParser();
            parser.WriteFile(SettingsLocation, Settings);

            //GameManager.Instance.ApplySettingsToApp(Settings);
        }
    }
}
