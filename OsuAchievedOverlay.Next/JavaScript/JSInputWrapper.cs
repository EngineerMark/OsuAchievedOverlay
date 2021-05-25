using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Next.JavaScript
{
    public class JSInputWrapper
    {
        private JSWrapper parent { get; }

        public JSInputWrapper(JSWrapper parent){
            this.parent = parent;
        }

        public void SetValue(string obj, string data)
        {
            parent.GetBrowser().ExecuteScriptAsyncWhenPageLoaded("$('" + obj + "').val('" + data + "')");
        }

        public async Task<string> GetValue(string obj)
        {
            JavascriptResponse res = await parent.GetBrowser().EvaluateScriptAsync("$('" + obj + "').val()");
            return res.Result.ToString();
        }
    }
}
