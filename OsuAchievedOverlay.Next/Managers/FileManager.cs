using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Next.Managers
{
    public class FileManager : StaticManager
    {
        public static string GetExecutablePath(){
            return System.Reflection.Assembly.GetEntryAssembly().Location;
        }

        public static string GetExecutableDirectory(){
            return Path.GetDirectoryName(GetExecutablePath());
        }

        public static long GetFileSize(string file){
            if(File.Exists(file)){
                return new FileInfo(file).Length;
            }
            return 0;
        }

        public static bool IsFileReady(string filename)
        {
            try
            {
                using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                    return inputStream.Length > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void MoveFile(string from, string to){
            if (!Directory.Exists(Path.GetDirectoryName(to)))
                Directory.CreateDirectory(Path.GetDirectoryName(to));

            File.Move(from, to);
        }

        public static void WriteAllText(string file, string data)
        {
            //FileWriteQueue.Enqueue(new KeyValuePair<string, string>(file, data));
            ThreadPool.QueueUserWorkItem((object stateInfo) =>
            {
                File.WriteAllText(file, data);
            });
        }
    }
}
