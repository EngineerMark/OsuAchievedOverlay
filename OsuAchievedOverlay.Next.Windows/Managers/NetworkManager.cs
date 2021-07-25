using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Next.Managers
{
    public class NetworkManager : Singleton<NetworkManager>
    {
        /// <summary>
        /// Performs ping tests on several domains that are virtually always online. If any of them pass, you have an internet connection.
        /// </summary>
        public bool HasConnection(){
            return (PingValid("google.com") || PingValid("bing.com") || PingValid("youtube.com"));
        }

        /// <summary>
        /// Perform ping test on a domain
        /// </summary>
        public bool PingValid(string url, int timeout = 1000){
            try{
                Ping p = new Ping();
                byte[] buffer = new byte[32];
                PingOptions pOptions = new PingOptions();
                PingReply pReply = p.Send(url, timeout, buffer, pOptions);
                return (pReply.Status == IPStatus.Success);
            }catch(Exception){
                return false;
            }
        }
    }
}
