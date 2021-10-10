using OsuAchievedOverlay.Next.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Next.Managers
{
    public class StartupManager : Singleton<StartupManager>
    {
        public static event EventHandler StartupFinished;

        public void CheckSetup()
        {
            bool exists = SettingsManager.Instance.LoadOrCreateSettings();
            bool validApiOrUser = ApiHelper.IsKeyValid(SettingsManager.Instance.Settings["api"]["key"])
                && ApiHelper.IsUserValid(SettingsManager.Instance.Settings["api"]["key"], SettingsManager.Instance.Settings["api"]["user"]);

            if (!exists || !validApiOrUser)
            {
                if (exists && !validApiOrUser)
                {
                    BrowserViewModel.Instance.SendNotification(NotificationType.Danger, StringStorage.Get("Message.NoAPIorUsername"));
                }

                BrowserViewModel.Instance.AttachedJavascriptWrapper.Modal.Show("#generateSettingsModal");
                cefOsuApp.SetupFinished += (object sender, EventArgs e) =>
                {
                    cefOsuApp.GetWindow().Dispatcher.Invoke(() =>
                    {
                        Task.Run(async () =>
                        {
                            BrowserViewModel.Instance.AttachedJavascriptWrapper.SetElementDisabled("#settingsConfirmButton", true);
                            BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#settingsConfirmButton", "<span class=\"spinner-border spinner-border-sm\" role=\"status\" aria-hidden=\"true\"></span> saving");

                            Task task = ProcessSetup();
                            if (await Task.WhenAny(task, Task.Delay(5000)) != task)
                            {
                                BrowserViewModel.Instance.SendNotification(NotificationType.Danger, StringStorage.Get("Message.SomethingWrong"));
                            }

                            //MessageBox.Show(apiKey);
                            BrowserViewModel.Instance.AttachedJavascriptWrapper.SetElementDisabled("#settingsConfirmButton", false);
                            BrowserViewModel.Instance.AttachedJavascriptWrapper.SetHtml("#settingsConfirmButton", "Save");
                        });
                    });
                };
            }
            else
            {
                StartupFinished?.Invoke(null, null);
            }
        }

        public async Task ProcessSetup()
        {
            string apiKey = await BrowserViewModel.Instance.SettingsGetApiKey();
            string username = await BrowserViewModel.Instance.SettingsGetUsername();

            bool processSettings = true;

            if (!ApiHelper.IsUserValid(apiKey, username))
            {
                BrowserViewModel.Instance.SendNotification(NotificationType.Danger, StringStorage.Get("Message.NoAPIorUsername"));
                processSettings = false;
            }

            if (processSettings)
            {
                SettingsManager.Instance.Settings["api"]["key"] = apiKey;
                SettingsManager.Instance.Settings["api"]["user"] = username;
                SettingsManager.Instance.SettingsSave();
                BrowserViewModel.Instance.SendNotification(NotificationType.Success, StringStorage.Get("Message.SettingsSaved"));
                StartupFinished?.Invoke(null, null);
            }
        }
    }
}
