using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Next.JavaScript
{
    /// <summary>
    /// A 'wrapper' or helper class for executing javascript onto a cef browser, built for OsuAchieved
    /// </summary>
    public class JSWrapper
    {
        private static ChromiumWebBrowser _internalBrowser;

        public JSWrapper(ChromiumWebBrowser browser)
        {
            _internalBrowser = browser;
        }

        public void SetAttribute(string obj, string attribute, string value)
        {
            _internalBrowser.ExecuteScriptAsyncWhenPageLoaded("$('" + obj + "').attr('" + attribute + "', '" + value + "');");
        }

        public void AddClass(string obj, string _class)
        {
            _internalBrowser.ExecuteScriptAsyncWhenPageLoaded("$('" + obj + "').addClass('" + _class + "');");
        }

        public void Html(string obj, string data){
            _internalBrowser.ExecuteScriptAsyncWhenPageLoaded("$('" + obj + "').html('" + data + "');");
        }
    }
}
