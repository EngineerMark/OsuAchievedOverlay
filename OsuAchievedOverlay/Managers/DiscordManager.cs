using DiscordRPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Managers
{
    public class DiscordManager : Manager<DiscordManager>
    {
        public enum DiscordDisplay{
            GainedSS,
            GainedS,
            GainedA,
            GainedRankedScore,
            GainedTotalScore
        }

        private DiscordRpcClient client;
        private DiscordDisplay display = DiscordDisplay.GainedSS;

        public DiscordRpcClient Client { get => client; set => client = value; }
        public DiscordDisplay Display { get => display; set => display = value; }

        public override void Start()
        {
            Stop();

            Client = new DiscordRpcClient("834746141475471401");
            Client.Initialize();

            Update();
        }

        public void SetStatus(){
            RichPresence richPresence = new RichPresence()
            {
                Details = "Test",
                State = "",
                Assets = new Assets()
                {
                    LargeImageText = "Tracking " + GameManager.Instance.Settings["api"]["user"]
                }
            };

            switch(Display){
                case DiscordDisplay.GainedSS:
                    if(GameManager.Instance.CurrentSession.DifferenceData!=null)
                        richPresence.Details = "Gained "+GameManager.Instance.CurrentSession.DifferenceData.RankSilverSS+" Silver SS and "+ GameManager.Instance.CurrentSession.DifferenceData.RankGoldSS+" Gold SS";
                    else
                        richPresence.Details = "Gained 0 Silver SS and 0 Gold SS";
                    break;
                case DiscordDisplay.GainedS:
                    if (GameManager.Instance.CurrentSession.DifferenceData != null)
                        richPresence.Details = "Gained " + GameManager.Instance.CurrentSession.DifferenceData.RankSilverS + " Silver S and " + GameManager.Instance.CurrentSession.DifferenceData.RankGoldS + " Gold S";
                    else
                        richPresence.Details = "Gained 0 Silver S and 0 Gold S";
                    break;
                case DiscordDisplay.GainedA:
                    if (GameManager.Instance.CurrentSession.DifferenceData != null)
                        richPresence.Details = "Gained " + GameManager.Instance.CurrentSession.DifferenceData.RankA + " A ranks";
                    else
                        richPresence.Details = "Gained 0 A ranks";
                    break;
                case DiscordDisplay.GainedRankedScore:
                    if (GameManager.Instance.CurrentSession.DifferenceData != null)
                        richPresence.Details = "Gained " + GameManager.Instance.CurrentSession.DifferenceData.RankedScore.ToString("#,##0.###") + " ranked score";
                    else
                        richPresence.Details = "Gained 0 ranked score";
                    break;
                case DiscordDisplay.GainedTotalScore:
                    if (GameManager.Instance.CurrentSession.DifferenceData != null)
                        richPresence.Details = "Gained " + GameManager.Instance.CurrentSession.DifferenceData.TotalScore.ToString("#,##0.###") + " total score";
                    else
                        richPresence.Details = "Gained 0 total score";
                    break;
            }

            Client.SetPresence(richPresence);
        }

        public void Update(){
            if(client!=null)
                SetStatus();
        }

        public override void Stop()
        {
            Client?.ClearPresence();
            Client?.Dispose();

            Client = null;
        }
    }
}
