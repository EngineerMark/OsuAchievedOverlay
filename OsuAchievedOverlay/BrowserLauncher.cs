using Chromely;
using Chromely.Core;
using Chromely.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

namespace OsuAchievedOverlay
{
    public class BrowserLauncher
    {
        public void Run(){
            var config = DefaultConfiguration.CreateForRuntimePlatform();
            config.StartUrl = "local://wwwroot/index.html";
            AppBuilder.Create().UseConfig<DefaultConfiguration>(config).UseApp<OsuApp>().Build().Run(Array.Empty<string>());
        }
    }

    public class CustomDisplayHandler : Xilium.CefGlue.CefDisplayHandler
    {
    }

    public class OsuApp : ChromelyBasicApp{
        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddLogging(configure => configure.AddConsole());
            //services.AddLogging(configure => configure.AddFile("Logs/serilog-{Date}.txt"));

            services.AddSingleton<Xilium.CefGlue.CefDisplayHandler, CustomDisplayHandler>();

            RegisterControllerAssembly(services, typeof(OsuApp).Assembly);
        }
    }
}
