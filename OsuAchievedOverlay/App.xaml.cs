using OsuAchievedOverlay.Managers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace OsuAchievedOverlay
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void StartApp(object sender, StartupEventArgs e)
        {
            WindowManager.Instance.LaunchWin = new LaunchWindow(e);
        }
    }
}
