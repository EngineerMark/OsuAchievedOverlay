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
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

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

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Ignore missing resources
            if (args.Name.Contains(".resources"))
                return null;

            // check for assemblies already loaded
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (assembly != null)
                return assembly;

            // Try to load by filename - split out the filename of the full assembly name
            // and append the base path of the original assembly (ie. look in the same dir)
            string filename = args.Name.Split(',')[0] + ".dll".ToLower();

            string asmFile = Path.Combine(@".\", "lib", filename);

            try
            {
                return System.Reflection.Assembly.LoadFrom(asmFile);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
