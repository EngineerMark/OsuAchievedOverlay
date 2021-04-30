using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace OsuAchievedOverlay
{
    public static class ProgressBarExtensions
    {
        public static void SetPercent(this ProgressBar progressBar, double percentage, TimeSpan duration)
        {
            DoubleAnimation animation = new DoubleAnimation(percentage, duration);
            progressBar.BeginAnimation(ProgressBar.ValueProperty, animation);
        }
    }
}
