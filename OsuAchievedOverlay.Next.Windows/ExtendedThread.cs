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
        public Thread InternalThread{ get; set; }
        public Action DelegateFunction { get; set; }
        public int SleepTime { get; set; }
        public int TimeoutTime { get; set; } = 30;
        public bool IsAlive { get; set; } = false;

        public ExtendedThread(Action func, int sleepTime, int timeout = 30){
            DelegateFunction = func;
            SleepTime = sleepTime;
            TimeoutTime = timeout;
        }

        public void Start(){
            Join();
            Build();
            InternalThread.Start();
        }

        public void Join(){
            if (InternalThread!=null && InternalThread.IsAlive)
                InternalThread?.Abort();
        }

        private void Build(){
            InternalThread = new Thread(new ThreadStart(() =>
            {
                IsAlive = true;
                while (IsAlive)
                {
                    Task task = Task.Run(()=>DelegateFunction());
                    task.Wait(TimeSpan.FromSeconds(TimeoutTime));
                    Thread.Sleep(SleepTime*1000);
                }
            }));
        }
    }
}
