using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Next
{
    public class Singleton<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (Singleton<T>._instance == null)
                    Singleton<T>._instance = (T)Activator.CreateInstance(typeof(T));
                return Singleton<T>._instance;
            }
        }
    }
}
