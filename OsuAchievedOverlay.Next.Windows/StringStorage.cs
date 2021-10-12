using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Next
{
    public static class StringStorage
    {
        public static readonly Dictionary<string, string> Storage = new Dictionary<string, string>(){
            ["Message.NoBeatmaps"] = "No sets could be found!",
            ["Message.NewSessionStart"] = "Started a new session",
            ["Message.LoadSessionFail"] = "Failed to load session",
            ["Message.OpenFileFail"] = "Could not open that file",
            ["Message.LoadSessionDeserializeFail"] = "Session file seemed fine, but unable to deserialize",
            ["Message.SessionSaved"] = "Saved session",
            ["Message.SessionSaveFail"] = "Something went wrong while saving, please retry",
            ["Message.NoAPIorUsername"] = "API Key or username is invalid",
            ["Message.SomethingWrong"] = "Something went wrong. Please retry.",
            ["Message.SettingsSaved"] = "Saved settings",
            ["Message.InvalidOsuInstall"] = "Selected osu directory is invalid",
            ["Message.InvalidUpdateRate"] = "Update rate value seems to be invalid",
            ["Message.InvalidRoundValue"] = "Rounding value seems to be invalid",
            ["Message.InvalidTheme"] = "Settings saved but selected theme could not be found. Did you delete the file?",
            ["Message.UnreadableSession"] = "Session list file is present, but unable to read it",
            ["Message.UserDataError"] = "Error retrieving user data",
            ["Message.NoInternet"] = "You are not connected to the internet",
            ["Message.Osu.NoConfig"] = "osu! directory is correct, but no configuration was found",
            ["Message.Osu.Process"] = "Process osu",
            ["Message.MissingTabContent"] = "Tab content for %s does not exist! Please reinstall the program.",
            ["Message.MissingAddonContent"] = "Tool content for %s does not exist! Please reinstall the program.",
            ["Button.SaveApply"] = "Save and apply",
            ["Title.OsuSelector"] = "Select osu! installation folder",
            ["Text.NoUpdate"] = "No update info to show. Perhaps something went wrong?",
            ["URL.Repository"] = "https://github.com/EngineerMark/OsuAchievedOverlay",
            ["URL.APIRepository"] = "https://api.github.com/repos/EngineerMark/OsuAchievedOverlay",
        };

        public static string Get(string key) => Storage[key];
    }
}
