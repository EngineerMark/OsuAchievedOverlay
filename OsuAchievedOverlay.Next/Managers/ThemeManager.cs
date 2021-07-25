using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Next.Managers
{
    public class ThemeManager : Singleton<ThemeManager>
    {
        public List<Theme> Themes { get; set; }

        private string themePath = "";

        public void Start(){
            themePath = Path.Combine(FileManager.GetExecutableDirectory(),"Themes");

            RefreshList();
        }

        public void RefreshList(){
            Themes = new List<Theme>();

            List<string> themeFiles = Directory.GetFiles(themePath).ToList();

            themeFiles.ForEach(file => {
                Themes.Add(Theme.LoadFromFile(file));
            });
        }
    }
}
