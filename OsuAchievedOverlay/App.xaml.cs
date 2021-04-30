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
using System.Windows.Threading;
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
            this.DispatcherUnhandledException += Dispatcher_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

#if DEBUG
            if (!Debugger.IsAttached)
                Debugger.Launch();
#endif
            WindowManager.Instance.LaunchWin = new LaunchWindow(e);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            CreateCrashDump((Exception)e.ExceptionObject);
        }

        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            CreateCrashDump(e.Exception);
        }

        public void CreateCrashDump(Exception e){
            Directory.CreateDirectory("Logs");
            Trace.WriteLine("osu!Achieved " + UpdateManager.version + "\n\n" + e.Message + "\n" + e.StackTrace);
            File.WriteAllText("Logs/CrashLog_" + DateTimeOffset.Now.ToUnixTimeSeconds() + ".txt", "osu!Achieved "+UpdateManager.version+"\n\n"+e.Message+"\n"+e.StackTrace);
            MessageBoxResult res = MessageBox.Show("A crash has occured! Check out /Logs/ folder for the crash log(s)", "Application Exception");
            Environment.Exit(-1);
        }
    }
}
