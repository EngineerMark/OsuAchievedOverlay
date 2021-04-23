using System.Threading;

namespace OsuAchievedOverlay.Interfaces
{
    internal interface IThreaded
    {
        public Thread Thread { get; set; }
    }
}