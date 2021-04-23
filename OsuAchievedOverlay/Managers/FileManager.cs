using OsuAchievedOverlay.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Managers
{
    public class FileManager : Manager<FileManager>, IThreaded
    {
        private Thread thread;
        private Queue<KeyValuePair<string, string>> FileWriteQueue { get; set; }

        public Thread Thread { get => thread; set => thread = value; }

        public FileManager(){
            Start();
        }

        public override void Start()
        {
            FileWriteQueue = new Queue<KeyValuePair<string, string>>();

            Thread = new Thread(new ThreadStart(ThreadStep))
            {
                IsBackground = true
            };
            Thread.Start();
        }

        public override void Stop()
        {
            Thread?.Join();
        }

        private void ThreadStep(){
            while(true){
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
