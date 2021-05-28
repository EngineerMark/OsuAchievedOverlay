using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Next.JavaScript
{
    public abstract class JSInput
    {
        public JSWrapper parent;

        public JSInput(JSWrapper parent)
        {
            this.parent = parent;
        }

        public abstract void SetValue(string obj, string data);
        public abstract Task<string> GetValue(string obj);
    }
}
