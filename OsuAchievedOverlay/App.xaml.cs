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
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

#if DEBUG
            if (!Debugger.IsAttached)
                Debugger.Launch();
#endif
            WindowManager.Instance.LaunchWin = new LaunchWindow(e);
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Directory.CreateDirectory("Logs");

            File.WriteAllText("Logs/CrashLog_" + DateTimeOffset.Now.ToUnixTimeSeconds() + ".txt", e.Exception.Message+"\n\n"+e.Exception.StackTrace);

            MessageBoxResult res = MessageBox.Show("A crash has occured! Check the latest crash log in the /Logs/ folder for details", "Application Exception");
        }
    }
}
