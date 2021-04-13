using System.Collections.Generic;

namespace OsuAchievedOverlay
{
    public abstract class Manager<T> : IManager where T : class, new()
    {
        private static T _instance;
        public static List<T> List = new List<T>();
        public static T Instance {
            get
            {
                if (_instance != null)
                    return _instance;

                _instance = new T();
                List.Add(_instance);
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
