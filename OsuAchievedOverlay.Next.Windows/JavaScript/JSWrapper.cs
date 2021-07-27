using CefSharp;
using CefSharp.WinForms;
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

        public JSTextInput TextInput { get; }
        public JSRangeInput RangeInput { get; }
        public JSSelectInput SelectInput { get; }
        public JSCheckbox Checkbox { get; }
        public JSModal Modal { get; }

        public JSWrapper(ChromiumWebBrowser browser)
        {
            _internalBrowser = browser;

            TextInput = new JSTextInput(this);
            RangeInput = new JSRangeInput(this);
            SelectInput = new JSSelectInput(this);
            Checkbox = new JSCheckbox(this);
            Modal = new JSModal(this);
        }

        public void Hide(string obj)
        {
            _internalBrowser.ExecuteScriptAsyncWhenPageLoaded("$('" + obj + "').hide();");
            SetProp(obj, "hidden", true);
        }

        public void Show(string obj)
        {
            _internalBrowser.ExecuteScriptAsyncWhenPageLoaded("$('" + obj + "').show();");
            SetProp(obj, "hidden", false);
        }

        public void FadeIn(string obj, int duration = 400)
        {
            _internalBrowser.ExecuteScriptAsyncWhenPageLoaded("$('" + obj + "').fadeIn(" + duration + ");");
            SetProp(obj, "hidden", false);
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

        public void SetElementDisabled(string obj, bool state)
        {
            //_internalBrowser.ExecuteScriptAsyncWhenPageLoaded("$('" + obj + "').prop('disabled', "+(state?"true":"false")+")");
            SetProp(obj, "disabled", state);
        }

        public void SetProp(string obj, string prop, bool state)
        {
            _internalBrowser.ExecuteScriptAsyncWhenPageLoaded("$('" + obj + "').prop('" + prop + "', " + (state ? "true" : "false") + ")");
        }

        public async Task<bool> GetProp(string obj, string prop)
        {
            string task = "$('" + obj + "').prop('" + prop + "')";
            JavascriptResponse res = await _internalBrowser.EvaluateScriptAsync(task);
            return (bool)res.Result;
        }

        public ChromiumWebBrowser GetBrowser() => _internalBrowser;
    }
}
