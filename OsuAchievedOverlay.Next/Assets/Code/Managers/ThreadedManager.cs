using System.Threading;

namespace OsuAchievedOverlay.Managers
{
    public abstract class ThreadedManager : Manager
    {
        protected bool running;

        private Thread thread;
        public Thread Thread { get => thread; set => thread = value; }

        public override void Start()
        {
            Thread = new Thread(new ThreadStart(ThreadStep))
            {
                IsBackground = true
            };
            running = true;
            Thread.Start();
        }
        public override void Stop()
        {
            running = false;
            Thread?.Join();
        }

        protected abstract void ThreadStep();
    }
}
