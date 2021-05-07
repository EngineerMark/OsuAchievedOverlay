using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OsuAchievedOverlay.Managers
{
    public class InputManager : Manager<InputManager>
    {
        public static KeyEventHandler OnKeyDown;
        public static KeyEventHandler OnKeyUp;

        public override void Start()
        {
        }

        public override void Stop()
        {
        }
    }
}
