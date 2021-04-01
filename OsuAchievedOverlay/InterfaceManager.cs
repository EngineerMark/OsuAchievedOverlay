using System.Windows.Controls;
using System.Windows.Media;

namespace OsuAchievedOverlay
{
    public class InterfaceManager : Manager<InterfaceManager>
    {
        public readonly string[] labelList = new string[]{
            "LabelBanchoDown", "LabelPreparing",
            "LabelSSCurrent", "LabelSSNew", 
            "LabelSCurrent", "LabelSNew",
            "LabelACurrent", "LabelANew",
            "LabelScoreCurrent", "LabelScoreNew",
            "LabelPlaycountCurrent", "LabelPlaycountNew",
            "LabelTimeAgoStarted",
            "LabelUsername",
            "LabelAveragePlayScore"
        };

        public override void Start()
        {
            //Not implemented
        }

        public override void Stop()
        {
            //Not implemented
        }

        private object FindItem(string name){
            return GameManager.Instance.DisplayWin.FindName(name);
        }

        private Label FindLabel(string name){
            return (Label)FindItem(name);
        }

        public void SetLabelColor(Color c){
            Brush b = new SolidColorBrush(c);

            for (int i = 0; i < labelList.Length; i++)
                FindLabel(labelList[i]).Foreground = b;
        }

        public void SetLabelFont(FontFamily font){
            for (int i = 0; i < labelList.Length; i++)
                FindLabel(labelList[i]).FontFamily = font;
        }
    }
}
