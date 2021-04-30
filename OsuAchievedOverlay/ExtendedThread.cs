using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OsuAchievedOverlay
{
    public class ExtendedThread
    {
        public bool IsRunning { get; set; }
        public Thread InternalThread{ get; set; }
        public Action DelegateFunction { get; set; }
        public int SleepTime { get; set; }

        public ExtendedThread(Action func, int sleepTime){
            DelegateFunction = func;
            SleepTime = sleepTime;

            Build();
        }

        public void Start(){
            IsRunning = true;
            Build();
            InternalThread.Start();
        }

        public void Join(){
            IsRunning = false;
            if(InternalThread.IsAlive)
                InternalThread?.Join();
        }

        private void Build(){
            InternalThread = new Thread(new ThreadStart(() =>
            {
                while (IsRunning)
                {
                    DelegateFunction();
                    Thread.Sleep(SleepTime);
                }
            }));
        }
    }
}
