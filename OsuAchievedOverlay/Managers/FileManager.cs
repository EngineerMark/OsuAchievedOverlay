using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Managers
{
    public class FileManager : Manager<FileManager>
    {
        public override void Start()
        {
        }

        public override void Stop()
        {
        }

        public void WriteTextAsync(string file, string data){
            byte[] byteData = Encoding.Unicode.GetBytes(data);
            using (FileStream fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)){
                fs.WriteAsync(byteData, 0, byteData.Length);
            }
        }
    }
}
