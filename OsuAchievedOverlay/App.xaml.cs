using OsuAchievedOverlay.Managers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
#if DEBUG
            if (!Debugger.IsAttached)
                Debugger.Launch();
#endif
            WindowManager.Instance.LaunchWin = new LaunchWindow(e);
        }
    }
}
