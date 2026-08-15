# Activation Planner — Installation Guide

*Download, run one setup command, and go — in plain language.*

*Generated August 14, 2026 · Markdown is the living source of truth.*


---


# 1. Before You Start

*What you're installing, what you need, and the whole install in a nutshell — so even a quick skim gets you there.*

> **IN A NUTSHELL** — 1) Go to the project's **Releases** page and download the one file for your system. 2) Unzip it. 3) Run the **one setup command** for your system (Windows, macOS, or Linux). 4) Press **Enter** at each prompt to accept the sensible default. That's it — Activation Planner opens and you can start planning. The rest of this guide just spells out those four steps for each system, with pictures of what you'll see.


## What This Guide Covers

This guide gets **Activation Planner** installed and running on your computer, step by step, in plain language. It covers **Windows, macOS, and Linux** — including **Raspberry Pi** — and both regular (Intel/AMD) and **ARM** processors. You don't need to be technical: the main path is a single setup command that asks you a few simple questions and does the rest.


## What You're Installing

You're installing **three things**, but the setup handles them together so it feels like one:

- **Activation Planner** — the app itself: the planning screens, your gear inventory, checklists, and so on.
- **VOACAP** — the **propagation-prediction engine** (the proven tool, originally from the Voice of America, that works out which bands will carry your signal and when). Activation Planner runs it behind the scenes.
- **NEC2++** — the **antenna-modeling engine** (Numerical Electromagnetics Code) that calculates how your antennas radiate.

> **WHY TWO EXTRA ENGINES?** — Activation Planner runs VOACAP and NEC2++ as **separate helper programs** rather than building their code into itself — that keeps their trusted results exact and honors their software licenses. You never open them yourself; the app drives them for you. The User Manual explains this in more depth; this guide just installs them. If they aren't installed, the app still runs — it simply shows **sample** predictions until they are (more on that later).

> **NO SEPARATE ".NET" INSTALL NEEDED** — Activation Planner is built on Microsoft's .NET, but the download is **self-contained** — everything it needs is included. You do **not** have to install .NET, Java, or anything else first. Just download and run the setup.


## What You Need — By Operating System

Activation Planner is light on requirements. Here's what each system needs. "GB" means **gigabyte** (a measure of storage/memory).

| System | Version | Processor | Space needed |
| --- | --- | --- | --- |
| Windows | Windows 10 or Windows 11 | 64-bit Intel/AMD (x64) or ARM (ARM64) | About 300 MB free disk |
| macOS | macOS 12 (Monterey) or newer | Intel or Apple Silicon (M1/M2/M3/M4) | About 300 MB free disk |
| Linux | A current 64-bit distribution (e.g. Ubuntu 22.04+, Debian 12+, Fedora) | 64-bit Intel/AMD (x64) or ARM (ARM64) | About 300 MB free disk |
| Raspberry Pi | Raspberry Pi OS (64-bit), Pi 4 or Pi 5 recommended | ARM64 (aarch64) | About 300 MB free disk |

A couple of gigabytes of memory (RAM) is plenty. There is **no graphics-card requirement** for normal use.


## What It Will Not Run On

So you don't waste time trying, here's what is **not** supported:

- **Phones and tablets** — iPhone, iPad, and Android are not supported. Activation Planner is a desktop/laptop app. (A phone/tablet version is a future project — see the roadmap.)
- **Very old 32-bit-only computers** — the standard downloads are 64-bit. Almost every computer from the last decade is 64-bit.
- **Windows 7 / 8 and very old macOS/Linux** — these are past their supported versions.

> **IS THIS A LAPTOP OR A DESKTOP APP? BOTH.** — It runs the same on a desktop or a laptop — and a laptop is what you'll usually take to the field. A Windows tablet (like a Surface) counts as Windows and works too. It's a genuine planning tool you can also carry with you.


## What Needs the Internet

Once installed, Activation Planner does its core job — band and antenna planning — **offline**. A few live features need an internet connection (a phone hotspot in the field is fine):

- **Live solar data** (to auto-fill current conditions), **weather forecast and alerts**, and **POTA spots** all need the internet.
- **Core planning, antenna patterns, checklists, and PDF export** work with no connection once the app and engines are installed.


## A Word About the Download Being Safe

Because Activation Planner is a free, independent app (not sold through the Microsoft Store or the Mac App Store), your computer may show a caution the first time you run it — this is normal for smaller apps and does **not** mean anything is wrong. Each operating system's chapter below tells you the exact, safe way to get past that message. You can also see the full source code, since the project is open source, at the project's page on GitHub (github.com/KE4CON/ActivationPlanner).


## Ready?

Pick your system and go to its chapter: **Windows**, **macOS**, or **Linux / Raspberry Pi**. Each one spells out every step and shows you what you'll see. Then the **Verifying Your Install** chapter confirms everything is working.


# 2. Install on Windows

*Download one file, run one setup command, answer a few Enter-key prompts, and you're done.*

> **IN A NUTSHELL** — Download the Windows **.zip** from the project's Releases page, right-click it and choose **Extract All**, then in the extracted folder right-click **install.ps1** and choose **Run with PowerShell**. Press **Enter** at each question to accept the default. Activation Planner installs and opens. If a blue "Windows protected your PC" box appears, click **More info** then **Run anyway** - that's normal for a new app.


## Step 1 - Download the Right File

Open the project's **Releases** page in your web browser: **https://github.com/KE4CON/ActivationPlanner/releases**. Under the newest release, find the file for Windows and click it to download. Pick by your processor type:

| Your PC | File to download |
| --- | --- |
| A normal Intel or AMD PC (almost everyone) | ActivationPlanner-<version>-win-x64.zip |
| A Windows-on-ARM PC (e.g. some Surface / Snapdragon laptops) | ActivationPlanner-<version>-win-arm64.zip |

Here "<version>" is the release number, for example **ActivationPlanner-1.0.0-win-x64.zip**. Not sure which processor you have? Press the **Windows key**, type **About your PC**, open it, and read the **System type** line - it says 64-bit and whether it's ARM. When in doubt, choose the **x64** file.

> **WHERE DID IT GO?** — By default the file lands in your **Downloads** folder. You'll go there in the next step.


## Step 2 - Unzip It

1. Open your **Downloads** folder (press the **Windows key**, type **Downloads**, press Enter).
2. Find the file you just downloaded (for example **ActivationPlanner-1.0.0-win-x64.zip**).
3. **Right-click** it and choose **Extract All...**
4. A small window appears. Click **Extract** (the default location is fine).
5. A new folder opens showing the files, including one named **ActivationPlanner.UI.exe** and one named **install.ps1**.

> **YOU MUST EXTRACT FIRST** — Don't try to run anything from inside the .zip while it's still zipped - Windows only shows a preview there. Use **Extract All** first, then work in the real folder it creates.


## Step 3 - Run the Setup

In the extracted folder, you'll run the one-step setup script, **install.ps1**. The easiest way:

1. **Right-click** the file named **install.ps1**.
2. Choose **Run with PowerShell** from the menu.
3. A blue text window opens and the setup starts asking you a few simple questions (covered below).

> **IF "RUN WITH POWERSHELL" IS MISSING OR BLOCKED** — Some PCs restrict running scripts. If you don't see the option, or you get a red "running scripts is disabled" message, do this instead: press the **Windows key**, type **PowerShell**, open it, then type this line and press Enter (replace the path with your extracted folder):  powershell -ExecutionPolicy Bypass -File "C:\Users\YourName\Downloads\ActivationPlanner-1.0.0-win-x64\install.ps1"  . A quick way to get the exact path: in the folder, hold **Shift**, right-click **install.ps1**, choose **Copy as path**, and paste it after -File.


## Getting Past "Windows Protected Your PC"

Because Activation Planner is a new independent app, Windows SmartScreen may show a **blue box titled "Windows protected your PC."** This does not mean anything is wrong - it just means the app isn't widely known yet. To continue:

1. Click the small **More info** link in the blue box.
2. A **Run anyway** button appears. Click it.
3. Setup continues normally.

> **SIGNED BUILDS SKIP THIS** — Once Activation Planner is distributed as a **code-signed** build, this blue screen won't appear at all - Windows will recognize the publisher. Until then (or if you have an unsigned build), the **More info -> Run anyway** steps above are the normal, safe way through.


## Answering the Setup Prompts

The setup asks a handful of questions. **Press Enter to accept the default** shown in brackets for any you're unsure about. Here's each one:

| Prompt | What it means / what to do |
| --- | --- |
| License notices, "Press Enter to accept" | It shows that VOACAP and NEC2++ are included under their own licenses. Press **Enter** to acknowledge and continue. |
| Install location [C:\Users\...\Activation Planner] | Where to install. Press **Enter** for the default, or type a different folder. |
| That folder exists. Overwrite it? (y/n) [y] | Only appears if you're reinstalling. Press **Enter** (yes) to replace the old copy. |
| Create a Start Menu shortcut? (y/n) [y] | Press **Enter** for yes so you can find it in the Start menu later. |
| Launch Activation Planner now? (y/n) [y] | Press **Enter** to open it right away. |

After the prompts, you'll see a line for each engine: whether **VOACAP** and **NEC2++** were bundled (real predictions) or not (sample mode). Both being "NOT bundled" is fine to start - the app still runs; see the **Helper Engines** chapter.


## What You'll See When It's Done

The window prints **"Setup complete"** and the folder it installed to. If you said yes to launching, Activation Planner opens to its first screen. From now on, start it any time from the **Start menu** (type **Activation Planner**) or the shortcut you created. You do not need the downloaded .zip anymore - you can delete it.


## Troubleshooting

| Symptom | What to do |
| --- | --- |
| "Running scripts is disabled on this system" | Use the PowerShell command shown above with **-ExecutionPolicy Bypass**. It runs this one script without changing your PC's settings. |
| Blue "Windows protected your PC" box | Click **More info**, then **Run anyway**. Normal for a new app; a signed build removes it. |
| "Cannot find ActivationPlanner.UI.exe" | You ran install.ps1 from the wrong place, or didn't extract the .zip. Extract with **Extract All** first, then run install.ps1 from inside the extracted folder. |
| It says engines are NOT bundled / sample mode | The predictions are placeholders until VOACAP/NEC2++ are included. See the **Helper Engines** chapter. The app still works for learning your way around. |
| Nothing happens when I double-click install.ps1 | Double-clicking opens it in an editor. **Right-click** it and choose **Run with PowerShell** instead. |
| Windows Defender / antivirus flags it | Unsigned apps are sometimes flagged by heuristics. A signed build avoids this. If you trust your download from the official Releases page, allow it; otherwise re-download from the official page. |


# 3. Install on macOS

*Download one file, run one setup command in Terminal, and get past the one-time "unidentified developer" message.*

> **IN A NUTSHELL** — Download the macOS **.tar.gz** for your Mac (Apple Silicon or Intel) from the Releases page. Double-click it to expand it into a folder. Open **Terminal**, type  cd  and the folder's path, then run  bash install.sh  and press **Enter** at each prompt. If macOS says the app is from an "unidentified developer," right-click the app and choose **Open**, then **Open** again - a one-time step for new apps.


## Which File to Download

First find out which Mac you have: click the **Apple menu** (top-left) then **About This Mac**. Look at the **Chip** or **Processor** line.

| Your Mac | File to download |
| --- | --- |
| Apple Silicon (M1, M2, M3, M4 - most Macs since 2020) | ActivationPlanner-<version>-osx-arm64.tar.gz |
| Intel (older Macs, "Intel Core" processor) | ActivationPlanner-<version>-osx-x64.tar.gz |

"<version>" is the release number, for example **ActivationPlanner-1.0.0-osx-arm64.tar.gz**.


## Step 1 - Download and Expand

1. Open **https://github.com/KE4CON/ActivationPlanner/releases** in Safari.
2. Under the newest release, click the correct **.tar.gz** file for your Mac. It downloads to your **Downloads** folder.
3. Open **Downloads** (in Finder), and **double-click** the .tar.gz file. macOS expands it into a folder next to it (for example **ActivationPlanner-osx-arm64**).


## Step 2 - Run the Setup in Terminal

macOS installs this kind of app with a short command. Open **Terminal** first: press **Command + Space**, type **Terminal**, and press **Enter**.

1. In Terminal, type  cd  followed by a space (don't press Enter yet).
2. Now type the path to the expanded folder. For most people that's:  cd ~/Downloads/ActivationPlanner-osx-arm64   (change to osx-x64 if that's what you downloaded), then press **Enter**.
3. Run the setup by typing:  bash install.sh   and pressing **Enter**.
4. The setup starts asking a few simple questions (covered below).

> **TIP - GET THE FOLDER PATH EXACTLY** — If the folder name is slightly different, type  cd ~/Downloads/  then press the **Tab** key to let Terminal complete the folder name for you. Tab-completion avoids typos.


## Getting Past "Unidentified Developer"

The first time you open a new independent app, macOS Gatekeeper may say it **"cannot be opened because it is from an unidentified developer"** (or is from the internet). This is expected for a new app and does not mean anything is wrong. The safe way through:

1. In Finder, open your installed folder and find **ActivationPlanner.UI** (the app).
2. **Right-click** (or Control-click) it and choose **Open**.
3. A dialog appears with an **Open** button - click it. macOS remembers your choice, so you only do this once.
4. If you don't see an Open button, go to the **Apple menu -> System Settings -> Privacy & Security**, scroll down, and click **Open Anyway** next to the Activation Planner message.

> **SIGNED + NOTARIZED BUILDS SKIP THIS** — Once Activation Planner is distributed as a **signed and notarized** build, this message won't appear - macOS will trust it automatically. Until then (or for an unsigned build), the right-click -> Open steps above are the normal, safe way through. As a last resort you can clear the "downloaded from the internet" flag in Terminal:  xattr -dr com.apple.quarantine ~/ActivationPlanner  .


## Answering the Setup Prompts

Press **Enter** to accept the default in brackets for anything you're unsure about.

| Prompt | What it means / what to do |
| --- | --- |
| License notices, "Press Enter to accept" | Shows that VOACAP and NEC2++ are included under their own licenses. Press **Enter** to continue. |
| Install location [/Users/you/ActivationPlanner] | Where to install. Press **Enter** for the default, or type another folder. |
| That folder exists. Overwrite it? (y/n) [y] | Only when reinstalling. Press **Enter** to replace the old copy. |
| Launch Activation Planner now? (y/n) [y] | Press **Enter** to open it. |

You'll also see whether **VOACAP** and **NEC2++** were bundled. If not, the app runs in sample mode until they are - see the **Helper Engines** chapter.


## Troubleshooting

| Symptom | What to do |
| --- | --- |
| "cannot be opened because it is from an unidentified developer" | Right-click the app -> **Open** -> **Open**; or System Settings -> Privacy & Security -> **Open Anyway**. One-time step; a notarized build removes it. |
| "command not found: bash" or the cd path is wrong | Make sure you expanded the .tar.gz (double-click it) and used the real folder name. Use **Tab** to auto-complete the folder name after  cd ~/Downloads/ . |
| "Permission denied" running the app | The setup marks it runnable, but if needed run:  chmod +x ~/ActivationPlanner/ActivationPlanner.UI  . |
| It says engines are NOT bundled / sample mode | Predictions are placeholders until VOACAP/NEC2++ are included. See the **Helper Engines** chapter. The app still runs. |
| The app bounces in the Dock and quits | This is usually the quarantine flag. Clear it with  xattr -dr com.apple.quarantine ~/ActivationPlanner  then open it again. |


# 4. Install on Linux (including Raspberry Pi)

*Download one file, run one setup command, and (optionally) get a menu entry. Works on regular PCs and on ARM boards like the Raspberry Pi.*

> **IN A NUTSHELL** — Download the Linux **.tar.gz** for your processor from the Releases page, expand it (double-click, or  tar -xzf <file> ), open a terminal in that folder, and run  bash install.sh  . Press **Enter** at each prompt. There's no "unknown publisher" warning on Linux - it just installs. On a Raspberry Pi, use the **arm64** file.


## Which File to Download

Pick by your processor. Not sure? Open a terminal and run  uname -m  : if it prints **x86_64** you want x64; if it prints **aarch64** you want arm64.

| Your computer | uname -m | File to download |
| --- | --- | --- |
| A regular Intel/AMD PC or laptop | x86_64 | ActivationPlanner-<version>-linux-x64.tar.gz |
| An ARM board (Raspberry Pi 4/5, other 64-bit ARM) | aarch64 | ActivationPlanner-<version>-linux-arm64.tar.gz |

> **OLD 32-BIT ARM** — If you're on an old 32-bit Raspberry Pi OS (uname -m shows **armv7l**, e.g. a Pi Zero or Pi 2), that's the separate **linux-arm** build. It can work, but we strongly recommend the 64-bit Raspberry Pi OS on a Pi 4 or Pi 5 and the **arm64** file - the app runs much better there.


## A Word on Raspberry Pi

Activation Planner runs on a Raspberry Pi running the 64-bit Raspberry Pi OS desktop - a genuinely handy field option in a Pi laptop/netbook build. Use the **linux-arm64** download. One extra note: to get **real** predictions (not sample data), the two helper engines must be built on the Pi itself (they're ARM programs). The **Helper Engines** chapter and the maintainer appendix cover that; the app installs and runs either way.


## Step 1 - Download and Expand

1. Open **https://github.com/KE4CON/ActivationPlanner/releases** in your browser and download the correct **.tar.gz**.
2. Expand it. In your file manager you can usually right-click -> **Extract Here**. Or in a terminal:  cd ~/Downloads  then  tar -xzf ActivationPlanner-1.0.0-linux-x64.tar.gz  (use your actual file name).
3. This creates a folder such as **ActivationPlanner-linux-x64** containing the app and **install.sh**.


## Step 2 - Run the Setup

1. Open a terminal in that folder (in many file managers: right-click -> **Open Terminal Here**), or  cd  into it.
2. Run:  bash install.sh   and press **Enter**.
3. Answer the few prompts (below). That's it.


## Answering the Setup Prompts

Press **Enter** to accept the default in brackets for anything you're unsure about.

| Prompt | What it means / what to do |
| --- | --- |
| License notices, "Press Enter to accept" | Shows that VOACAP and NEC2++ are included under their own licenses. Press **Enter**. |
| Install location [/home/you/ActivationPlanner] | Where to install. Press **Enter** for the default. |
| That folder exists. Overwrite it? (y/n) [y] | Only when reinstalling. Press **Enter** to replace it. |
| Create an application menu entry? (y/n) [y] | Linux only. Press **Enter** so Activation Planner appears in your apps menu. |
| Launch Activation Planner now? (y/n) [y] | Press **Enter** to open it. |


## If the App Won't Start - Missing System Libraries

On a very minimal Linux install, the graphical toolkit may need a few common libraries that aren't present yet. If the app fails to open, install the usual desktop libraries:

- **Debian / Ubuntu / Raspberry Pi OS:**  sudo apt update && sudo apt install -y libice6 libsm6 libfontconfig1 libx11-6  
- **Fedora:**  sudo dnf install -y libICE libSM fontconfig libX11  

Most desktop systems already have these, so you likely won't need this step.


## Troubleshooting

| Symptom | What to do |
| --- | --- |
| I downloaded the wrong architecture | Run  uname -m . x86_64 -> the x64 file; aarch64 -> the arm64 file. Re-download the matching one. |
| "Permission denied" launching the app | Run:  chmod +x ~/ActivationPlanner/ActivationPlanner.UI  , then start it again. |
| App exits immediately / library errors | Install the desktop libraries listed above for your distribution. |
| It says engines are NOT bundled / sample mode | Predictions are placeholders until VOACAP/NEC2++ are built for your machine. See the **Helper Engines** chapter and the maintainer appendix. |
| No menu entry appeared | You answered 'n', or your desktop caches the menu. Log out and back in, or start it directly:  ~/ActivationPlanner/ActivationPlanner.UI  . |


# 5. The Two Helper Engines (VOACAP & NEC2++)

*What the two bundled engines do, how to tell if they're active, and the license notices you'll see - explained plainly.*

> **IN A NUTSHELL** — Activation Planner uses two helper programs behind the scenes: **VOACAP** (which bands will work, and when) and **NEC2++** (how your antennas radiate). If they came bundled with your download, you get **real** predictions. If not, the app runs in **sample mode** - fully usable to learn the app, but the numbers are placeholders until the engines are added. You never open these programs yourself.


## What They Are

Activation Planner does the planning; the two engines do the heavy science:

- **VOACAP** - the **propagation-prediction engine** (originally from the Voice of America). It works out which bands will actually carry your signal to your target, hour by hour. It powers the band recommendations.
- **NEC2++** - the **antenna-modeling engine** (Numerical Electromagnetics Code). It calculates your antenna's radiation pattern - where your signal goes and at what angle - from the measurements you enter.

> **WHY SEPARATE PROGRAMS?** — Activation Planner runs VOACAP and NEC2++ as their own programs rather than copying their code inside itself. That keeps their trusted results exact and unmodified, and it honors their software licenses cleanly (especially NEC2++, which is shared under terms that are respected precisely by keeping it a standalone program the planner simply calls). You never launch them - the app drives them for you and turns their output into plain-English advice.


## Sample Mode vs Real Predictions

You can always tell which mode you're in:

| What you see | What it means |
| --- | --- |
| An orange "sample data" banner on the planning screen | VOACAP isn't active - band numbers are illustrative placeholders. |
| A "representative pattern" note on the antenna pattern screen | NEC2++ isn't active - antenna patterns are approximate stand-ins. |
| No such banners/notes | Both engines are active - you're seeing real predictions and real modeled patterns. |

The setup script also tells you at install time: it prints whether **VOACAP** and **NEC2++** were bundled. Sample mode is a perfectly good way to learn the app; for real planning you'll want the engines active.


## How the Engines Get Onto Your Computer

The engines live in a **tools** folder next to the app (the app looks there automatically). There are two ways they get there:

- **Bundled in your download (the easy way).** If the release you downloaded was built with the engines included, they're already in place and predictions are real from the first launch - nothing to do.
- **Added afterward (for your platform).** If your download shipped without them (sample mode), they can be built for your computer and dropped into the tools folder. This is a one-time, more technical step covered in the **Build It Yourself / For Maintainers** appendix.

> **WHY A DOWNLOAD MIGHT NOT INCLUDE THEM** — The engines are compiled programs that differ per operating system and processor. A given release may include them for some platforms and not others. Raspberry Pi and other ARM builds, in particular, are usually built on the device itself.


## The License Notices at Install

During setup you're shown - and asked to accept - short notices about the two engines. This is required by their licenses and is nothing to worry about. In plain terms:

- **VOACAP** is a U.S. Government work (from NTIA/ITS), not subject to U.S. copyright, with the porting changes placed in the public domain. A standard government disclaimer is included.
- **NEC2++** is shared under the **GNU General Public License, version 2 (GPLv2)**. Because Activation Planner runs it as a separate program, including it is fully permitted. Its license text and an offer of its source code are included with the install.

The full notices are placed in a **licenses** folder inside your installation, and are summarized in this guide's Licenses section and in the project's THIRD_PARTY_LICENSES document.


## Getting Real Predictions If You're in Sample Mode

If your install shows sample mode and you want real predictions, you (or someone helping you) can build the engines for your machine and place them in the app's **tools** folder. The steps are in the **Build It Yourself / For Maintainers** appendix - it's a one-time process using the included build scripts. Once the engines are in place, restart Activation Planner and the sample banners disappear.

> **THE APP ALWAYS RUNS EITHER WAY** — Missing engines never stop the app from opening. You can set up your gear, explore every screen, and learn the workflow in sample mode, then switch to real predictions whenever the engines are added.


# 6. Verifying Your Install

*A two-minute check that everything installed correctly - and how to tell real predictions from sample data.*

> **IN A NUTSHELL** — Open Activation Planner. If it starts and you can reach the planning screen, the install worked. Look for an orange **"sample data"** banner: if it's absent, the propagation engine is active and predictions are real; if it's present, you're in sample mode (still fine to use). That's the whole check.


## Step 1 - Open the App

- **Windows:** Start menu -> type **Activation Planner** -> open it (or use the shortcut you made).
- **macOS:** open the **ActivationPlanner.UI** app in your install folder (right-click -> Open the first time).
- **Linux:** find **Activation Planner** in your apps menu, or run  ~/ActivationPlanner/ActivationPlanner.UI  .

The very first time, you'll see the **setup wizard** for entering your gear. That's expected - it only appears once. You can go through it or press **Skip** for now; either way, reaching that screen means the app installed and runs.


## Step 2 - The Quick Health Check

| Check | Good sign |
| --- | --- |
| The app window opens | Install succeeded and the app runs on your system. |
| You can move between the navigation tabs | The interface is working normally. |
| The clock at the top shows your local time over UTC | The app is live and running. |


## Step 3 - Are the Engines Active?

Go to the **Plan session** tab and generate a plan (or open **Quick plan**). Then check:

- **No orange "sample data" banner** -> the **VOACAP** propagation engine is active; the band numbers are real predictions.
- **An orange "sample data" banner** -> you're in **sample mode**; the numbers are placeholders. The app is fully usable; see the **Helper Engines** chapter to switch to real predictions.
- On the **Antenna Patterns** tab, a **"representative pattern"** note means the **NEC2++** engine isn't active yet; no note means real modeled patterns.


## Step 4 - A First Real Plan

To confirm end-to-end operation, try a real plan:

1. On **Plan session**, press **Use my location** (or type your latitude/longitude, or enter your grid square).
2. Set a target you'd like to reach.
3. Press **Generate plan**.
4. You should see bands listed with reliability scores, best hours, and (if you've added antennas) matched antennas.

If that works, you're fully installed and ready. The User Manual explains every screen and number in depth.


## If Something's Not Right

| Symptom | Where to look |
| --- | --- |
| The app won't open at all | See the Troubleshooting chapter for your operating system's fixes (Windows SmartScreen, macOS Gatekeeper, Linux libraries). |
| Everything says sample data | Expected if the engines weren't bundled. See the **Helper Engines** chapter and the maintainer appendix to add them. |
| Location is wrong (on a hotspot) | Type your latitude/longitude or grid square instead of auto-location. The User Manual's Location chapter explains why. |
| No antennas are suggested | Your inventory is empty - add antennas on the Inventory screen (see the User Manual). |


# 7. Keeping It Running - Updates and Backups

*How to update to a new version, reinstall safely, and - most importantly - back up your gear inventory so you never lose it.*

> **IN A NUTSHELL** — To update, download the new release and run the setup again - it replaces the app but leaves your saved gear alone, because your inventory is stored separately in your user profile. To be safe, copy that one file (gear-inventory.json) somewhere before big changes or when moving computers.


## Updating to a New Version

Updating is the same as installing - the setup replaces the old copy:

1. Download the newest release for your system from **https://github.com/KE4CON/ActivationPlanner/releases**.
2. Run the setup the same way you did the first time (install.ps1 on Windows, bash install.sh on macOS/Linux).
3. When it asks **"That folder exists. Overwrite it?"**, press **Enter** (yes) to replace the old version.
4. Your **gear inventory is not touched** - it lives in a separate place (below), so your radios, antennas, and settings carry over automatically.


## Where Your Gear Inventory Is Stored

Your inventory (everything you entered - radios, antennas, and their measurements) is saved in **one file**, kept in your user profile, separate from the app itself. That's why reinstalling or updating never loses it. The file is:

| System | Location of gear-inventory.json |
| --- | --- |
| Windows | C:\Users\<you>\AppData\Roaming\ActivationPlanner\gear-inventory.json |
| macOS | /Users/<you>/.config/ActivationPlanner/gear-inventory.json |
| Linux | /home/<you>/.config/ActivationPlanner/gear-inventory.json |

> **FINDING THE HIDDEN FOLDER** — On Windows, paste  %APPDATA%\ActivationPlanner  into the File Explorer address bar to jump straight there. On macOS/Linux the  .config  folder is hidden; in a file manager press **Ctrl+H** (Linux) or **Command+Shift+.** (macOS Finder) to show hidden folders, or use a terminal.


## Backing Up Your Inventory

Because it's a single file, backing up is easy - and worth doing if you've spent time entering antenna measurements:

1. Go to the folder shown above for your system.
2. **Copy** the file **gear-inventory.json** to somewhere safe - a USB stick, a cloud folder, or another drive.
3. That copy is your complete backup. To restore, copy it back into the same folder.

> **BACK UP BEFORE BIG CHANGES** — Make a copy before experimenting with a lot of gear edits, before updating across a major version, or before moving computers. Restoring is just copying the file back.


## Moving to a New Computer

1. Install Activation Planner on the new computer (this guide's steps for that system).
2. Copy your **gear-inventory.json** from the old computer's folder (above) to the **same folder** on the new one, replacing the empty one created at first run.
3. Start Activation Planner - all your gear is there.

The inventory file is plain text and works across Windows, macOS, and Linux, so you can move it between different kinds of computers freely.


## Reinstalling Safely

If something goes wrong with the app itself, you can reinstall without losing your gear:

1. (Optional but wise) Copy your **gear-inventory.json** somewhere safe first.
2. Run the setup again and choose to overwrite the existing folder.
3. Your inventory - stored separately - is still there when the app reopens.

> **A TRULY FRESH START** — If you ever want to wipe everything and start over, close the app and delete the  ActivationPlanner  folder shown in the table above (that removes your saved inventory). The next launch begins with the empty setup wizard again.


# 8. Uninstalling

*How to remove Activation Planner cleanly - and how to decide whether to keep or delete your saved gear.*

> **IN A NUTSHELL** — Activation Planner doesn't spread files across your system. To uninstall, just delete the folder you installed it to (and the Start Menu shortcut or menu entry, if you made one). Your saved gear lives in a separate small folder - delete that too only if you want to erase your inventory.


## The Two Parts of an Install

There are only two things to remove, and they're separate on purpose:

- **The app folder** - where you installed Activation Planner (it contains the app, the engines, and the license notices).
- **Your gear data** - the small  ActivationPlanner  folder in your user profile holding  gear-inventory.json  (see the previous chapter for its exact location).

Removing the app folder uninstalls the program. Your gear data is left behind unless you also delete it - which is handy if you plan to reinstall later.


## Removing the App on Windows

1. Open the folder you installed to (default: press Windows key, type  %LOCALAPPDATA%\Programs  , open it, and find **Activation Planner**).
2. Delete the **Activation Planner** folder.
3. Remove the Start Menu shortcut if you created one: press Windows key, type  shell:programs  , open it, and delete the **Activation Planner** shortcut.


## Removing the App on macOS

1. Open your install folder (default:  /Users/<you>/ActivationPlanner  ).
2. Drag the **ActivationPlanner** folder to the Trash (or right-click -> Move to Trash).
3. Empty the Trash when you're ready.


## Removing the App on Linux

1. Delete your install folder:  rm -rf ~/ActivationPlanner   (or delete it in your file manager).
2. Remove the menu entry if you created one:  rm -f ~/.local/share/applications/activation-planner.desktop  .


## Keeping or Deleting Your Gear Inventory

Your inventory is **not** removed when you delete the app folder - it stays in your user profile so a future reinstall picks it back up. Decide based on what you want:

| You want to... | Do this |
| --- | --- |
| Reinstall later and keep your gear | Leave the inventory folder alone. Don't delete it. |
| Erase everything, including your gear | Also delete the  ActivationPlanner  folder in your user profile (Windows:  %APPDATA%\ActivationPlanner ; macOS/Linux:  ~/.config/ActivationPlanner ). |
| Keep a backup just in case | Copy  gear-inventory.json  somewhere safe before deleting anything (see the previous chapter). |

> **NOTHING ELSE TO CLEAN UP** — Activation Planner doesn't install system services, drivers, or registry-wide changes. Removing the app folder and (optionally) the gear folder leaves your system clean.


# 9. Troubleshooting

*The install problems people hit most, with plain fixes - grouped so you can jump to yours.*

> **START HERE** — Most install issues are one of three things: you didn't unzip before running the setup; your system showed a normal "new app" caution you need to click through; or the helper engines weren't bundled (sample mode). All three are covered below.


## Download and Unzip Problems

| Symptom | Fix |
| --- | --- |
| "Cannot find ActivationPlanner.UI" when running setup | You didn't extract the download first. Unzip (Windows: **Extract All**) or expand (macOS/Linux: double-click or  tar -xzf ) it, then run the setup from inside the extracted folder. |
| I downloaded the wrong file for my computer | Match your processor: Windows x64 vs ARM; macOS Apple Silicon (osx-arm64) vs Intel (osx-x64); Linux  uname -m  x86_64 -> x64, aarch64 -> arm64. Re-download the right one. |
| The download seems corrupted / setup errors oddly | Re-download from the official Releases page; a partial download can fail to extract. |


## Windows-Specific

| Symptom | Fix |
| --- | --- |
| Blue "Windows protected your PC" | Click **More info** -> **Run anyway**. Normal for a new/unsigned app; a signed build removes it. |
| "Running scripts is disabled on this system" | Run the setup with:  powershell -ExecutionPolicy Bypass -File "<path>\install.ps1"  (Shift+right-click install.ps1 -> **Copy as path** to get the path). |
| Double-clicking install.ps1 opens an editor | Right-click it -> **Run with PowerShell** instead. |
| Antivirus flags the app | Unsigned apps can trip heuristics. Use a signed build, or allow it if you trust your official download. |


## macOS-Specific

| Symptom | Fix |
| --- | --- |
| "Unidentified developer" / can't be opened | Right-click the app -> **Open** -> **Open**; or System Settings -> Privacy & Security -> **Open Anyway**. One-time; a notarized build removes it. |
| App bounces in the Dock then quits | Clear the internet-quarantine flag:  xattr -dr com.apple.quarantine ~/ActivationPlanner  , then open again. |
| "Permission denied" | chmod +x ~/ActivationPlanner/ActivationPlanner.UI  . |
| cd can't find the folder | Use **Tab** to auto-complete after  cd ~/Downloads/  , and make sure you expanded the .tar.gz first. |


## Linux / Raspberry Pi-Specific

| Symptom | Fix |
| --- | --- |
| App exits immediately / library errors | Install desktop libraries: Debian/Ubuntu/Pi OS:  sudo apt install -y libice6 libsm6 libfontconfig1 libx11-6  ; Fedora:  sudo dnf install -y libICE libSM fontconfig libX11  . |
| "Permission denied" launching | chmod +x ~/ActivationPlanner/ActivationPlanner.UI  . |
| Wrong architecture downloaded | uname -m : x86_64 -> linux-x64, aarch64 -> linux-arm64. On old 32-bit Pi OS (armv7l) prefer switching to 64-bit Pi OS. |
| No menu entry | Log out/in to refresh the menu, or run  ~/ActivationPlanner/ActivationPlanner.UI  directly. |


## "Sample Data" and the Engines

| Symptom | Fix |
| --- | --- |
| Orange "sample data" banner on the plan | VOACAP isn't active. Your build shipped without it; see the **Helper Engines** chapter and the maintainer appendix to build/add it. The app still works. |
| "Representative pattern" note on antenna patterns | NEC2++ isn't active - same story as above; patterns are approximate until it's added. |
| I added the engines but still see sample data | Make sure they're in the app's  tools/voacap  and  tools/nec  folders (with VOACAP's  itshfbc  data folder), then fully restart the app. On Windows, Cygwin/MSYS builds also need their runtime DLLs beside the .exe. |


## Location and Internet

| Symptom | Fix |
| --- | --- |
| Location is wrong, especially on a phone hotspot | Auto-location is a rough guess over a hotspot. Type your latitude/longitude, or your grid square, on the planning screen (see the User Manual). |
| No live solar / weather / POTA data | Those need an internet connection (a hotspot is fine). Core planning still works offline once the engines are installed. |


## Still Stuck?

- Re-read the chapter for your operating system - each step says what you should see.
- Confirm you downloaded the file that matches your processor.
- For engine/sample-mode questions, see the **Helper Engines** chapter and the **Build It Yourself** appendix.
- For how to actually use a feature once installed, see the **User Manual**.


# 10. Appendix - Build It Yourself / For Maintainers

*How the release packages are made: building the engines, packaging per platform, and (optionally) code-signing. More technical than the rest of this guide.*

> **IN A NUTSHELL** — From a clone of the public repo: run  build-engines  to compile VOACAP + NEC2++ into  third_party/<rid>/ , then run  package  to publish the self-contained app, bundle those engines and the license notices, and produce a ready-to-ship archive in  dist/ . Signing is optional and off by default. Do each OS's build on that OS.


## Who This Appendix Is For

Everyone else can ignore this - the normal way to install is to download a ready-made release and run the setup. This appendix is for a maintainer (or a technical user on a platform without a prebuilt release, such as Raspberry Pi) who wants to **produce** a distributable, or add the helper engines to a sample-mode install. The project is open source, so anyone can do this from the public source at **https://github.com/KE4CON/ActivationPlanner**.


## The Pieces and Where They Live

| Item | What it does |
| --- | --- |
| build/build-engines.sh | Clones and builds voacapl + necpp into third_party/<rid>/ (macOS/Linux, and Windows via MSYS2). |
| build/build-engines.ps1 | Windows helper that runs build-engines.sh under an MSYS2 bash. |
| build/package.ps1 | Windows: publishes self-contained, bundles engines + licenses, signs (optional), zips to dist/. |
| build/package.sh | macOS/Linux: publishes self-contained, bundles engines + licenses, signs (optional), tars to dist/. |
| build/sign-windows.ps1 | Optional Authenticode signing (Azure Trusted Signing or Key Vault). Off unless configured. |
| build/sign-macos.sh | Optional Developer ID signing + notarization. Off unless configured. |
| third_party/README.md | The exact per-RID layout the engines must be staged into. |
| licenses/ | The notices bundled into every build (VOACAP disclaimer, NEC2++ GPLv2 + source offer). |

"RID" is a .NET Runtime Identifier - the platform tag, such as  win-x64 ,  osx-arm64 ,  linux-x64 ,  linux-arm64 .


## Step 1 - Build the Engines (VOACAP + NEC2++)

The engines are compiled from their public sources into the staging area. Run this on the target operating system (they're native programs):

- **macOS / Linux / Raspberry Pi:**  build/build-engines.sh   (auto-detects the RID; pass one to override, e.g.  build/build-engines.sh linux-arm64 ). Run  build/build-engines.sh --check  first to verify you have git, make, gcc/g++, gfortran, and autotools.
- **Windows:**  build/build-engines.ps1   (needs MSYS2 with the gcc/gfortran toolchain; the script prints the exact  pacman  packages if they're missing).

This stages  third_party/<rid>/voacap/{voacapl, itshfbc/}  and  third_party/<rid>/nec/nec2++ , and copies NEC2++'s GPLv2  COPYING  and a source snapshot into  licenses/nec2++/ .

> **WINDOWS RUNTIME DLLs** — Engines built under MSYS2/Cygwin need their runtime DLLs (gcc/gfortran runtime, or cygwin1.dll) beside the .exe to run on a machine without MSYS2. Copy those DLLs into third_party/win-x64/voacap and .../nec after building, or build a static/native variant.


## Step 2 - Package the App for a Platform

Packaging publishes the app self-contained (no separate .NET needed by the user), bundles whatever engines are staged, copies the license notices, and produces an archive in  dist/ :

- **Windows:**  build/package.ps1 -Version 1.0.0   (add  -Rid win-arm64  for ARM). Produces  dist/ActivationPlanner-1.0.0-win-x64.zip .
- **macOS / Linux:**  build/package.sh <rid> 1.0.0  , e.g.  build/package.sh osx-arm64 1.0.0  or  build/package.sh linux-x64 1.0.0 . Produces a  .tar.gz  in  dist/ .

If the engines aren't staged, packaging still succeeds and warns - the resulting build runs in sample mode. That's the same archive an end user downloads and installs with  install.ps1 / install.sh .


## Step 3 - Code Signing (Optional, Per Operating System)

Signing is **off by default** and credential-gated, so the same pipeline makes signed or unsigned builds. Nothing in the scripts stores your certificates or passwords - they read values you set in environment variables, and you run the signed build.

- **Windows** (from  package.ps1 , which calls  sign-windows.ps1 ): set  ACTIVATIONPLANNER_SIGN=trustedsigning  (Azure Trusted Signing) or  =keyvault  (Azure Key Vault cert), plus the mode's variables. A signed Windows build avoids the SmartScreen "unknown publisher" warning.
- **macOS** (from  package.sh , which calls  sign-macos.sh ): set  ACTIVATIONPLANNER_SIGN=developerid  and  APPLE_SIGN_IDENTITY , plus notarization credentials (a stored  notarytool  profile, or APPLE_ID / APPLE_TEAM_ID / APPLE_APP_PASSWORD). A signed + notarized macOS build avoids the Gatekeeper "unidentified developer" block.

> **SIGNING STATUS** — Code signing is intended but may not be operational yet. Until it is, releases ship unsigned and users follow the get-past-the-warning steps in the Windows/macOS chapters. The scripts already support signing so builds become signed the moment credentials are configured.


## ARM and Raspberry Pi Builds

The whole matrix is supported: Windows / macOS / Linux, each on x64 or ARM. The app cross-publishes to any RID, but the **engines must be compiled on the target architecture** - so for a Raspberry Pi, build on the Pi:

1. On a Raspberry Pi (64-bit Pi OS, Pi 4 or Pi 5 recommended):  build/build-engines.sh linux-arm64  .
2. Then  build/package.sh linux-arm64 1.0.0  to produce the Pi distributable.
3. Install it with  install.sh  as in the Linux chapter.

Apple Silicon uses  osx-arm64  (built on an Apple Silicon Mac); ARM Windows uses  win-arm64 .


## GPLv2 Source Obligation for NEC2++

NEC2++ is GPLv2. The build stages its  COPYING  and a source snapshot into  licenses/nec2++/ , and the packaging step bundles the whole  licenses/  folder into every build - so each distributable carries the GPLv2 text and the corresponding source (or the written offer in  NEC2++-Source-Offer.txt ). Keep this intact; it's how the redistribution stays compliant. Full detail is in  docs/THIRD_PARTY_LICENSES.md .


## The One Rule: Sign Each OS on Its Own Machine

> **WHERE EACH BUILD HAPPENS** — You can cross-BUILD the app for any platform from one machine, but you must SIGN each platform's build on that platform: Windows signing (Azure) runs on Windows; macOS signing + notarization (codesign/notarytool) runs only on a Mac. And because the engines are native, compile them on the target OS/architecture too. In practice: build + sign the Windows release on Windows, and the macOS release on a Mac.
