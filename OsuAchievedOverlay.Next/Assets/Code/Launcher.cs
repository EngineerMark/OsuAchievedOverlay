using IniParser;
using IniParser.Model;
using OsuAchievedOverlay.Helpers;
using OsuAchievedOverlay.Managers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Launcher : MonoBehaviour
{
    public GameObject SettingsFailWindow;
    public TMP_InputField InputAPIKey;
    public TMP_InputField InputUsername;

    public UpdateManager UpdateManager;

    public void Start()
    {
        SettingsFailWindow.SetActive(false);
        string[] cmds = System.Environment.GetCommandLineArgs();
        if (cmds.Contains("-osufinishupdate"))
        {
            if (Directory.Exists(Path.Combine(Application.dataPath, "temp")))
            {
                bool deleted = false;
                while (!deleted)
                {
                    try
                    {
                        Directory.Delete(Path.Combine(Application.dataPath, "temp"), true);
                        deleted = true;
                    }
                    catch (IOException _e)
                    {
                        deleted = false;
                    }
                }
            }
        }else{
            UpdateManager.InternalStart(()=>
            {
                TestSettings();
            });
        }
    }

    public void ApplyFixedSettings(){
        SettingsManager.Settings["api"]["key"] = InputAPIKey.text;
        SettingsManager.Settings["api"]["user"] = InputUsername.text;

        OsuApiHelper.OsuApiKey.Key = SettingsManager.Settings["api"]["key"];
        if (!OsuApiHelper.OsuApi.IsKeyValid()){
            SettingsFailWindow.SetActive(true);
            return;
        }

        if(!OsuApiHelper.OsuApi.IsUserValid(SettingsManager.Settings["api"]["user"])){
            SettingsFailWindow.SetActive(true);
            return;
        }

        SettingsManager.SaveSettings();
        SceneManager.LoadScene(1);
    }

    public void TestSettings(){
        SettingsFailWindow.SetActive(false);
        string baseFolder = FileManager.GetExecutableDirectory();

        FileIniDataParser parser = new FileIniDataParser();

        if(File.Exists(Path.Combine(baseFolder, "settings.ini"))){
            IniData data = parser.ReadFile(Path.Combine(baseFolder, "settings.ini"));
            SettingsManager.Settings = data;
            string key = data["api"]["key"];
            OsuApiHelper.OsuApiKey.Key = key;
            if (!OsuApiHelper.OsuApi.IsKeyValid())
            {
                SettingsFailWindow.SetActive(true);
            }else{
                //Done
                SceneManager.LoadScene(1);
            }
        }
        else
        {
            IniData newData = new IniData();

            newData = SettingsManager.FixIniData(parser, newData);
            parser.WriteFile(Path.Combine(baseFolder, "settings.ini"), newData);
            SettingsManager.Settings = newData;

            SettingsFailWindow.SetActive(true);
        }
    }
}
