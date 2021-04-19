# OsuAchievedOverlay
Stream Overlay for osu! containing information on your session achievements

This is still very experimental. Crashes can and will occur. If not reported yet, throw it into Issues.

I only work on Windows builds.
If you have experience in other OS, feel free to make it possible to compile for them. Test it and if it's working perfectly, I will probably merge it in.

# Sessions
You only need to save an session once, the current and difference data are calculated automatically.

Currently, incompatible session files (older versions for example), can still cause the program to crash instead of notify you.\
This will be fixed in the future.

# Local API

Local API in this context means the seperate files for certain stat data.
You can create your own formatting strings and files. It dumps it in pure text files ready to be read by other software.

## Reference

| Key  | Description |
| ------------- | ------------- |
| `{new_ssh}` | Gained Silver SS Ranks |
| `{new_ss}` | Gained Golden SS Ranks |
| `{new_sh}` | Gained Silver S Ranks |
| `{new_s}` | Gained Golden S Ranks
| `{new_a}` | Gained A Ranks |
| `{new_totalscore}` | Gained Total Score |
| `{new_rankedscore}` | Gained Ranked Score |
| `{new_playtime}` | Gained Playtime |
| `{new_pc}` | Gained Playcount |
|  |  |
| `{current_ssh}` | Current Silver SS Ranks |
| `{current_ss}` | Current Golden SS Ranks |
| `{current_sh}` | Current Silver S Ranks |
| `{current_s}` | Current Golden S Ranks
| `{current_a}` | Current A Ranks |
| `{current_totalscore}` | Current Total Score |
| `{current_rankedscore}` | Current Ranked Score |
| `{current_playtime}` | Current Playtime |
| `{current_pc}` | Current Playcount |
|  |  |
| `{initial_ssh}` | Starting Silver SS Ranks |
| `{initial_ss}` | Starting Golden SS Ranks |
| `{initial_sh}` | Starting Silver S Ranks |
| `{initial_s}` | Starting Golden S Ranks
| `{initial_a}` | Starting A Ranks |
| `{initial_totalscore}` | Starting Total Score |
| `{initial_rankedscore}` | Starting Ranked Score |
| `{initial_playtime}` | Starting Playtime |
| `{initial_pc}` | Starting Playcount |

Starting = value at start of the session

# OBS Studio

When capturing the window, set Window Match Priority to "Match title, otherwise find window of same executable", that way it automatically recaptures when you change settings.

# Dependencies

This project makes use of my OsuHelper library: https://github.com/EngineerMark/OsuHelper\
It uses Newtonsoft for json parsing. Latest version should be safe, but in case of errors or crashes; check used version in NuGet package
