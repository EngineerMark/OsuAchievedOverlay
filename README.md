[![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/engineermark/osuachievedoverlay?include_prereleases)](https://github.com/EngineerMark/OsuAchievedOverlay/releases/latest)
![Total Downloads](https://img.shields.io/github/downloads/EngineerMark/OsuAchievedOverlay/total)
![Code Quality Dev Branch](https://img.shields.io/codefactor/grade/github/EngineerMark/OsuAchievedOverlay/dev)

# Table of Contents
1. [OsuAchievedOverlay](#OsuAchievedOverlay)
    1. [osu! API key](#osu!-API-key)
    2. [Release format](#release-format)
2. [Building](#building)
    1. [Dependencies](#dependencies)
3. [Sessions](#sessions)
4. [Local API](#local-api)
    1. [Reference](#reference)
5. [OBS Studio](#obs-studio)

# OsuAchievedOverlay
Stream Overlay for osu! containing information on your session achievements

This is still very experimental. Crashes can and will occur. If not reported yet, throw it into Issues.

I only work on Windows builds.
If you have experience in other OS, feel free to make it possible to compile for them. Test it and if it's working perfectly, I will probably merge it in.

## osu! API key

URLs to request an api key: https://osu.ppy.sh/p/api, https://old.ppy.sh/p/api

For some reason, they sometimes redirect you to the community forum. When that happens, retry later again, chances are you do get to the api page.

I don't know what causes this nor do I know a solution. Try with peppy maybe.

## Release format

Releases are currently versioned as the following: 1.2.3.4
1. Main version
2. Beta version
3. Release
4. Hotfix version (Only applicable for hotfixes)

This format will be standard here for the ease of the upcoming updater in the application.

# Building

To build osu!achieved for yourself, please make sure of the following things:
- ILMerge is used to combine the resulting managed dlls into the .exe
- 7zip is used to package the web resources into a single file

These are both done after the initial build, but are still part of the build process in Visual Studio 2019 (and works in other IDEs most likely, but I did not test that).
The first one does not happen if the target is set to Debug, the latter does.

## Dependencies

This project makes use of my OsuHelper library: https://github.com/EngineerMark/OsuHelper\
It uses Newtonsoft for json parsing. Latest version should be safe, but in case of errors or crashes; check used version in NuGet package

# Sessions
You only need to save a session once, the current and difference data are calculated automatically when opened.

Currently, incompatible session files (older versions for example), can still cause the program to crash instead of notify you.\
This will be fixed in the future.

# Local API

Local API in this context means the seperate files for certain stat data.
You can create your own formatting strings and files. It dumps it in pure text files ready to be read by other software.

Files are (created and) written to every update iteration (when the progress bar in display is full).\
They are never deleted. (Maybe something to consider next update, or later down the line).

The location of these files is ./api/file.extension\
The api folder is next to the exe file.

## Reference

| Key  | Description |
| ------------- | ------------- |
| `{new_ssh}` | Gained Silver SS Ranks |
| `{new_ss}` | Gained Golden SS Ranks |
| `{new_total_ss}` | Gained Total SS Ranks |
| `{new_sh}` | Gained Silver S Ranks |
| `{new_s}` | Gained Golden S Ranks
| `{new_total_s}` | Gained Total S Ranks
| `{new_a}` | Gained A Ranks |
| `{new_totalscore}` | Gained Total Score |
| `{new_rankedscore}` | Gained Ranked Score |
| `{new_playtime}` | Gained Playtime |
| `{new_pc}` | Gained Playcount |
|  |  |
| `{current_ssh}` | Current Silver SS Ranks |
| `{current_ss}` | Current Golden SS Ranks |
| `{current_total_ss}` | Current Total SS Ranks |
| `{current_sh}` | Current Silver S Ranks |
| `{current_s}` | Current Golden S Ranks
| `{current_total_s}` | Current Total S Ranks
| `{current_a}` | Current A Ranks |
| `{current_totalscore}` | Current Total Score |
| `{current_rankedscore}` | Current Ranked Score |
| `{current_playtime}` | Current Playtime |
| `{current_pc}` | Current Playcount |
|  |  |
| `{initial_ssh}` | Starting Silver SS Ranks |
| `{initial_ss}` | Starting Golden SS Ranks |
| `{initial_total_ss}` | Starting Total SS Ranks |
| `{initial_sh}` | Starting Silver S Ranks |
| `{initial_s}` | Starting Golden S Ranks
| `{initial_total_s}` | Starting Total S Ranks
| `{initial_a}` | Starting A Ranks |
| `{initial_totalscore}` | Starting Total Score |
| `{initial_rankedscore}` | Starting Ranked Score |
| `{initial_playtime}` | Starting Playtime |
| `{initial_pc}` | Starting Playcount |

Starting = value at start of the session

# OBS Studio

When capturing the window, set Window Match Priority to "Match title, otherwise find window of same executable", that way it automatically recaptures when you change settings.

---

# Header image creators

- Kyvrin.png created by [Kyvrin](https://osu.ppy.sh/users/11589256)
- placeholder_banner.jpeg by [ppy/osu!team](https://twitter.com/osugame/status/1127880443863289859)
- Porukana.png created by [Porukana](https://osu.ppy.sh/users/12992775)
- risuuna.png created by [risuuna](https://osu.ppy.sh/users/7266506)
- Sjao.png created by [Sjao](https://osu.ppy.sh/users/7295733)
- Tofumang.png created by [Tofumang](https://osu.ppy.sh/users/4817223)

