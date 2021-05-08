using osu.Shared;
using osu_database_reader.Components.Beatmaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OsuAchievedOverlay
{
    public static class BeatmapHelper
    {
        // Reverse iterate through it, sr above key value means its that color and then break
        // structure: sr, regular color, hover color
        public static List<Tuple<double, Brush, Brush>> difficultyColorMap = new List<Tuple<double, Brush, Brush>>(){
            new Tuple<double, Brush, Brush>(0, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#719115")), new SolidColorBrush((Color)ColorConverter.ConvertFromString("#88b300"))),
            new Tuple<double, Brush, Brush>(2, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#57a0c4")), new SolidColorBrush((Color)ColorConverter.ConvertFromString("#66ccff"))),
            new Tuple<double, Brush, Brush>(2.7, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c5a42e")), new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffcc22"))),
            new Tuple<double, Brush, Brush>(4, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c05889")), new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff66aa"))),
            new Tuple<double, Brush, Brush>(5.3, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#aa88ff")), new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8771c3"))),
            new Tuple<double, Brush, Brush>(6.5, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#212323")), new SolidColorBrush((Color)ColorConverter.ConvertFromString("#121415"))),
        };
        //{
        //    {{0, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#729113")) , null} },
        //    {2, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#59A1C5"))},
        //    {2.7, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C5A42E"))},
        //    {4, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C45B8C"))},
        //    {5.3, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8A74C9"))},
        //    {6.5, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C1E20"))},
        //};

        public static Tuple<double, Brush, Brush> GetColorFromDifficulty(double diff){
            List<Tuple<double, Brush, Brush>> reversedMap = new List<Tuple<double, Brush, Brush>>(difficultyColorMap);
            reversedMap.Reverse();
            foreach (Tuple<double, Brush, Brush> colorMap in reversedMap)
                if (diff > colorMap.Item1)
                    return colorMap;
            return difficultyColorMap[0];
        }

        public static BitmapImage IconStandard = new BitmapImage(new Uri("pack://application:,,,/OsuAchievedOverlay;component/Assets/Images/Icons/Gamemodes/osu.png"));
        public static BitmapImage IconTaiko = new BitmapImage(new Uri("pack://application:,,,/OsuAchievedOverlay;component/Assets/Images/Icons/Gamemodes/taiko.png"));
        public static BitmapImage IconMania = new BitmapImage(new Uri("pack://application:,,,/OsuAchievedOverlay;component/Assets/Images/Icons/Gamemodes/mania.png"));
        public static BitmapImage IconCatch = new BitmapImage(new Uri("pack://application:,,,/OsuAchievedOverlay;component/Assets/Images/Icons/Gamemodes/ctb.png"));

        public static BitmapImage GetGamemodeIcon(GameMode gamemode)
        {
            switch(gamemode){
                default:
                case GameMode.Standard:
                    return IconStandard;
                case GameMode.Taiko:
                    return IconTaiko;
                case GameMode.Mania:
                    return IconMania;
                case GameMode.CatchTheBeat:
                    return IconCatch;
            }
        }

        public static string GetGamemodeString(GameMode mode){
            switch (mode)
            {
                default:
                case osu.Shared.GameMode.Standard:
                    return "Standard";
                case osu.Shared.GameMode.Taiko:
                    return "Taiko";
                case osu.Shared.GameMode.Mania:
                    return "Mania";
                case osu.Shared.GameMode.CatchTheBeat:
                    return "Catch";
            }
        }

        public static double GetStarRating(BeatmapEntry map){
            double sr = -1;
            switch (map.GameMode)
            {
                case osu.Shared.GameMode.Standard:
                    sr = map.DiffStarRatingStandard[0];
                    break;
                case osu.Shared.GameMode.Taiko:
                    sr = map.DiffStarRatingTaiko[0];
                    break;
                case osu.Shared.GameMode.Mania:
                    sr = map.DiffStarRatingMania[0];
                    break;
                case osu.Shared.GameMode.CatchTheBeat:
                    sr = map.DiffStarRatingCtB[0];
                    break;
            }
            return sr;
        }
    }
}
