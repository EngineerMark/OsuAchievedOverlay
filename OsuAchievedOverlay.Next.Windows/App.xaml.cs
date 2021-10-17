using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace OsuAchievedOverlay.Next
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainWindow mainWin;
        public void App_Startup(object sender, StartupEventArgs e)
        {
#if DEBUG
            if (!Debugger.IsAttached)
                Debugger.Launch();
#endif

            mainWin = new MainWindow(e);
            mainWin.Show();
        }
    }
}
