using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Next.JavaScript
{
    public class JSCheckbox
    {
        private JSWrapper parent;

        public JSCheckbox(JSWrapper parent){
            this.parent = parent;
        }

        public async Task<bool> IsChecked(string obj)
        {
            //JavascriptResponse res = await parent.GetBrowser().EvaluateScriptAsync("$('" + obj + "').val()");
            //return res.Result.ToString();
            return await parent.GetProp(obj, "checked");
        }

        public void SetChecked(string obj, bool check)
        {
            parent.SetProp(obj, "checked", check);
            //parent.GetBrowser().ExecuteScriptAsyncWhenPageLoaded("$('" + obj + "').val('" + data + "').trigger('change');");
        }
    }
}
