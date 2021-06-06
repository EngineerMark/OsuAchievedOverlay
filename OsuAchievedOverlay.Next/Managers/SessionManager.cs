using OsuApiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Next.Managers
{
    public class SessionManager : Singleton<SessionManager>
    {
        public Session CurrentSession { get; set; }
        public ExtendedThread SessionThread { get; set; }

        //Prepare a new session
        public void PrepareSession()
        {
            SessionThread?.Join();

            CurrentSession = new Session();

            SessionThread = new ExtendedThread(() => OnUpdate(), Convert.ToInt32(SettingsManager.Instance.Settings["api"]["updateRate"]));
            SessionThread.Start();
        }

        public void Stop(){
            SessionThread?.Join();
        }

        private void OnUpdate()
        {
            if (!NetworkManager.Instance.HasConnection())
            {
                BrowserViewModel.Instance.SendNotification(NotificationType.Warning, "You are not connected to the internet");
                return;
            }

            //BrowserViewModel.Instance.SendNotification(NotificationType.Success, "Test: session iteration");
            OsuUser currentUserData = null;
            try
            {
                currentUserData = OsuApi.GetUser(SettingsManager.Instance.Settings["api"]["user"], (OsuMode)Enum.Parse(typeof(OsuMode), SettingsManager.Instance.Settings["api"]["gamemode"]));
            }catch(Exception){
                BrowserViewModel.Instance.SendNotification(NotificationType.Danger, "Error retrieving user data");
            }

            if (currentUserData != null)
            {
                if (CurrentSession.InitialData == null)
                    CurrentSession.InitialData = SessionData.FromUser(currentUserData);

                CurrentSession.CurrentData = SessionData.FromUser(currentUserData);
                CurrentSession.DifferenceData = CurrentSession.CurrentData - CurrentSession.InitialData;

                BrowserViewModel.Instance.ApplySession(CurrentSession);
                BrowserViewModel.Instance.ApplyUser(currentUserData);

                int updateRate = Convert.ToInt32(SettingsManager.Instance.Settings["api"]["updateRate"]);
                if (updateRate != SessionThread.SleepTime)
                    SessionThread.SleepTime = updateRate;
            }
        }
    }
}
