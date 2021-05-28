using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Next.JavaScript
{
    public class JSSelectInput : JSInput
    {
        public JSSelectInput(JSWrapper parent) : base(parent) { }

        public override async Task<string> GetValue(string obj)
        {
            JavascriptResponse res = await parent.GetBrowser().EvaluateScriptAsync("$('" + obj + "').val();");
            return res.Result.ToString();
        }

        public override void SetValue(string obj, string data)
        {
            parent.GetBrowser().ExecuteScriptAsyncWhenPageLoaded("$('" + obj + "').val(" + data + ").change();");
        }
    }
}
