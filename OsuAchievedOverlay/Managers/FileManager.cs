using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace OsuAchievedOverlay.Managers
{
    public class FileManager : ThreadedManager<FileManager>
    {
        private Queue<KeyValuePair<string, string>> FileWriteQueue { get; set; }

        public FileManager(){
            Start();
        }

        public override void Start()
        {
            FileWriteQueue = new Queue<KeyValuePair<string, string>>();

            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
        }

        protected override void ThreadStep(){
            while(running)
            {
                if(FileWriteQueue.Count>0){
                    KeyValuePair<string, string> fileProcess = FileWriteQueue.Dequeue();
                    File.WriteAllText(fileProcess.Key, fileProcess.Value);
                }
                Thread.Sleep(100);
            }
        }

        public void WriteAllText(string file, string data){
            FileWriteQueue.Enqueue(new KeyValuePair<string, string>(file, data));
        }
    }
}
