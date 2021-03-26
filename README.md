# OsuAchievedOverlay
Stream Overlay for osu! containing information on your session achievements

This is still very experimental. Crashes can and will occur. If not reported yet, throw it into Issues.

I only work on Windows builds.
If you have experience in other OS, feel free to make it possible to compile for them. Test it and if it's working perfectly, I will probably merge it in.

# OBS Studio

When capturing the window, set Window Match Priority to "Match title, otherwise find window of same executable", that way it automatically recaptures when you change settings.

# Dependencies

This project makes use of my OsuHelper library: https://github.com/EngineerMark/OsuHelper
It uses Newtonsoft for json parsing. Latest version should be safe, but in case of errors or crashes; check used version in NuGet package
