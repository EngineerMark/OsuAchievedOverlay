using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay.Next.Helpers
{
    //Kudos to Tillerino for this
    //didnt know how to reverse calculate the damn accuracy to hits
    //actual legend
    public class AccuracyDistribution
    {
        public int Hits300 { get; set; }
        public int Hits100 { get; set; }
        public int Hits50 { get; set; }
        public int Misses { get; set; }

        public AccuracyDistribution(int objectCount, int missCount, float accuracy){
            Tuple<int, int> best = GetBest300s(objectCount, missCount, accuracy);
            Hits300 = best.Item1;
            Hits100 = best.Item2;
            Hits50 = objectCount - Hits300 - Hits100 - Misses;
            Misses = missCount;
        }

        private Tuple<int, int> GetBest300s(int objectCount, int missCount, float accuracy)
        {
            accuracy *= objectCount * 6;

            int intAccuracy = (int)Math.Round(accuracy);
            int aomm = objectCount - missCount;

            int guess300s = (int)Math.Round((accuracy - aomm) / 5);
            int best300s = 0;
            int best100s = 0;
            double bestError = Double.PositiveInfinity;

            // what the fuck
            for(int _300s = Math.Max(0, guess300s-1);_300s<=Math.Min(aomm, guess300s+1);_300s++){
                int localAccuracy = Math.Min(2 * aomm + _300s * 4, Math.Max(aomm + _300s * 5, intAccuracy));
                int _100s = localAccuracy - (aomm + _300s * 5);
                double error = Math.Abs(accuracy - localAccuracy);
                if(error<bestError){
                    best300s = _300s;
                    bestError = error;
                    best100s = _100s;
                }
            }
            return new Tuple<int, int>(best300s, best100s);
        }
    }
}
