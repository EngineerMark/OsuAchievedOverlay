using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

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

        public List<string> GetAllFilesInDirectory(string dir){
            List<string> files = new List<string>();

            foreach (string f in Directory.GetFiles(dir))
                files.Add(f);
            foreach (string d in Directory.GetDirectories(dir))
                files.AddRange(GetAllFilesInDirectory(d));

            return files;
        }

        public DirectoryInfo GetApplicationDirectory(){
            return Directory.GetParent(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).FullName);
        }

        public string GetCurrentExecutable(){
            string path = Application.ResourceAssembly.Location;
            path = Path.ChangeExtension(path, ".exe");
            return path;
        }

        public void MoveFile(string from, string to, bool overwrite = false){
            if (!Directory.Exists(Path.GetDirectoryName(to)))
                Directory.CreateDirectory(Path.GetDirectoryName(to));
            if (File.Exists(to) && overwrite)
                File.Delete(to);
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

        public void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
