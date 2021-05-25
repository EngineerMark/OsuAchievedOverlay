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

        public void SetHtml(string obj, string data)
        {
            _internalBrowser.ExecuteScriptAsyncWhenPageLoaded("$('" + obj + "').html('" + data + "');");
        }

        public async Task<string> GetHtml(string obj)
        {
            JavascriptResponse res = await _internalBrowser.EvaluateScriptAsync("$('" + obj + "').html()");
            return res.Result.ToString();
        }

        public async Task<string> GetAttribute(string obj, string attribute)
        {
            JavascriptResponse res = await _internalBrowser.EvaluateScriptAsync("$('" + obj + "').attr('" + attribute + "')");
            return res.Result.ToString();
        }

        public void SetInputValue(string obj, string data)
        {
            _internalBrowser.ExecuteScriptAsyncWhenPageLoaded("$('" + obj + "').val('"+data+"')");
        }

        public async Task<string> GetInputValue(string obj)
        {
            JavascriptResponse res = await _internalBrowser.EvaluateScriptAsync("$('" + obj + "').val()");
            return res.Result.ToString();
        }

        public void SetElementDisabled(string obj, bool state)
        {
            _internalBrowser.ExecuteScriptAsyncWhenPageLoaded("$('" + obj + "').prop('disabled', "+(state?"true":"false")+")");
        }

        private T EvalJs<T>(string script, TimeSpan timeout)
        {
            T val = default(T);

            if (_internalBrowser.IsBrowserInitialized && !_internalBrowser.IsDisposed)
            {
                try
                {
                    var task = _internalBrowser.EvaluateScriptAsync(script, timeout);
                    var completed = task.ContinueWith(res =>
                    {
                        if (!res.IsFaulted)
                        {
                            var response = res.Result;
                            val = response.Success ? (T)response.Result : default(T);
                        }
                        else
                        {
                            Console.WriteLine("JS Thread is faulted");
                        }
                    });
                    completed.Wait();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.InnerException.Message);
                }
            }

            return (T)val;
        }
    }
}
