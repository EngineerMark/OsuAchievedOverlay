using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay
{
    public class WindowManager : Manager<WindowManager>
    {
        private BetaDisplayWindow betaDisplayWin = null;
        private MainWindow mainWin = null;
        private LoadSessionWindow sessionWin = null;
        private LocalApiWindow apiWin = null;

        public MainWindow MainWin { get => mainWin; set => mainWin = value; }
        public BetaDisplayWindow BetaDisplayWin { get => betaDisplayWin; set => betaDisplayWin = value; }
        public LoadSessionWindow SessionWin { get => sessionWin; set => sessionWin = value; }
        public LocalApiWindow ApiWin { get => apiWin; set => apiWin = value; }

        public override void Start()
        {
        }

        public override void Stop()
        {
        }
    }
}
