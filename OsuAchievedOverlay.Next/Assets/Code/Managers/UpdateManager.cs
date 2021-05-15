using Newtonsoft.Json;
using OsuAchievedOverlay.Github;
using OsuAchievedOverlay.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OsuAchievedOverlay.Managers
{
    public class UpdateManager : Manager
    {
        public const string version = "1.0.6";

        public Release AvailableUpdate = null;

        public void Start()
        {
            GetAvailableUpdates();
        }

        public void GetAvailableUpdates()
        {
            string data = "";
            try
            {
                data = ApiHelper.GetData("https://api.github.com/repos/EngineerMark/OsuAchievedOverlay/releases");
            }
            catch (Exception e)
            {
                data = "[]";
            }

            List<Release> releases = JsonConvert.DeserializeObject<List<Release>>(data);

            if(releases.Count>0){
                Version currentVersion = new Version(version);
                Version latestVersion = new Version(releases[0].Version);

                if(latestVersion>currentVersion){
                    AvailableUpdate = releases[0];
                }
            }
        }
    }
}
