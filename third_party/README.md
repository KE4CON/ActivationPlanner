# third_party — bundled engine staging area

Activation Planner shells out to two external programs and **bundles them in the installer**
(Decisions Log Item #19). This folder is where the built engine binaries are staged **per platform**
before packaging. The packaging script (`build/package.*`) copies the right platform's staging into
the distributable as the `tools/` folder that [`ExternalToolLocator`](../ActivationPlanner.UI/Composition/ExternalToolLocator.cs)
looks for beside the app.

> These binaries are **not** committed to git — they are build outputs. Produce them with
> `build/build-engines.ps1` (Windows) or `build/build-engines.sh` (macOS/Linux), or drop your own
> builds in by hand following the layout below.

## Required layout

Each supported [.NET Runtime Identifier (RID)](https://learn.microsoft.com/dotnet/core/rid-catalog)
gets its own subfolder. The packaging script picks the one matching the target it is publishing.

```
third_party/
  win-x64/
    voacap/
      voacapl.exe            # the VOACAP executable (ExternalToolLocator accepts voacapl(.exe)/VOACAPW.EXE)
      itshfbc/               # the VOACAP data directory (coeffs, antennas) — required
      *.dll                  # Cygwin/gfortran runtime DLLs the build needs beside the exe (if a Cygwin build)
    nec/
      nec2++.exe             # the NEC2 engine (ExternalToolLocator accepts nec2++(.exe)/nec2c(.exe))
  osx-x64/  |  osx-arm64/
    voacap/{voacapl, itshfbc/}
    nec/{nec2++}
  linux-x64/  |  linux-arm64/
    voacap/{voacapl, itshfbc/}
    nec/{nec2++}
```

## Why we build them ourselves

Redistribution of both engines is permitted (VOACAP is a U.S. Government work / CC0 port; NEC2++ is
GPLv2, redistributed as a separate shelled-out program — see
[`docs/THIRD_PARTY_LICENSES.md`](../docs/THIRD_PARTY_LICENSES.md)). Building from the public sources
(`jawatson/voacapl`, `tmolteno/necpp`) is the reliable cross-platform way to get correct binaries and,
for NEC2++, gives us the corresponding source we must offer under GPLv2.

## Licensing obligations (do not skip)

The packaging step also bundles `licenses/` (see the repo-root `licenses/` folder). The GPLv2
`COPYING` for NEC2++ is copied out of the necpp source during `build-engines`, so it always matches
the exact version shipped.
