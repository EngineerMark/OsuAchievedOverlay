using OsuAchievedOverlay.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Managers
{
    public abstract class ThreadedManager<T> : Manager<T>, IThreaded where T : class, new()
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
