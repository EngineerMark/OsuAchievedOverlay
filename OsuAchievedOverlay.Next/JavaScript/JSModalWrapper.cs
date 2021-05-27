using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Next.JavaScript
{
    public class JSModalWrapper
    {
        private JSWrapper parent { get; }

        public JSModalWrapper(JSWrapper parent)
        {
            this.parent = parent;
        }

        public void Show(string obj){
            parent.GetBrowser().ExecuteScriptAsyncWhenPageLoaded("$('"+obj+ "').modal('show');");
        }

        public void Hide(string obj){
            parent.GetBrowser().ExecuteScriptAsyncWhenPageLoaded("$('"+obj+ "').modal('hide');");
        }

        public void Toggle(string obj)
        {
            parent.GetBrowser().ExecuteScriptAsyncWhenPageLoaded("$('" + obj + "').modal('toggle');");
        }
    }
}
