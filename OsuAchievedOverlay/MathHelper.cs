namespace OsuAchievedOverlay
{
    public static class MathHelper
    {
        public static decimal Map(this decimal value, decimal fromSource, decimal toSource, decimal fromTarget, decimal toTarget)
        {
            return ((value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget);
        }

        public static int Map(this int value, int fromSource, int toSource, int fromTarget, int toTarget)
        {
            return ((value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget);
        }

        public static double Map(this double value, double fromSource, double toSource, double fromTarget, double toTarget)
        {
            return ((value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget);
        }
    }
}
