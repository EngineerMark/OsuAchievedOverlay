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

        public DiscordRpcClient Client { get => client; set => client = value; }

        public override void Start()
        {
            //Stop();

            //Client = new DiscordRpcClient("834746141475471401");
            //Client.Initialize();

            //Client.SetPresence(new RichPresence()
            //{
            //    Details = "Test",
            //    State = "test2",
            //});
        }

        public void SetStatus(){
            
        }

        public override void Stop()
        {
            Client?.ClearPresence();
            Client?.Dispose();

            Client = null;
        }
    }
}
