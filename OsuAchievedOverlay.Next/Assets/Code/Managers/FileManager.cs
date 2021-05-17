using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace OsuAchievedOverlay.Managers
{
    public class FileManager : ThreadedManager
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

        public static List<string> GetAllFilesInDirectory(string dir){
            List<string> files = new List<string>();

            foreach (string f in Directory.GetFiles(dir))
                files.Add(f);
            foreach (string d in Directory.GetDirectories(dir))
                files.AddRange(GetAllFilesInDirectory(d));

            return files;
        }

        public static DirectoryInfo GetApplicationDirectory(){
            return Directory.GetParent(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).FullName);
        }

        public static string GetExecutableDirectory(){
#if UNITY_EDITOR
            string baseFolder = Path.Combine(Application.dataPath, "BuildDir");
            if (!Directory.Exists(baseFolder))
                Directory.CreateDirectory(baseFolder);
#else
            string baseFolder = Directory.GetParent(Application.dataPath).FullName;
#endif
            return baseFolder;
        }

        public static void MoveFile(string from, string to, bool overwrite = false){
            if (File.Exists(to) && overwrite)
                File.Delete(to);
            if (!Directory.Exists(Path.GetDirectoryName(to)))
                Directory.CreateDirectory(Path.GetDirectoryName(to));
            File.Move(from, to);
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
