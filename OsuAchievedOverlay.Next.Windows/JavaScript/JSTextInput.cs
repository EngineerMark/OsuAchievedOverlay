using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OsuAchievedOverlay.Next.JavaScript
{
    public class JSTextInput : JSInput
    {
        public JSTextInput(JSWrapper parent) : base(parent) { }

        public override void SetValue(string obj, string data)
        {
            parent.GetBrowser().ExecuteScriptAsyncWhenPageLoaded("$('" + obj + "').val('" + HttpUtility.JavaScriptStringEncode(data) + "').trigger('change');");
        }

        public override async Task<string> GetValue(string obj)
        {
            JavascriptResponse res = await parent.GetBrowser().EvaluateScriptAsync("$('" + obj + "').val()");
            return res.Result.ToString();
        }
    }
}
