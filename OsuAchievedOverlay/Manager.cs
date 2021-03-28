using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay
{
    public abstract class Manager<T> : IManager where T : class, new()
    {
        private static T _instance;
        public static T Instance {
            get
            {
                if (_instance != null)
                    return _instance;

                _instance = new T();
                return _instance;
            }
        }

        public abstract void Start();

        public abstract void Stop();
    }

    public interface IManager{
        void Start();
        void Stop();
    }
}
