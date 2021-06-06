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

        private CancellationTokenSource token;

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
            token?.Cancel();
            token?.Dispose();
        }

        private void Build(){
            token = new CancellationTokenSource();
            InternalThread = new Thread(new ThreadStart(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    Task task = Task.Run(()=>DelegateFunction());
                    task.Wait(TimeSpan.FromSeconds(TimeoutTime));
                    Thread.Sleep(SleepTime*1000);
                }
            }));
        }
    }
}
