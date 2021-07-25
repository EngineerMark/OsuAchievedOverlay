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

        public static List<string> GetAllFilesInDirectory(string dir)
        {
            List<string> files = new List<string>();

            foreach (string f in Directory.GetFiles(dir))
                files.Add(f);
            foreach (string d in Directory.GetDirectories(dir))
                files.AddRange(GetAllFilesInDirectory(d));

            return files;
        }

        public static bool IsSubDir(string fullpath, string directoryName)
        {
            return fullpath.Contains(string.Format(@"{0}\", directoryName));
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

        public static void CopyFile(string from, string to)
        {
            if (!Directory.Exists(Path.GetDirectoryName(to)))
                Directory.CreateDirectory(Path.GetDirectoryName(to));

            File.Copy(from, to, true);
        }

        public static void WriteAllText(string file, string data)
        {
            //FileWriteQueue.Enqueue(new KeyValuePair<string, string>(file, data));
            ThreadPool.QueueUserWorkItem((object stateInfo) =>
            {
                File.WriteAllText(file, data);
            });
        }

        public static string ReadAllText(string file)
        {
            return File.ReadAllText(file);
        }
    }
}
