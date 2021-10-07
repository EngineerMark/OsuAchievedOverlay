using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CefSharp;
using CefSharp.WinForms;

namespace OsuAchievedOverlay.Next.Managers
{
    public class ThemeManager : Singleton<ThemeManager>
    {
        public List<Theme> Themes { get; set; }

        private string themePath = "";

        public void Start()
        {
            themePath = Path.Combine(FileManager.GetExecutableDirectory(), "Themes");

            RefreshList();
        }

        public Theme GetThemeFromInternalName(string name)
        {
            return Themes.Find(t => t.InternalName.Equals(name));
        }

        public void ApplyTheme(Theme theme)
        {
            if (theme.ThemeData != null)
            {
                switch (theme.ThemeData.BackgroundType)
                {
                    default:
                    case "color":
                        BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#appBody').css('background-image', '');");
                        BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#appBody').css('background-color', '" + theme.ThemeData.Background + "');");
                        break;
                    case "remote_image":
                        BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#appBody').css('background-color', '');");
                        BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("$('#appBody').css('background-image', 'url(\\\'" + theme.ThemeData.Background + "\\\')');");
                        break;
                }
            }

            BrowserViewModel.Instance.AttachedBrowser.ExecuteScriptAsyncWhenPageLoaded("applyTheme('" + HttpUtility.JavaScriptStringEncode(theme.CustomStyle) + "');");
        }

        public void RefreshList()
        {
            Themes = new List<Theme>();

            List<string> themeFiles = Directory.GetFiles(themePath).ToList();

            themeFiles.ForEach(file =>
            {
                Theme t = Theme.LoadFromFile(file);
                if (t != null)
                    Themes.Add(t);
            });
        }
    }
}
