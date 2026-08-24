# Activation Planner — User Manual

*Every feature, explained and step by step — in plain language.*

*Generated August 24, 2026 · Markdown is the living source of truth.*


---


# 1. Introduction — What Activation Planner Is

*What the program does, who it's for, and how the pieces fit together — so the rest of this manual makes sense.*

> **QUICK VERSION** — Activation Planner helps you plan a radio operating session. You tell it your gear and where you're going; it tells you which **bands** will actually work (using real propagation predictions, not a guess), which of **your** antennas suits each band, and builds you a **packing checklist**. Plan at home the night before — then take it to the field on a laptop to re-check propagation, watch **POTA spots**, spot yourself, and check the weather. Everything lives on the buttons across the top of the window. Start on **Inventory** to enter your gear once, then use **Plan session** (or **Quick plan**) every time you operate.


## What This Is / What It Is For

**Activation Planner** is a desktop program that helps a single amateur-radio operator plan an operating session **before** it happens. Think of it as the step you do at the kitchen table (or the trailhead) the night before, or an hour before: deciding what to bring, which bands to try, and when — so that when you actually key up, you're set up for success instead of guessing.

It is built around one honest promise: the recommendations are **grounded in real propagation science, not rules of thumb.** Under the hood it runs the same professional prediction engine that serious HF planning relies on, feeds it the current space-weather conditions, and turns the results into plain-English advice about which bands are worth your time.

There is no account to create and no subscription; your gear list stays on your own computer.

> **REAL DATA, NOT GUESSWORK** — Where most planning is "20 meters is usually good in the afternoon," Activation Planner asks a propagation model what the bands should actually do for **your** location, **today**, under **today's** solar conditions — and ranks them for you. It's the difference between a weather forecast and a hunch.


## Plan at Home — and Take It to the Field

Activation Planner earns its keep twice. At the kitchen table the night before, it's a planning tool. On site, running on a laptop, it's a companion you keep open while you operate — because several of its screens are **live**:

- **Re-check propagation as the day changes.** Bands open and close through the day; re-run a plan or watch the **Trend** screen to catch it.
- **See who's on the air.** The **POTA spots** screen shows current Parks on the Air activity, and (when enabled) lets you post your own **self-spot** so hunters can find and call you.
- **Check the weather** for your exact location — and get can't-miss watch/warning alerts if severe weather is headed your way.
- **Catch the grey line** — the **Grey line** screen tells you when the low bands are about to come alive at your location.

> **THE LIVE FEATURES NEED INTERNET — A PHONE HOTSPOT IS FINE** — The live screens — current solar data, POTA spots and self-spotting, and weather — need an internet connection. In the field that's usually your **phone's mobile hotspot**. The core band planning (the VOACAP propagation predictions) runs right on your computer and works even with **no signal at all**, so you're never stranded without a plan.


## What You Can Run It On

Activation Planner is a **cross-platform computer application** — the same program on every system. It runs on:

- **Windows** laptops and desktops (Windows 10 or 11).
- **macOS** — MacBooks and desktop Macs.
- **Linux** laptops and desktops.
- A **Windows tablet** such as a Microsoft Surface works too, because it runs full Windows.

For the field, any lightweight laptop is ideal — a small Windows laptop, a MacBook Air, or a Linux netbook all do the job. Bring whatever you already carry; there's nothing special to buy.

> **WHAT IT WILL NOT RUN ON** — Activation Planner is **not** a phone or tablet app. It will **not** run on an Apple **iPhone or iPad**, or on an **Android** phone or tablet — there is no mobile version (yet). You need a real computer running **Windows, macOS, or Linux**. If you connect to the internet through your phone in the field, that's perfect — just run the planner on a laptop and let the phone provide the hotspot.


## Who This Manual — and This Program — Is For

This program is for the individual operator planning their own outing. It's aimed squarely at portable and field operating, and it understands the common activity types you'll choose from:

- **POTA (Parks on the Air)** — portable operating from a park, facing a stream of hunters calling you.
- **SOTA (Summits on the Air)** — weight-critical operating from a summit, where every ounce matters.
- **Field Day** — a longer, higher-power multi-band event with more support gear.
- **EMCOMM (Emergency Communications)** — regional nets and a load-bearing go-kit, where reliability beats distance.
- **General operating** — casual sessions with no special emphasis.

You do not need to be technical to use it, and you do not need to understand propagation math — the program does that part. If you can tell it what radios and antennas you own and where you'll be, it does the rest.

> **WHAT IT DELIBERATELY DOES NOT DO** — Activation Planner is a **planning** tool only. It does not log your contacts (that's a job for a logging program), and it does not manage teams, incidents, or resources. Keeping it focused on planning is what keeps it simple.


## The Big Idea: Plan From Real Data

Three ideas run through the whole program. Understanding them makes every screen easier to follow.

**1. Real propagation, per band.** The program predicts how well each HF band should carry your signal to where you're trying to reach, hour by hour, using the current sunspot/solar numbers pulled live from the internet. You get a ranked list of bands with a reliability score and a 24-hour heat strip, not a generic chart.

**2. Your gear comes first.** Recommendations are built from the equipment you actually own. When the program suggests an antenna for a band, it's suggesting **your** antenna — and it clearly separates "you own this" from "you might consider acquiring this," so the two never blur together.

**3. A plan you can carry.** The end product isn't just a screen of numbers — it's a tailored **packing checklist** for the operation you chose, which you can check off, print, or export to a PDF (Portable Document Format) file to take with you.


## The Planning Workflow, Start to Finish

Here is the whole program in five steps. Each step is a chapter later in this manual, so this is just the map.

1. **Set up your gear once.** On first launch a guided setup wizard walks you through entering your radios and antennas. This is the only "setup" step, and you only do it once (you can edit it any time afterward on the **Inventory** screen).
2. **Start a plan.** Use **Plan session** to enter where you are, where you're trying to reach, and let it pull current solar conditions — or use **Quick plan** to jump straight to recommendations using your location.
3. **Read the recommendations.** The program ranks the bands, shows a 24-hour reliability strip and the common calling frequencies for each, and lists which of your antennas fits each band.
4. **Build your checklist.** On **Mission & checklist**, pick the operation type (POTA, SOTA, and so on) and the program tailors a packing list from your inventory, with suggestions highlighted.
5. **Take it with you.** Print the checklist, or export the whole plan to a PDF.

> **YOU CAN RE-PLAN ANY TIME** — Conditions change through the day, so planning isn't a one-time thing. Re-run a plan whenever you like — it always uses the current time and the latest solar data. The program doesn't keep a history; each plan is a fresh look at right now.


## Getting Around: The Navigation Bar

Everything is reached from the row of buttons across the **top** of the window. The button for the screen you're on is highlighted in blue. Here's what each one does — every screen has its own chapter later.

| Button | What it's for |
| --- | --- |
| Quick plan | Jump straight to band/antenna recommendations for your current location, with the plan generated for you. |
| Plan session | The full planning screen: enter your location, target, and conditions, then generate a detailed plan. |
| Trend | How each band's reliability has moved over the last few hours this session. |
| Grey line | The twilight band where the low bands open up — a live world map plus when your grey-line window is. |
| Weather | The forecast for your operating location, and can't-miss weather watch/warning alerts. |
| Band plan | A plain-language reference of US amateur privileges — where each license class may operate, and in what mode. |
| Mission & checklist | Pick your operation type and get a tailored, printable packing list from your gear. |
| POTA spots | Live Parks on the Air activator spots, and (when enabled) posting your own self-spot. |
| Antenna | The radiation-pattern plots for your antennas. |
| Inventory | Add, edit, or remove your radios and antennas at any time. |
| Battery | Estimate how long a battery will run your radio in the field. |

> **THE CLOCK AND THE THEME BUTTON** — At the top-right you'll always see a live clock showing your **local time above UTC** (Coordinated Universal Time — the worldwide reference hams use). Next to it, a small theme button cycles the app's look between **Auto** (match your system), **Light**, and **Dark**.


## A Few Words You'll Meet

This manual defines terms as they come up, but here are the ones you'll see most often, in plain language:

- **Band** — a range of frequencies, named by wavelength (e.g. "20 meters"). Different bands behave very differently and at different times of day.
- **Propagation** — how (and whether) your radio signal travels from you to where you want it to go. It changes with the time of day, the season, and the sun.
- **VOACAP (Voice of America Coverage Analysis Program)** — the professional propagation-prediction engine the program uses to rank bands.
- **Grey line** — the moving line of twilight between day and night, along which low-band signals can travel unusually far.
- **NVIS (Near Vertical Incidence Skywave)** — sending signals nearly straight up so they rain back down for short-to-medium regional coverage; common in EMCOMM.
- **Reliability** — the program's headline score for a band: roughly, the chance a contact will succeed at that time. Higher is better.
- **Self-spot** — posting a note to a spotting website that you're on the air at a certain frequency, so others can find and call you.


## How to Read This Manual

Every chapter covers one screen or feature and is built the same way, so you can skim or read deeply as you like:

- Each chapter opens with a **Quick Version** box — read just that and you'll get the gist.
- Steps are numbered; **buttons and fields are shown in bold** exactly as they appear on screen.
- Colored callout boxes flag **Tips**, **Notes**, **Important** points, and **Warnings**.
- Feature chapters end with a **Troubleshooting** section — symptom, then fix.

> **A PLANNING AID, NOT THE LAST WORD** — Activation Planner helps you plan, but you are always the operator in charge. Follow the current FCC rules and your license privileges, use good judgment about safety and the weather, and treat the program's predictions as well-informed guidance, not a guarantee.


# 2. Getting Started — First Launch and the Setup Wizard

*Opening the program for the first time and walking through the one-time guided setup that teaches it what gear you own.*

> **QUICK VERSION** — The first time you open Activation Planner, a **setup wizard** appears and walks you through entering your gear — radios, antennas, power, and so on — one screen at a time. Use **Next** and **Back** to move through it, add each item to the running list, and press **Finish** on the last screen. That's the only setup there is, and you only do it once. In a hurry? Press **Skip** to jump straight to recommendations and add gear later.


## Installing and Starting the Program

This chapter assumes Activation Planner is already installed. If it isn't, follow the separate **Installation Guide**, which spells out the download-and-install steps for each operating system. Installation is a one-time thing and is the same idea on every system: get the program onto your computer, then open it.

Starting Activation Planner is exactly like starting any other program on your computer:

- **Windows:** open it from the **Start menu** (or double-click its icon on the desktop).
- **macOS:** open it from **Launchpad** or the **Applications** folder (or double-click its icon).
- **Linux:** launch it from your applications menu, or run it the way your distribution starts installed apps.

The program looks and works the same on all three systems, so the rest of this manual applies no matter which one you use.


## Two Helper Programs — VOACAP and NEC2++

When you install Activation Planner, you'll notice it also sets up two small **helper programs** alongside it: **VOACAP** and **NEC2++**. This is normal and intended. Here's what they are, in plain terms:

- **VOACAP** is the **propagation-prediction engine** — the proven tool (originally created for the Voice of America) that works out which bands will actually carry your signal, and when. It's the brains behind the band recommendations.
- **NEC2++** is the **antenna-modeling engine** — it calculates your antenna's radiation pattern from the measurements you enter (Chapter 3).

Activation Planner **runs these as separate programs** rather than building their code inside itself. That's a deliberate design choice, and it's worth understanding because you'll see them installed:

- **Accuracy and trust.** VOACAP and NEC2++ are respected, independently-maintained engineering tools. Running them exactly as their authors made them — unmodified — means you get the same trustworthy results a professional would, not a home-grown imitation.
- **Honest, clean licensing.** These tools come under their own separate software licenses. The correct, above-board way to include someone else's licensed program is to **run it as its own program** rather than copying its code into ours. Keeping them separate honors those licenses and keeps everyone's rights clear — NEC2++ in particular is shared under terms that are respected precisely by keeping it a standalone program the planner simply calls.
- **Independent updates.** Each helper can be updated on its own, without disturbing the planner.

> **YOU DON'T USE THEM DIRECTLY** — You never open VOACAP or NEC2++ yourself — Activation Planner drives them for you behind the scenes and turns their output into plain-English advice. Just know that seeing them during installation is expected and correct. The **Installation Guide** covers setting them up, and the **Licenses & Credits** chapter lists their full license notices and credits.


## Why You See a Setup Wizard First

The first time you ever run Activation Planner, it has an empty gear list — it doesn't yet know what radios or antennas you own. Since **every recommendation it makes is built from your gear**, it needs that information before it can be useful. So instead of dropping you onto a blank planning screen, it opens a friendly, step-by-step **setup wizard** to collect it.

This happens **only once.** After you finish (or skip) the wizard, the program remembers your gear and goes straight to the planning screen every time you open it from then on. You never have to repeat setup, and you can change your gear whenever you like.

> **NOTHING HERE IS PERMANENT** — Don't worry about getting every detail perfect right now. Everything you enter in the wizard can be edited, added to, or removed later on the **Inventory** screen (Chapter 3). The goal of the wizard is just to get your main gear in so you can start planning.


## How the Wizard Works

The wizard shows one category of gear per screen. A few controls are always available:

| Control | What it does |
| --- | --- |
| Next | Move forward to the next step. Your entries so far are kept. |
| Back | Return to the previous step to review or change what you entered. |
| Progress indicator | Shows how far along you are and how many steps remain. |
| Skip | Leave setup entirely and jump straight to Quick Mode (see the end of this chapter). |
| Finish | Appears on the final Summary step; saves everything and opens the planner. |

The steps come in this order: **Radios → Antennas → Power → Digital Interfaces → Computers → EMCOMM Gear → Summary.** You can move through them at your own pace, and any step you have nothing for can simply be left empty and passed with **Next**.

> **EVERY CATEGORY IS OPTIONAL** — Only enter what you actually own. If you have no digital interface or no dedicated EMCOMM gear, leave that step empty and press **Next**. The planner works fine with just a radio and an antenna — everything else is a bonus that makes the packing lists smarter.


## Step 1 — Radios

This step collects your transceivers. It offers a **"Start from a model"** picker listing common radios; choosing yours fills in the name for you. If your radio isn't listed, just type its name.

1. If your radio is in the **Start from a model** list, pick it — the name fills in automatically.
2. Otherwise, type the radio's name (for example, **IC-705** or **FT-891**) in the name box.
3. Optionally add a note (a band range, power level, anything you want to remember).
4. Press **Add** to put it on the running list. Repeat for each radio you own.
5. When your radios are all listed, press **Next**.

> **PICKING A MODEL FILLS IN DETAILS** — Choosing your radio from the model list is worth it — it carries over useful details (like the radio's power) that help the planner tailor suggestions later. You can still edit the name afterward.


## Step 2 — Antennas

Antennas get their own step because they carry the most detail — the program uses their physical dimensions to model how they radiate. As with radios, there's a **"Start from a model"** picker (many popular portable antennas are listed); picking one **prefills the measurements for you**, which you can then adjust.

You add antennas one at a time to a running list. For each antenna you'll set its **type**, **feed point**, and **dimensions**.

> **ANTENNA DETAILS MATTER — SEE CHAPTER 3** — Getting the antenna numbers right is what lets the program model your pattern correctly. Because it's so important, the full, field-by-field explanation of what to measure and enter for each antenna type lives in **Chapter 3 (Your Gear Inventory)**. If you pick a model from the list, the numbers are filled in for you and you can move on; if you're entering a custom antenna, read Chapter 3 first.

1. Pick your antenna from **Start from a model** if it's listed — its measurements prefill.
2. Otherwise choose **Custom / Home-brew** and enter the type, feed point, and dimensions (Chapter 3 explains each).
3. Press **Add** to add it to your antenna list. Repeat for each antenna.
4. Press **Next** when done.


## Steps 3–6 — Power, Digital Interfaces, Computers, EMCOMM Gear

The remaining steps all work the same simple way as the Radios step — a name box, an optional note, and an **Add** button, with a model picker where one is available. Enter what you own, or leave a step empty if it doesn't apply:

- **Power** — batteries, power stations, solar panels, and the like (e.g. a **Bioenno 12 Ah**). Including the capacity in the name helps the Battery calculator later.
- **Digital Interfaces** — sound-card/CAT devices for digital modes (e.g. a **Digirig** or **SignaLink**).
- **Computers** — a laptop or tablet you take to the field for logging or digital modes.
- **EMCOMM Gear** — anything specific to emergency communications (go-kit items, forms, and so on).

These categories aren't just a list — the program uses them to tailor your packing checklists. A computer or digital interface, for instance, will be suggested for a Field Day or EMCOMM operation but left off a lightweight summit hike.


## The Summary Step and Finishing

The last step shows a **Summary** of everything you've entered so you can look it over before committing.

1. Review the summary. If something's wrong, press **Back** to the right step and fix it.
2. When it looks right, press **Finish**.
3. Your gear is saved, and the program opens to the planning screen — you're ready to plan.

> **WHERE YOUR GEAR IS SAVED** — Your inventory is saved on your own computer and reloaded automatically every time you open the program. It is not uploaded anywhere. To back it up or move it to another computer, see the Installation Guide.


## Skipping Setup — Straight to Quick Mode

If you'd rather not do setup right now, press **Skip**. This takes you directly to **Quick Mode**, which generates band recommendations for your location immediately (Chapter 5). You can add your gear later on the **Inventory** screen, and the recommendations get more useful once you do — the program can only match antennas you've told it about.


## Changing Your Gear Later

You are never locked into what you entered in the wizard. At any time, click **Inventory** in the navigation bar to add new gear, edit an existing item, or remove something you sold. Chapter 3 covers the Inventory screen in full.


## Troubleshooting

| Symptom | What to do |
| --- | --- |
| The wizard didn't appear — I went straight to the planner | That's normal on later launches; the wizard only appears the first time. Use the **Inventory** screen to enter or change gear. |
| I pressed Next past a step I meant to fill in | Press **Back** to return to it — nothing is lost. You can also add that gear later in **Inventory**. |
| My radio or antenna isn't in the model list | That's fine — just type the name (radios) or choose **Custom / Home-brew** (antennas) and enter it yourself. The list is a convenience, not a limit. |
| I finished the wizard but entered a radio wrong | Open **Inventory**, find the item, and press **Edit** (or **Remove** and re-add it). Changes save automatically. |
| I want to start over completely | Remove the items you don't want in **Inventory**; there's no need to re-run the wizard. (To wipe everything, see the Installation Guide's note on the saved inventory file.) |


# 3. Your Gear Inventory — Radios and Antennas

*Adding, editing, and removing your equipment — and, most importantly, exactly what to enter for each antenna so the program can model it correctly.*

> **QUICK VERSION** — Click **Inventory** in the top bar. It has two tabs: **Gear** (radios, power, and everything else) and **Antennas**. On each, fill in the form on the left and press **Add** — items appear in the list on the right, and everything saves automatically. For antennas, the easiest path is to pick your model from the **Start from a model** list, which fills in the measurements for you. Entering a custom antenna? The one rule to remember: **if you don't know a measurement, leave it 0** — the program fills in a sensible estimate and says so.


## What This Is / What It Is For

The **Inventory** screen is where the program keeps the list of equipment you own. It matters more than it might sound, because everything Activation Planner recommends is built from this list: which antenna to use on a band, what to pack for an operation, how long your battery will last. The better your inventory, the better the advice.

You reach it any time by clicking **Inventory** in the navigation bar. Unlike the first-run wizard (Chapter 2), this screen is always available, and any change you make is **saved the moment you make it** — there's no separate "save" button to remember.


## The Two Tabs: Gear and Antennas

The Inventory screen is split into two tabs near the top:

- **Gear** — everything that isn't an antenna: radios, batteries and power, digital interfaces, computers, and miscellaneous items.
- **Antennas** — kept separate because antennas carry extra measurements the program needs to model how they perform.

Both tabs work the same way: a **form on the left** to add or edit an item, and a **list on the right** of what you've already entered, each row with **Edit** and **Remove** buttons.


## Adding Ordinary Gear

On the **Gear** tab, adding an item takes just a moment:

1. Choose the **Category** that fits the item (see the table below).
2. If a **Start from a model** list appears for that category (radios have one), pick your model to fill in the name.
3. Type or adjust the **Name** (for example, **IC-705**).
4. Optionally add a **Note** — a band range, power level, capacity, anything useful.
5. Press the **Add** (or **Save**) button. The item joins the list on the right and is saved.

Here's what each gear category means, and why the program cares which one you pick:

| Category | What goes here | Why it matters |
| --- | --- | --- |
| Radio | Your transceivers. | Sets the transmit power used in planning and the battery calculator. |
| Power | Batteries, power stations, solar panels. | Feeds the Battery runtime calculator; suggested for every operation. |
| Digital Interface | Sound-card / CAT devices for digital modes (Digirig, SignaLink). | Suggested for Field Day and EMCOMM packing lists. |
| Computer | A laptop or tablet you take to the field. | Suggested for Field Day and EMCOMM. |
| EMCOMM | Gear specific to emergency communications. | Suggested for EMCOMM operations. |
| Other | Anything else you want tracked. | Kept on your list; packed as a general item. |

To change an item later, press **Edit** on its row, adjust the form, and save. To delete one, press **Remove**. Both take effect immediately.


## Adding an Antenna — and Why the Details Matter

Antennas are the part of your inventory that most affects the program's advice, because Activation Planner **models** each antenna to work out how it radiates — where its signal goes, and therefore which bands and which kind of operating it suits. Modeling is done by a proven engineering tool (called NEC2++) that the program runs for you behind the scenes. You never see that tool directly; you just give it good measurements and it does the math.

And that's the key idea: **the model is only as good as the numbers you type in.** Good measurements give a trustworthy pattern; wrong measurements give a wrong one. This section explains exactly what to enter so the model is right. It's more detail than the other gear — on purpose.

> **THE GOLDEN RULE — DON'T KNOW A NUMBER? LEAVE IT 0** — If you don't know a measurement, **leave that field at 0.** The program does not silently guess — it fills in a sensible standard estimate for that band and clearly labels the result as an estimate on the pattern plot. Entering a wrong number is worse than leaving it 0, so when in doubt, use 0.

> **ALL LENGTHS AND HEIGHTS ARE IN FEET** — Every measurement on the antenna form is in **feet**. If you know a length in meters, multiply by about 3.28 to get feet (for example, 20 m ≈ 66 ft).


## The Antenna Fields, in Plain Words

The antenna form has a handful of fields. Here's what each one means before we go type by type. Don't worry about the jargon — each term is explained plainly.


### Name

Whatever you'll recognize it by — "40m EFHW", "Chameleon MPAS", "my dipole". It's just a label.


### Category (the antenna type)

The physical family the antenna belongs to. This is important because it changes what the other fields mean and how the program models the antenna. The choices, in plain terms:

| Category | In plain words |
| --- | --- |
| Vertical | A rod or wire that stands up vertically, usually needing wires (radials) on the ground under it. |
| Whip | A short telescoping or loaded vertical rod, like a mobile whip — treated like a small vertical. |
| End-Fed Half-Wave | A long single wire fed (connected) at one end — very popular for portable use. Strung low, it does NVIS; strung high, it favors distance. |
| Dipole | A wire with two equal legs fed in the middle — the classic "T" shape. Strung low it's an NVIS/regional antenna; strung high it favors distance. |
| Magnetic Loop | A small tuned loop of tubing. Specialized; needs a hand-built model (see below). |
| NVIS Crossed Dipole | A purpose-built NVIS antenna: two low dipoles crossed in an X for even regional coverage. Not required for NVIS — a single low dipole also works (see below). |
| Other | Anything that doesn't fit the above — needs a hand-built model. |


### Feed point

"Feed point" simply means **the spot where your coax (feedline) connects to the antenna.** Where that is affects the pattern, so the program asks. In plain terms:

| Feed point | In plain words |
| --- | --- |
| Center-fed | Connected in the middle — normal for a dipole. |
| End-fed half-wave | Connected at one end of a half-wavelength wire. |
| End-fed random wire | Connected at the end of a wire that isn't a specific resonant length (used with a tuner). |
| Off-center-fed | Connected off to one side of center, not the middle. |
| Base-fed | Connected at the bottom — normal for a vertical. |
| Other | Anything else. |

If you pick your antenna from the model list, the feed point is already set correctly — you don't have to think about it.


### Length

The size of the radiating part of the antenna. **What exactly "length" means depends on the antenna type**, so the form's label changes to tell you (for example, it may say "Length — tip to tip" for a dipole, or "Element length" for a vertical). The next section spells out each case.


### Height

How high the **feed point** is above the ground, in feet. Height changes an antenna's pattern a lot — the same dipole strung low behaves very differently from one strung high — so this number matters. If the antenna sits right on the ground, enter **0**.


### Radials

"Radials" are the wires you spread out under a vertical antenna — they act as the other half of the antenna (its electrical "ground"). These fields appear **only for Vertical and Whip types.** There are **three** radial boxes:

| Box | What to enter |
| --- | --- |
| Radial count | How many radial wires you lay out. None (or a self-contained antenna)? Enter 0. |
| Radial length (ft) | How long each radial wire is. Don't know? Leave 0 and the program estimates a resonant length. |
| Radial height (ft) | How high the radials sit above the ground. **On the ground? Leave it 0.** Raised up on stakes a few feet? Enter that height (see "On-ground vs elevated radials" below). |

> **ON-GROUND VS ELEVATED RADIALS** — Radials laid on the dirt and radials raised a few feet on stakes behave very differently, so the program asks their height. **On-ground radials** (height 0) are the classic setup and usually want many wires (a dozen or more) to work well. **Elevated radials** — even just 2 to 4 wires raised 3 or so feet — work as well as a large on-ground field, lower the take-off angle (better for distance), and cut ground loss. If you elevate your radials, enter their height so the program models that benefit; leave it 0 for on-ground radials.


## Exactly What to Enter, by Antenna Type

This is the heart of the chapter. Find your antenna type and enter its measurements as described. A quick word first on two terms you'll see: a **half-wave** and a **quarter-wave** are just antenna lengths that are naturally "in tune" on a band — you don't need the math; the program handles resonance when you leave a length at 0.


### Dipole

- **Length — tip to tip:** the whole dipole end to end, **both legs together** (a 40 m dipole is about 66 ft total, not 33).
- **Height:** how high the **center** (where the coax connects) is above ground.
- **Leave Length at 0** and the program models a properly-tuned dipole for whatever band it's checking.


### End-Fed Half-Wave (EFHW)

- **Wire length:** the total length of the wire (a 40 m EFHW is about 66 ft).
- **Height:** how high the **fed end** is above ground. If you hang it as a sloper (one end high, one end low), use the height of the **high** end.
- **Leave Length at 0** to have it modeled as a tuned half-wave for the band.


### Vertical / Whip

- **Element length:** just the **vertical part** — not the radials. If it's a loaded or modular antenna whose true electrical length you don't know (a Chameleon MPAS, a screwdriver antenna, a Wolf River coil), **leave it 0** and the program estimates a tuned quarter-wave.
- **Height:** how high the **base (feed point)** is above ground. Sitting on the ground? Enter **0**. Mounted a few feet up on a tripod? Enter that height.
- **Radial count** and **Radial length:** how many radial wires you lay out, and how long each is. None? Leave both 0.
- **Radial height:** **0** if your radials lie on the ground. If you raise them on stakes (a common trick — even 2 to 4 wires a few feet up), enter that height. The program then models the lower take-off angle and reduced ground loss that elevated radials give you.

> **WORKED EXAMPLE — VERTICAL WITH ELEVATED RADIALS** — A Chelegance MC-750 (or similar) with four radials raised about 3 ft on stakes: Category = **Vertical**, Element length = **0** (loaded/telescoping — let the program estimate), Radial count = **4**, Radial length = whatever you deploy (or 0 to estimate), **Radial height = 3**. The pattern should show a lower take-off angle than the same antenna with on-ground radials — better for reaching distance.


### NVIS — a Note Before the Antenna Types

**NVIS** stands for Near Vertical Incidence Skywave — sending your signal nearly **straight up** so it reflects back down over a wide region a few hundred miles across. It's the go-to for regional and emergency nets on the low bands (40, 60, and 80 meters). Here's the key point many people miss: **NVIS is a technique, not a special antenna.** You don't need a dedicated "NVIS antenna" to do it.

> **THE MOST COMMON NVIS ANTENNA IS JUST A LOW DIPOLE** — By far the most common NVIS antenna is an ordinary **dipole (or end-fed wire) strung low to the ground** — roughly one-tenth to one-quarter of a wavelength up (about **10 to 20 feet** on 40 and 80 meters). Hung that low, a plain wire naturally fires its energy almost straight up, which is exactly what NVIS needs. You do **not** enter it as a special type — enter it as a **Dipole** (or **End-Fed Half-Wave**) with a **low Height**, and the program models the high, NVIS-style take-off angle for you.


### A Single Low Dipole (or Low End-Fed) for NVIS

This is the simplest and most popular way to do NVIS, so it's worth spelling out. It's just your normal wire antenna hung low:

- **Category:** choose **Dipole** (for a center-fed wire) or **End-Fed Half-Wave** (for an end-fed wire) — whichever you actually have. There's no separate box to tick for NVIS.
- **Length:** enter it exactly as you would for any dipole or end-fed (tip to tip for a dipole; total wire length for an end-fed), or leave **0** to have the program model a tuned length.
- **Height:** this is the important one — enter your **low** height, typically **10 to 20 ft** for 40/80 m NVIS. The lower you go (down to about a tenth of a wavelength), the more the energy goes straight up.

> **WORKED EXAMPLE — 40 m NVIS DIPOLE** — A 40-meter dipole strung about 15 ft off the ground for regional coverage: Category = **Dipole**, Length — tip to tip = about **66** (or 0 to estimate), Height = **15**. View its pattern (Chapter 7) and you'll see a big lobe pointing nearly straight up — a **high take-off angle** — which is exactly what fills in a region a few hundred miles around you. The same wire hung at 40 ft instead would lower that angle and favor distance rather than regional coverage. Height is the whole story.

In short: **the app already calculates NVIS for a single low wire** — it's the low **Height** on a normal Dipole or End-Fed that does it. The crossed-dipole type below is a purpose-built NVIS antenna some operators own, but it is not required to work NVIS.


### NVIS Crossed Dipole (e.g. Chameleon 4-wire NVIS)

This is a purpose-built NVIS antenna: two dipoles crossed in an X, fed at the center on top of a short mast, with the four legs sloping down to stakes in the ground. Like a low dipole, it's designed to send signals nearly **straight up** — the crossed pair just gives more even coverage in all compass directions. Use this type **only if you actually own a crossed-dipole antenna**; for a plain low wire, use Dipole or End-Fed Half-Wave as described just above.

- **Leg length:** the length of **ONE of the four wires**, measured from the center out to its far (staked) end — **not** all four added together. The Chameleon 4-wire NVIS uses legs of about **45 ft**.
- **Height:** the height of the **center feed at the top of the mast** (the peak of the X). The legs slope down from there. A typical NVIS mast is about **15 ft**.
- **Leave Leg length at 0** to model a tuned leg automatically.

> **WORKED EXAMPLE — CHAMELEON 4-WIRE NVIS** — Category = **NVIS Crossed Dipole**, Leg length = **45**, Height = **15**. With those numbers the pattern should peak nearly straight up (a high "take-off angle") — exactly what you want for regional NVIS coverage on 40, 60, and 80 meters.


### Magnetic Loop / Other

These don't have a simple automatic model yet. Enter them if you like, but the program will tell you a hand-built model is required rather than guess at a pattern. You can still keep them in your inventory and pack them; they just won't produce an automatic radiation-pattern plot.


## Why the Numbers Matter — and "Measured" vs "Approximate"

When you view an antenna's pattern (Chapter 7), the program shows how confident it is in the model, based on what you gave it:

- **Measured** — you (or the chosen model preset) supplied real dimensions for a simple wire antenna, so the pattern is an accurate model of your actual antenna.
- **Approximate** — the antenna is a loaded or broadband design whose exact electrical length isn't published, so the shape is representative but not exact. The program says so plainly rather than pretending.

Likewise, if you leave a length at 0, the antenna's pattern shows a small blue note such as *"Length not set — modeled as a resonant quarter-wave."* The **shape** of the pattern is still useful; only the exact size was assumed. Type in the real length and that note disappears.

> **WHY THE SHAPE OF THE PATTERN MATTERS** — The whole point of modeling is the **take-off angle** — the direction the antenna sends most of its energy. **Energy aimed high** (straight up) is for short-range, regional, and NVIS work. **Energy aimed low** (toward the horizon) is for long-distance (DX). The program combines your antenna's pattern with the propagation forecast to recommend bands, so getting the antenna right makes the whole plan better.


## The Easy Path: Start From a Model

Above the antenna form is a **Start from a model** list of many popular portable antennas. Picking yours **fills in the type, feed point, and measurements for you** — no measuring required. You can still adjust any field afterward.

If your exact model isn't listed but a similar one is, you can pick the similar one as a starting point and edit the numbers. If it's truly one-of-a-kind, choose **Custom / Home-brew** and enter the fields yourself using this chapter. When a preset is a loaded or broadband design, the app shows a short note that its pattern will be **Approximate** — that's the honest label described above, not a problem.


## Editing and Removing Antennas Later

Just like ordinary gear, each antenna row has **Edit** and **Remove** buttons. Press **Edit** to load it back into the form, change anything, and save; press **Remove** to delete it. Changes are saved immediately and are reflected the next time you generate a plan or view a pattern.


## Troubleshooting

| Symptom | What to do |
| --- | --- |
| The Radials boxes don't show up | They appear only for **Vertical** and **Whip** types. If your antenna isn't one of those, it doesn't use radials in the model. |
| How do I model elevated radials? | Enter their height in the **Radial height (ft)** box (verticals/whips only). Leave it 0 for on-ground radials. Even 2–4 radials a few feet up perform like a big on-ground field and lower the take-off angle. |
| How do I set up an NVIS antenna? | For the common case — a single low wire — just enter a **Dipole** or **End-Fed Half-Wave** with a **low Height** (about 10–20 ft on 40/80 m). The program models the high, straight-up take-off angle automatically. Use the **NVIS Crossed Dipole** type only if you own an actual crossed-dipole antenna. |
| The antenna's pattern says "estimate" / "approximate" | That's expected if you left a length at 0 or picked a loaded/broadband model. Enter the real length to remove the estimate note; the "Approximate" label is normal for loaded antennas. |
| I don't know my antenna's length | Leave **Length** at 0 — the program models a tuned length for the band and labels it an estimate. Don't guess a wrong number. |
| I entered a dipole as 33 ft and it seems off | Length is **tip to tip (both legs)** — a 40 m dipole is about **66 ft**, not 33. Re-enter the full end-to-end length. |
| My antenna type isn't listed | Use the closest match, or **Other**. Magnetic Loop and Other won't auto-model a pattern but can still live in your inventory. |
| I edited an antenna but the pattern didn't change | Re-open the **Antenna** tab (Chapter 7) and reselect the antenna/band so it re-models with the new numbers. |


# 4. Planning a Session — The Main Screen

*The heart of the program: enter where you are and the current conditions, generate a plan, and understand every number it gives back — including what the solar figures really mean.*

> **QUICK VERSION** — Click **Plan session**. Set **your location** (or press **Use my location**), set the **target** you're trying to reach, and leave the **conditions** as they are — the sunspot number is filled in from live solar data for you. Press **Generate plan**. You get a list of bands ranked best-first, each showing how reliable it is, when it peaks, where to call, and which of your antennas fits. In a real hurry, use **Quick plan** instead (Chapter 5), which does all of this automatically.


## What This Is / What It Is For

**Plan session** is the main screen of the program and where the real work happens. You describe your situation — where you are, who you're trying to reach, and the current conditions — and the program predicts, band by band, how well you'll get through, hour by hour across the day. It then matches each band to the antennas you own.

The screen has two parts: a **column of inputs on the left**, and the **results on the right** once you generate a plan. We'll go through the inputs first (top to bottom), then the results.


## The Inputs — Top to Bottom

You don't have to touch every field. The defaults are sensible, the solar numbers fill themselves in, and **Use my location** handles your position. But here's what each one is and does, so nothing is a mystery. Every field also has a short plain-language hint right beneath it in the program.


### Operation framing

"Framing" tells the program **what kind of contact you're planning for**, because that changes the question it asks. There are two choices:

- **DX / point-to-point** — you're trying to reach a specific distant place. This is the normal choice for chasing distance.
- **Regional / NVIS** — you're trying to cover a nearby area a few hundred miles across (common for emergency nets). NVIS stands for Near Vertical Incidence Skywave — sending signals nearly straight up so they rain back down over a region.

The framing is set automatically from the operation type you picked on the Mission screen, but you can change it here. When **Regional / NVIS** is selected, the program reminds you to set a **near-in** target and focuses on the low bands that do NVIS well.


### Your location

Where you'll be operating from. You can fill it three ways, whichever is easiest:

- Type your **Latitude** and **Longitude** in decimal degrees (for example, 39.83 and -98.58). West longitude and south latitude are negative.
- Press **Use my location** to have the program find you automatically.
- Type a **grid square** (like EM29) and press **Set from grid** — handy in the field. The grid box and the latitude/longitude stay in sync both ways.

Location and the grid square are covered in full in their own chapter, including why the grid box is so useful in the field. For planning, just get your position in by any of the three methods above.


### Target location

Where you're trying to **reach**. For DX, that's the far-off place you want to work. For Regional / NVIS, set it to a nearby point that represents your coverage area (a town in the region, say). Enter it as **Latitude** and **Longitude**, the same way as your own location.

> **WHY THE PROGRAM NEEDS A TARGET** — Propagation isn't the same in every direction or over every distance — a band that's wide open to a station 500 miles away may be dead to one 5,000 miles away. Telling the program where you're aiming is what lets it predict honestly for **your** path, not a generic one.


### Conditions — the solar numbers and power

This is the most important group to understand, so it gets its own deep section below. In short: the **Sunspot number** is filled in for you from live solar data (with a "Live: …" line showing the current figures and a **↻ Solar** button to refresh), and **Power** is your transmit power in watts (for example, 5 for a QRP radio, 100 for a typical HF radio). Read *A Deeper Look at the Solar Numbers* below to understand what these figures mean.


### Month and year

The month and year you'll operate. Propagation is **seasonal** — the same band behaves differently in summer and winter — so this matters, and it defaults to right now. Change it only if you're planning ahead for a different date.


### Noise environment

How electrically **noisy** your operating site is. Everything from power lines to chargers to computer gear adds background hiss that buries weak signals. Pick the option that fits where you'll be — roughly, a quiet rural spot, a normal residential area, or a noisy industrial/city location. Not sure? A residential setting is a safe middle. A quieter site lets you hear weaker stations, so the program factors this into how reliable a contact will be.


### Long path

Radio signals can travel the short way between two points, or the **long way** around the globe. Leave this **off** for normal operating. Tick it only if you intend to work the long path (aiming away from the direct bearing) — the program will then predict for that longer route instead.


## A Deeper Look at the Solar Numbers

The sun controls shortwave propagation, so a handful of solar numbers tell you a lot about how the bands will behave. Activation Planner pulls the current figures from a public space-weather feed and shows them on the **Live: …** line under the Conditions heading. Here's what each one means — no background required.


### Sunspot number (SSN)

A **sunspot** is a dark, magnetically active patch on the sun. The **sunspot number** is, roughly, a count of how many are visible — a simple stand-in for "how active is the sun right now." It's the single figure the propagation engine uses, which is why it's the box that gets filled in.

**Why you care:** more solar activity charges up the layer of the atmosphere that bends your signals back to Earth, and it especially helps the **higher bands** (like 10, 12, and 15 meters). A high sunspot number means the high bands are more likely to be open for long distances; a low number means you'll lean on the lower bands. Values range from near 0 (very quiet sun) to well over 200 (very active). If the live figure ever looks wrong or you're planning for a different day, you can type your own number over it.


### Solar flux index (SFI)

The **solar flux index** measures the sun's radio energy at a specific wavelength (10.7 cm). It moves up and down together with the sunspot number and tells basically the same story in a different way: **higher SFI means better high-band conditions.** As a rough feel, an SFI around 70 is quiet, and 150+ is very good for the high bands. The program shows it on the Live line for context; the sunspot number is what actually drives the prediction.


### The A and K indices (how "stormy" the Earth's magnetic field is)

Where the sunspot number and flux describe how *charged up* the bands are, the **A and K indices** describe how *disturbed* the Earth's magnetic field is — in other words, whether there's a geomagnetic "storm" that can wreck propagation even when the sunspot number is high.

- **K-index** — a near-real-time reading on a 0-to-9 scale. **0-2 is calm** (good), **4 is unsettled**, and **5 and up means a geomagnetic storm** that degrades the bands, especially on paths that cross far-northern (polar) regions.
- **A-index** — a longer-term, once-a-day summary on a bigger scale (0 into the hundreds). It's the daily companion to the K-index; **low is good, high means disturbed.**

**How to act on them:** if the K-index is high (say 5+), don't be surprised by poor conditions and lean toward lower bands and shorter paths — a storm can quiet the bands no matter how high the sunspot number is. When A and K are low, the bands are stable and the sunspot number tells the fuller story.

> **WHERE THESE NUMBERS COME FROM** — Activation Planner fetches the current SSN, SFI, and K figures live from a public space-weather source the moment you open the planning screen, and again whenever you press **↻ Solar**. They need an internet connection (a phone hotspot is fine). If the program can't reach the feed, it keeps the last value shown and says so — you can always type a number in by hand.


## Generating the Plan

Once your inputs look right, press **Generate plan**. The program runs the propagation engine for every band across all 24 hours and lays out the results on the right. It takes a moment; a "Generating…" note shows while it works. You can change any input and press **Generate plan** again as often as you like — re-planning is encouraged as conditions change.


## Reading the Results — the Band Cards

Each band comes back as its own card, ordered best-first. Here's every part of a card and what it's telling you.


### The header: band, frequency, and best hour

The top line shows the **band name** (e.g. "20m"), the representative **frequency** the prediction used, and the **best hour** — the single time of day this band is predicted to be at its best, in UTC, with that peak reliability in parentheses (or "no opening" if the band isn't expected to work to your target).


### Reliability — the headline number

**Reliability** is the program's headline score for a band: roughly, **the chance that a contact will succeed** under the conditions you entered, shown as a percentage with a colored bar. Higher is better and greener; lower is redder. It's an average across the day; the heatmap below breaks it down hour by hour. Think of it like a chance-of-rain figure, but for "chance this band gets through."


### The 24-hour heatmap

Beneath the reliability bar is a strip of 24 little colored squares, one per hour of the day (in UTC). Each square is colored by that hour's reliability — green for good, fading to red for poor — so you can see **at a glance when the band opens and closes.** Hovering over a square shows the exact figures for that hour, including the MUF (explained next).


### Calling frequencies

A blue line shows the **common calling frequencies** for that band — the SSB, CW, and FT8 spots portable operators typically gather on — so a recommendation is immediately actionable. (Always operate within your license privileges; see the Band Plan chapter.)


### Your antennas for this band

Finally, the card lists which of **your** antennas suits this band, drawn from your inventory. Each is tagged so you know whether it's a straightforward match the program modeled from a standard library, or one that needed (or would benefit from) custom modeling. If you own nothing suited to the band, the card says so plainly rather than inventing a suggestion.


## Understanding the Derived Outputs

A few of the results are worth a second, plain-language pass so the numbers mean something:

- **Reliability (%)** — the chance of a successful contact at that time. It already blends everything you entered: the solar level, your power, the noise at your site, the distance to your target, and the time of day. You don't add anything to it; it's the bottom line.
- **Best hour** — simply the hour that scored highest. Great for deciding *when* to operate, not just *what band*.
- **MUF (Maximum Usable Frequency)** — shown when you hover a heatmap square. It's the highest frequency the atmosphere is bending back to Earth on your path at that moment. Bands **below** the MUF tend to work; bands **above** it usually don't. It's a useful sanity check: if a band's frequency is comfortably under the MUF, that's a good sign.


## Exporting Your Plan as a PDF

Once you have results, an **Export** bar appears. It lets you save your plan to a PDF (Portable Document Format) file to print or carry on a phone:

1. Tick which sections to include — **Bands**, **Antennas**, and/or **Checklist** — in any combination.
2. Press **Export PDF…**.
3. Choose where to save the file. Open or print it like any other PDF.


## The "Sample Data" Banner

If you ever see an orange banner saying the predictions are **sample data**, it means the propagation engine (VOACAP) isn't set up yet, so the numbers shown are illustrative placeholders, not a real forecast. Everything on the screen still works so you can learn your way around — but for real predictions, make sure VOACAP is installed (see the Installation Guide). Once it is, the banner disappears and the numbers are the real thing.


## Troubleshooting

| Symptom | What to do |
| --- | --- |
| The Sunspot number is blank or looks wrong | Press **↻ Solar** to refetch. No internet (or on a hotspot with no signal)? Type a number by hand — around 70 is a fair average if you have nothing better. |
| Every band shows low reliability / "no opening" | Check your **target** — a very long path in poor conditions can genuinely be dead. Also check the **K-index** on the Live line; a geomagnetic storm quiets the bands. Try a nearer target or a different time. |
| A band I expected is missing or last | The list is ranked by predicted reliability for your path and conditions. A band that's poor right now sinks to the bottom; check its heatmap for a better hour. |
| It says "sample data" | VOACAP isn't configured. The numbers are placeholders until you install it — see the Installation Guide. |
| "Use my location" is off, especially on a hotspot | Automatic location over a cellular hotspot can be far off. Type your latitude/longitude, or better, enter your **grid square** (see the Location chapter). |
| No antennas listed on any band | Your inventory has no antennas yet, or none suit these bands. Add antennas on the **Inventory** screen (Chapter 3). |


# 5. Quick Mode — Recommendations in One Click

*The fast path: skip the setup and planning screens and land straight on a full band-and-antenna recommendation for right here, right now.*

> **QUICK VERSION** — **Quick plan** gets you a full set of band and antenna recommendations for your current location and the current conditions with **no setup and no form-filling.** Click **Quick plan**, and you're looking at ranked bands in seconds. It's the same recommendation view as the main planning screen — just with the inputs chosen for you automatically.


## What This Is / What It Is For

Quick Mode is for the moments when you just want an answer **now** — you've arrived at a park or a summit, conditions are what they are, and you want to know which band to try first without stopping to fill anything in. It reuses the exact same prediction engine and the exact same recommendation view as the full **Plan session** screen (Chapter 4); it simply makes all the input choices for you using sensible defaults and your current situation.

> **IT'S THE FULL VIEW, NOT A STRIPPED-DOWN ONE** — Quick Mode does **not** give you a shortened or "lite" set of results. You get the complete band list — reliability, best hour, the 24-hour heatmap, calling frequencies, and matched antennas — identical to what the main planning screen produces. The only thing that's "quick" is getting there.


## Two Ways to Reach Quick Mode

You can get into Quick Mode two ways:

- **From the first-run setup wizard:** press **Skip** on any wizard step (Chapter 2). This is handy the very first time you open the program if you don't want to enter gear yet.
- **From the navigation bar, any time:** click **Quick plan**. Use this whenever you want fast recommendations later on.


## What Quick Mode Fills In For You

So you don't have to, Quick Mode makes these choices automatically:

- **Your location** — taken automatically from your current position (the same as pressing *Use my location* on the main screen).
- **The conditions** — the current live **sunspot number** and solar data, fetched for you.
- **The date and time** — right now.
- **Sensible defaults** — a reasonable power level and a general-operating framing, so the prediction is realistic out of the box.

Everything is chosen to give you an honest, useful answer for your current situation with zero input. If any of it isn't quite right — say your location came out wrong on a hotspot, or you're running QRP power — you can refine it, as described below.


## What You Get

Quick Mode lands you on the full recommendation view: a list of bands ranked best-first, each on its own card showing the reliability score and bar, the best hour, the 24-hour heatmap, the common calling frequencies, and which of your antennas suits that band. Every one of those is explained in detail in Chapter 4 — Quick Mode just gets you looking at them faster.

> **ANTENNA MATCHES NEED GEAR** — The band recommendations work even with an empty inventory, but the program can only suggest antennas you've told it you own. If you reached Quick Mode by skipping setup, the band advice is still solid — add your antennas later on the **Inventory** screen (Chapter 3) to get antenna matches too.


## Turning a Quick Look into a Full Plan

Quick Mode and the main planning screen are the same engine, so moving between them is seamless. When you want more control — a specific DX target, a different power level, an NVIS/regional framing, or a PDF export — just switch to **Plan session** in the navigation bar. Your location and the live conditions carry the same values, and you can adjust any input and press **Generate plan** to refine the result.


## Troubleshooting

| Symptom | What to do |
| --- | --- |
| My location looks wrong (especially on a phone hotspot) | Automatic location over a cellular hotspot can be off by a lot. Switch to **Plan session** and type your latitude/longitude, or enter your **grid square** (Chapter 13). |
| No antennas are suggested | Your inventory is empty (common if you skipped setup). Add antennas on the **Inventory** screen (Chapter 3); the band advice is still valid without them. |
| The power seems too high/low for my radio | Quick Mode uses a default. For an accurate weak-signal picture, switch to **Plan session** and set your real transmit power. |
| It says "sample data" | The propagation engine (VOACAP) isn't installed yet, so the numbers are placeholders. See the Installation Guide to set it up. |
| I wanted to enter gear after all | Open the **Inventory** screen any time to add gear — you don't have to re-run the setup wizard. |


# 6. Mission Type and Your Packing Checklist

*Tell the program what kind of operation you're planning, and it tailors your gear suggestions and builds a packing checklist you can check off and print.*

> **QUICK VERSION** — Pick your **operation type** (POTA, SOTA, Field Day, EMCOMM, or General) at the top of the Mission screen. The program marks the gear it recommends with clear **SUGGESTED** and **ESSENTIAL** badges, listing gear you own first. Tick the **printer box** next to the items you want on your packing list, press **Print checklist…**, and you get a PDF of exactly those items. When you're actually packing, use the **packed** check-off to tick items into the bag.


## What This Is / What It Is For

Different operations need different gear. A lightweight summit hike (SOTA) rewards a tiny QRP radio and a wire antenna; a Field Day setup can run a bigger radio and a computer; an emergency deployment (EMCOMM) leans on batteries, regional antennas, and go-kit items. The **Mission** screen lets you tell the program which kind of operation you're planning so it can **tailor two things at once**: the gear it suggests, and the way it frames the propagation question on the planning screen.


## Choosing Your Operation Type

At the top of the screen, pick the operation type that fits. Each is a common way hams operate:

| Type | What it means | In full |
| --- | --- | --- |
| POTA | Parks on the Air | Operating from a public park to activate it for others to contact. |
| SOTA | Summits on the Air | Operating from a mountain summit — weight and low power matter most. |
| Field Day | The annual ARRL Field Day (and similar) | A group or solo emergency-preparedness operating exercise; often bigger stations. |
| EMCOMM | Emergency Communications | Real or practice emergency operating, usually covering a nearby region. |
| General | Everyday operating | No special constraints — the all-purpose choice. |


## How the Mission Type Changes Things

Your choice affects the program in two distinct ways:

- **It tailors your gear suggestions.** The program flags gear that suits the operation. A SOTA choice favors low-power, lightweight gear; Field Day and EMCOMM bring computers, higher power, and go-kit items into the picture.
- **It frames the propagation question** on the planning screen (Chapter 4). EMCOMM, for example, switches the framing toward **regional / NVIS** coverage — a few hundred miles around you — instead of chasing distance.

> **IT CHANGES THE QUESTION, NEVER THE PHYSICS** — Selecting EMCOMM changes *what the program asks* the propagation engine — regional coverage instead of long-distance — but it never fudges the answer. The reliability numbers are always the honest, physics-based prediction for whatever path you've framed. Mission type steers the question; it doesn't put a thumb on the scale.


## Reading the Checklist — SUGGESTED and ESSENTIAL

Below the operation type, the program lists gear, with the items it recommends called out so you **can't miss them**:

- **★ ESSENTIAL** — a bold badge with a colored accent bar down the left edge. This is gear you really shouldn't leave without for the chosen operation.
- **★ SUGGESTED** — a bold badge, also with a left accent bar, marking gear that fits the operation well and is worth bringing.

Items without a badge are simply available in your inventory but not specifically called for by this operation type. Nothing is hidden — the badges and ordering guide you, but the whole list stays visible so you're always in control.


## Owned Gear First, Then Things to Consider

The list always shows gear **you own** first. Where the program thinks a category would help but you don't own a suitable item, it may note that separately as something to consider acquiring — kept clearly apart from your owned gear, so "you have this" and "you might want this" are never mixed into one confusing list.


## The Two Kinds of Checkbox

Each item has **two** independent checkboxes, and they do different jobs. This split is deliberate — choosing what to print is not the same as ticking things into your bag.

| Checkbox | What it's for |
| --- | --- |
| 🖨 (printer) | **Select for print.** Tick this on the items you want to appear on your printed packing list. Only ticked items go into the PDF. |
| Packed | **The pack check-off.** Use this while you're actually loading the bag, to tick each item as it goes in. It's your live packing tracker, separate from what you chose to print. |

> **WHY TWO CHECKBOXES** — You might want to print a full list but only pack part of it, or print a subset and still track everything as you load up. Keeping *what to print* and *what's packed* separate means one never overwrites the other.


## Select All or None at Once

To save clicking, the screen offers **Select all** and **Select none** buttons for the print selection. Press **Select all** to mark every item for printing (then untick the few you don't want), or **Select none** to clear the selection and start fresh.


## Printing Your Checklist as a PDF

When your print selection looks right, save it as a PDF (Portable Document Format) file you can print on paper or carry on your phone:

1. Tick the **🖨 printer box** on each item you want on the list (or use **Select all** then untick a few).
2. Press **Print checklist…**.
3. Choose where to save the PDF file.
4. Open or print it like any other PDF. Only the items you selected appear on it.

> **IT PRINTS ONLY WHAT YOU SELECTED** — The PDF contains exactly the items whose printer box is ticked — nothing more. If the list comes out empty or short, check that you ticked the printer boxes (not just the packed boxes).


## Troubleshooting

| Symptom | What to do |
| --- | --- |
| The gear list didn't change when I picked a different operation type | The full inventory always stays visible; what changes is the **badges** and the **ordering**. Look for items gaining or losing a ★ SUGGESTED / ★ ESSENTIAL badge and moving up the list. |
| My printed checklist is empty or missing items | You likely ticked the **packed** boxes instead of the **🖨 printer** boxes. Only printer-ticked items are printed. Tick those and print again. |
| A radio I expected to be suggested isn't | Suggestions use the power level in the item's note. For example, SOTA favors low-power (QRP) radios. Add a wattage to the radio's note on the **Inventory** screen so the program can match it. |
| An item I own isn't listed at all | It isn't in your inventory yet. Add it on the **Inventory** screen (Chapter 3). |
| I want to print everything | Press **Select all**, then **Print checklist…**. |


# 7. Antenna Patterns — Theory, and Reading the 2D and 3D Plots

*A plain-language primer on how antennas radiate, then a complete guide to the 2D polar plot and the 3D far-field surface — what you're looking at, and what to look for.*

> **QUICK VERSION** — Open the **Antenna Patterns** tab, pick one of your antennas and a band. The **2D** view is a slice showing gain vs elevation angle (horizon to straight up), with a green **take-off angle** line. Flip the **2D/3D** toggle for a rotatable 3D shape of where your signal goes — **distance from the center is gain**, and **color** goes blue (weak) to red (strong). The single most useful thing to read is the **take-off angle**: low (under ~20°) is for distance; high (toward straight up) is for nearby/regional (NVIS).


## Antenna Theory in Plain Language

Before the plots, here's the small amount of antenna theory that makes them meaningful. No math, no prior knowledge — just the ideas you need to read the pictures and make good choices. If you already know this, skip to the plots; if you don't, ten minutes here pays off every time you plan.


### What a radiation pattern is

An antenna doesn't send your signal equally in all directions. It's strong in some directions and weak in others. A **radiation pattern** is simply a map of that — a picture of how "loud" the antenna is in every direction. Imagine the antenna at the center and the signal puffing outward in a 3D shape: fat where it's strong, pinched where it's weak. That shape is the pattern, and it's the single most useful thing to know about an antenna, because it tells you **where your power actually goes.**

> **WHY DIRECTION MATTERS MORE THAN POWER** — Two antennas fed the same 10 watts can perform completely differently, because one aims the energy where you need it and the other wastes it. That's why a good antenna in the right pattern beats more power almost every time.


### Take-off angle — the most important idea

Of everything in a pattern, the **take-off angle** matters most. It's the angle above the horizon where the antenna radiates most strongly. Picture standing at your antenna and looking up: 0° is straight out at the horizon, 90° is straight up overhead. The take-off angle is where the signal's strongest lobe points.

It matters because it largely decides **how far you reach**:

- **Low take-off angle (roughly under 20°)** — the signal leaves nearly parallel to the ground and takes long, shallow skips off the ionosphere. This is what you want for **DX / long distance.**
- **High take-off angle (60° up to straight overhead)** — the signal goes up steeply and rains back down over a region a few hundred miles across. This is exactly what **NVIS / regional and emergency** work needs.
- **In between (~20–50°)** — a general-purpose compromise good for medium distances.

> **JARGON, IN PLAIN WORDS** — **Ionosphere:** a high layer of the atmosphere that bends shortwave signals back to Earth, letting them skip long distances. **NVIS (Near Vertical Incidence Skywave):** deliberately aiming nearly straight up so the signal comes back down over a wide nearby area.


### Gain, and what "dBi" means

**Gain** is how much an antenna concentrates your signal in its best direction, compared with a simple reference antenna. It's measured in **dBi** (decibels over an ideal reference). You don't need the math — just the feel: **bigger dBi means a stronger signal that way.** A few dB is a noticeable difference; about 6 dB is roughly "twice as strong" to the ear. Gain isn't free power — an antenna with high gain in one direction is simply quieter in other directions. It focuses what you have.


### Height and the ground — why the same antenna changes

This surprises new operators: **the exact same antenna radiates very differently depending on how high it is and what's under it.** The ground acts like a mirror, and the reflected signal combines with the direct signal — reinforcing it at some angles, cancelling it at others. Raise a horizontal antenna and its take-off angle drops (better for distance); lower it and the angle rises (better for regional/NVIS). This is why the program asks so carefully for **height** (Chapter 3): height isn't a detail, it's a main control of where your signal goes.

> **THIS IS WHY YOUR NUMBERS MATTER** — Because height and dimensions change the pattern so much, the plots are only as accurate as the measurements you entered. A wrong height gives a wrong-but-confident-looking pattern. Get the inputs right (Chapter 3) and the picture is trustworthy.


### Polarization (briefly)

**Polarization** is the orientation of the radio wave — vertical or horizontal — set by the antenna. A vertical antenna makes vertically-polarized waves; a horizontal dipole makes horizontal ones. For the planning this program does you don't need to fuss over it, but it's why a vertical and a dipole "feel" different on the air. It's mentioned here only so the word isn't a mystery.


### Wavelength and resonance — why "leave it 0" works

Radio waves have a physical length (a **wavelength**), and antennas work best when their size is a natural fraction of it — a **half-wave** or **quarter-wave**. An antenna cut to the right length for a band is **resonant** and radiates efficiently. This is why Chapter 3 says you can leave a length at **0**: the program then models a properly-resonant length for whatever band it's checking, so you get a correct-shaped pattern even without measuring. Enter the real length and it models exactly what you have.

> **THE ONE TAKEAWAY** — If you remember one thing: **look at the take-off angle.** Low = far away (DX). High = nearby (regional/NVIS). Everything else on the plots supports reading that.


## Opening the Antenna Patterns Tab

Click **Antenna Patterns** (labeled **Antenna**) in the navigation bar. Choose an **antenna** and a **band**. The dropdown lists your own antennas first, and also a few **example** wires (a Half-Wave Dipole, an Inverted-V, and an End-Fed — marked "(example)") so you can always explore a common pattern even before adding gear. Because a pattern changes with band, switch bands to see how the same antenna behaves across the spectrum.

At the top-right of the plot is the **2D / 3D toggle**. The left panel always shows the numeric readout — **Peak gain** and **Take-off** angle — for the current antenna and band.


## The 2D Polar Plot — Every Element Explained

The 2D view is an **elevation slice**: a vertical cut through the pattern, showing gain versus elevation angle from the horizon up over the top and back down to the horizon. It looks like a dome (the classic EZNEC-style elevation plot). Reading it is simple once you know the parts:

| Element | What it is / how to read it |
| --- | --- |
| The dome outline (red curve) | The pattern itself — the further the red curve is from the center in a direction, the stronger the antenna radiates at that elevation angle. |
| dB rings (0, −10, −20, −30) | Concentric range rings. The outer ring is 0 dB (the peak/strongest), and each ring inward is 10 dB weaker, down to −30 dB at the very center. So a lobe reaching the outer ring is at full strength; one that only reaches −20 is much weaker. |
| Angle labels (0° / 30° / 60° / 90°) | Elevation angle, on both sides: 0° at each horizon, 90° straight up at the top. Read where the lobe points against these. |
| Green dashed take-off line | The take-off angle — the elevation of peak gain — drawn as a distinct line and labeled "take-off N°". This is the number to read first (see the theory above). |
| Peak readout (top-left) | "Peak X dBi @ N° elevation" — the maximum gain and the angle it occurs at, in words. |

> **READING IT AT A GLANCE** — Find the green take-off line: if it's low (near a horizon), the antenna favors distance; if it stands up near 90°, it favors overhead/regional coverage. Then see how far the red lobe reaches the outer (0 dB) ring in that direction — that's your strongest shot.


## The 3D Far-Field Surface — Every Element Explained

Flip the toggle to **3D** for a rotatable surface showing the whole pattern at once — the same idea as a 3D plot in EZNEC. It's the most intuitive way to "see" where your signal goes. Drag with the mouse to **rotate**, scroll to **zoom**.

| Element | What it is / how to read it |
| --- | --- |
| The colored surface | The pattern as a 3D shape. Distance from the center in any direction = the gain that way, so the surface bulges toward strong directions and pinches toward weak ones. |
| Color (blue → red) | Also gain: blue is weak, through green and yellow, to red for the strongest directions. The color scale (bottom-right) shows the range (Strong at top, Weak / −30 dB at bottom). |
| Wireframe grid lines | The thin curves over the surface (constant-azimuth "meridians" and constant-elevation "rings") — they give the shape its 3D structure, like the lines on a globe. |
| Ground plane (rings + spokes) | The flat grid at the bottom is the horizon (ground). The concentric rings and 8 spokes are range/compass references so you can judge direction and how low the lobe reaches. |
| Compass labels (N / E / S / W) | Direction around the horizon, so you can tell which way a directional antenna favors. |
| Zenith / Horizon labels | "Zenith" marks straight up (90° elevation) at the top of the vertical axis; "Horizon" marks the ground plane (0°). |
| Green take-off line | A bold line from the center out to the single strongest direction — the 3D echo of the 2D take-off line. |
| Info card (top-left) | Antenna name, peak gain, take-off angle, and a reminder that distance from center = relative gain, plus the drag/scroll controls. |

> **2D AND 3D SHOW THE SAME DATA** — The 2D plot is one vertical slice through the 3D surface (taken at the strongest azimuth). Use 3D to understand the overall shape and which compass directions are favored; use 2D to read exact angles and dB levels. They always agree.


## What to Look For, by Antenna Type

Here's how common antennas look on the plots and what that means for operating:

| Antenna | What the pattern looks like | What it means |
| --- | --- | --- |
| Vertical (with radials) | A low, flattened donut hugging the horizon, with a null (dimple) straight up. | Low take-off angle, all directions equally — good all-around for DX/distance; poor for close-in contacts. |
| Dipole (up high) | Two broad lobes broadside to the wire (a figure-8 from above), at a fairly low angle. | Favors distance broadside to the wire; aim the wire so its broadside faces your target. |
| Dipole or wire strung LOW | A big rounded lobe pointing nearly straight up. | High take-off angle — an NVIS/regional antenna. Great for a few-hundred-mile net; not for DX. |
| NVIS crossed dipole | A dome filling the sky overhead, fairly even in all compass directions. | Purpose-built high-angle regional coverage — the classic emergency/regional choice. |
| End-fed half-wave | Similar to a dipole but often a bit more lopsided; angle depends on height. | A versatile portable wire; check its take-off angle for the height you actually use. |

> **MATCH THE PATTERN TO THE MISSION** — Chasing DX? Want a low take-off angle (vertical, or a dipole up high). Running a regional or emergency net? Want a high take-off angle (a low dipole or an NVIS antenna). The plots let you confirm your antenna actually does what the mission needs — before you're in the field.


## Where the Pattern Comes From

The pictures are computed for **your** antenna by the **NEC2++** modeling engine (Chapter 2) from the measurements you entered — not stock images. That's why accurate inputs matter (Chapter 3). Two labels tell you how much to trust a given pattern:

- **Measured** — a straightforward wire-and-dimensions antenna the engine models accurately. Trust the pattern.
- **Approximate** — a loaded or broadband antenna (coils, traps) a simple wire model can't capture exactly; the shape and take-off angle are a good guide, not an exact measurement.

> **SAMPLE MODE** — If NEC2++ isn't installed, the tab shows a **"representative pattern"** note and the shapes are realistic stand-ins tied to your antenna's height — useful for learning, but install NEC2++ (see the Installation Guide) for exact modeled patterns.


## Troubleshooting

| Symptom | What to do |
| --- | --- |
| The pattern looks obviously wrong | Check the antenna's measurements on the Inventory screen (Chapter 3) — a wrong height or length is the usual cause. The engine models exactly what you entered. |
| The 3D view shows a message instead of a shape | Either the antenna has no 3D data (use 2D), or your computer has no Vulkan-capable graphics — in which case the program falls back to the 2D plot automatically. |
| The pattern changes when I switch bands | That's correct — antennas radiate differently on different bands. Always view the band you'll actually use. |
| It says "representative pattern" / Approximate | Representative = NEC2++ isn't installed (sample mode). Approximate = a loaded/broadband antenna. Both are expected; see above. |
| 3D looks flat or oddly framed | Give it a moment to auto-frame, then drag to rotate. Scroll to zoom in or out. |
| I don't own the antenna I want to view | Pick one of the built-in "(example)" wires in the dropdown, or add your antenna on the Inventory screen (Chapter 3). |


# 8. The Grey Line — Working the Sunrise/Sunset Edge

*For a short window around your sunrise and sunset, the low bands can reach much farther than usual. This tab tells you plainly whether you're in that window right now, when the next one is, and what to do about it.*

> **QUICK VERSION** — Open the **Grey Line** tab. A floating box tells you in plain words whether the grey line is **active right now** or **not** — and if not, when the next window opens. During a grey-line window (around your local sunrise and sunset), the low bands (like 40, 80, and 160 meters) can reach unusually far, so it's a great time to try for distance on them.


## What This Is / What It Is For

The **grey line** is the moving line of twilight that circles the Earth — the boundary between day and night — passing over you at **sunrise** and **sunset**. For a short time while that line is over you, the atmosphere is in a special in-between state that lets the **low bands** carry signals much farther than they do in full daylight or full darkness. Operators call chasing this effect "working the grey line." This tab makes it easy to catch.

> **JARGON, IN PLAIN WORDS** — **Grey line:** the twilight zone between day and night sweeping around the globe. **Why it matters:** the daytime layer of the atmosphere that normally absorbs low-band signals fades out fast at twilight, while the reflecting layer is still there — so for a little while, signals on the low bands skip long distances with less loss than usual.


## Reading the Floating Status Box

Open the **Grey Line** tab from the navigation bar. The first thing you'll see is a **floating status box** that answers the only question that matters in the moment, in plain language:

- **"Grey line is ACTIVE now"** — you're in a window right now. Good time to try the low bands for distance.
- **"Grey line is not active"** — you're outside a window, along with **when the next one begins** so you can plan for it.

The box floats above the rest of the tab so the answer is always right in front of you, no matter how far you scroll. The whole tab scrolls, so nothing gets cut off on smaller screens.


## The Window Times

Below the status, the tab lists **today's grey-line windows** — the times around your **sunrise** and your **sunset**, calculated from your latitude, longitude, and today's date. These are real astronomical times for your exact location, not rough estimates. Each window is a span (it begins a little before the exact sunrise/sunset moment and lasts a little after), because the helpful effect builds and fades rather than switching on and off instantly.

> **TIMES FOLLOW YOUR LOCATION** — Because the windows come from your position, make sure your location is set correctly (Chapter 13) — especially in the field on a hotspot, where automatic location can be off. Wrong location means wrong sunrise/sunset times.


## The "What To Do" Guidance

Times alone don't tell a newer operator what to *do*, so the tab includes plain-language guidance: which bands to reach for during the window (the low bands), that it's aimed at **distance** work, and a reminder that the effect is strongest right around the sunrise/sunset moment. The idea is that you can act on the tab without needing outside knowledge — see you're in a window, switch to a low band, and try for some distance.


## Why It Isn't a Score Boost

You might expect the grey line to bump a band's reliability score on the planning screen. It deliberately does **not.** The propagation engine (VOACAP) already models time-of-day effects, so the sunrise/sunset benefit is largely baked into the normal predictions. Adding a separate "grey-line bonus" on top would **count the same effect twice** and mislead you. So the grey line lives here as its own clear indicator — a *"now's a good moment for the low bands"* nudge — while the band scores stay honest on the planning screen.

> **USE BOTH TOGETHER** — Read the grey-line tab alongside the planning screen: if a low band already scores decently on the plan **and** you're in a grey-line window, that's a strong signal to try it for distance right then. The tab tells you *when*; the plan tells you *how good*.


## Troubleshooting

| Symptom | What to do |
| --- | --- |
| The window times look wrong | Check your location (Chapter 13). Grey-line times are computed from your latitude/longitude and date — a wrong location (common on a hotspot) gives wrong sunrise/sunset times. |
| It says not active but I expected a window | The windows are around **your** sunrise and sunset only. If it's midday or the middle of the night for you, there's no window now — check the "next window" time. |
| Part of the tab is cut off | The whole tab scrolls; scroll down to see the window times and guidance. The status box stays floating in view. |
| Grey line doesn't change my band scores | That's intended — VOACAP already includes time-of-day effects, so the grey line is shown separately rather than double-counted. Use it as a timing nudge alongside the plan. |


# 9. The Propagation Trend — Are Conditions Improving or Fading?

*A rolling few-hour view that samples conditions in the background so you can see, at a glance, whether a band is getting better or worse over time — not just how it looks this second.*

> **QUICK VERSION** — The **Trend** view watches conditions over the last few hours and shows whether bands are **improving or fading**. It refreshes itself in the background every 15–30 minutes — you don't press anything. Leave the program open during your session and glance at the trend to decide whether to keep working a band or move on. The trend resets each time you start the program; it isn't saved.


## What This Is / What It Is For

The planning screen gives you a **snapshot** — how the bands look right now. But conditions move: a band can be climbing toward a peak or sliding into a slump, and the snapshot alone can't tell you which. The **Trend** view fills that gap by keeping a **rolling few-hour history** of the predictions, so you can see the *direction* things are heading.

Knowing the direction changes your decisions. A band that's mediocre right now but clearly **improving** is worth waiting on; a band that looks fine but is **fading** is one to work now before it's gone. That's the value of a moving picture over a single frame.


## It Samples Automatically in the Background

You don't have to do anything to build the trend. While the program is open, it quietly re-runs the propagation prediction every **15 to 30 minutes** and adds each result to the rolling history. Over a session, that builds up into a picture of how conditions are changing. Because it's automatic and in the background, the trend is simply *there* whenever you switch to the tab.

> **IT NEEDS THE PROGRAM LEFT OPEN** — The trend builds only while Activation Planner is running. If you just opened the program, the trend will be sparse until a few sampling intervals have passed. For a useful trend, leave the program open through your session.


## Reading the Trend

The trend plots recent prediction samples over the last few hours so you can read the **slope**: a line climbing to the right means conditions are improving; a line sloping down means they're fading; a flat line means steady. Use it to answer the practical question — *is this band on the way up or on the way down?* — rather than to read exact numbers, which the planning screen already gives you precisely.


## It's Session-Only — and Why

The trend is **not saved.** When you close the program, the rolling history is cleared, and it starts fresh next time. This is deliberate and matches how the whole program works: Activation Planner is **stateless** — it doesn't keep a history of your sessions. Every plan is based on the current time and the current solar data, so a stale trend from days ago would be misleading. Keeping the trend session-local means what you see always reflects the here and now.

> **CONSISTENT WITH STATELESS REPLANNING** — The program never tracks past sessions or activations. Each replan uses the current time and current conditions. The trend follows the same rule — it lives only for the current session — so you're never fooled by old data.


## How It Works With the Main Plan

Use the two together. The **planning screen** tells you which band is best *right now* and how good it is; the **Trend** view tells you whether that's likely to *hold, improve, or fade* over the next while. A common pattern: pick the best band from the plan, then check the trend to decide whether to jump on it immediately (fading) or set up other things first (still improving).


## Troubleshooting

| Symptom | What to do |
| --- | --- |
| The trend is empty or nearly flat with few points | You just opened the program. The trend builds every 15–30 minutes — leave the program open and check back after a couple of intervals. |
| The trend disappeared after I restarted | That's expected — the trend is session-only and isn't saved, in keeping with the program's stateless design. It rebuilds as the new session runs. |
| The trend isn't updating | Background sampling needs the propagation engine (VOACAP) working and, for live solar data, an internet connection. Check the planning screen isn't showing "sample data," and that you're online. |
| I want exact numbers | The trend shows direction; for precise figures use the planning screen (Chapter 4), which you can regenerate any time. |


# 10. Weather Forecast and Safety Alerts

*A local forecast for your operating site, plus automatic weather watch and warning alerts that reach you anywhere in the program — because a summit or a park in bad weather is a safety matter, not just a comfort one.*

> **QUICK VERSION** — The **Weather** tab shows a local forecast for wherever you're operating. Separately, the program watches for official weather **watches and warnings** for your area the whole time it's running — no matter which screen you're on. A serious alert (a warning) pops up a **full-screen notice you must click to clear**; a minor one shows a **banner**. This runs app-wide so you can't miss dangerous weather while heads-down planning.


## What This Is / What It Is For

Portable operating happens outdoors — on summits, in parks, in fields — where weather is a real safety factor, not a footnote. Activation Planner therefore does two weather things: it shows you a **forecast** for your operating location, and it **actively watches for official alerts** (watches and warnings) and interrupts you if something dangerous is issued for your area. The second part is the important one: it's designed so you find out about a severe thunderstorm or tornado warning even if you're buried in the planning screen.


## Where the Forecast Comes From

The forecast is pulled from the official government weather service for **your location** (from your latitude and longitude). Because it's tied to your position, it describes the weather where you'll actually be, not a nearby city. It needs an internet connection — a phone hotspot in the field is fine.

> **GET YOUR LOCATION RIGHT** — As with the grey line, the forecast and alerts follow your set location. On a hotspot, automatic location can be off — set your position or grid square accurately (Chapter 13) so you get the forecast and alerts for where you really are.


## Reading the Forecast

Open the **Weather** tab. It shows a series of upcoming periods (like "This Afternoon," "Tonight," "Tomorrow") with the expected conditions for each — temperature, sky, wind, and a short plain-language description. Use it to decide whether it's a good day to be out, what to pack, and how long you can safely operate.


## The App-Wide Alert System

This is the part that runs everywhere. While the program is open, it checks for **active weather alerts** for your area on a regular schedule (about every 10 minutes) in the background. If an official watch or warning is in effect, the program shows it to you **no matter which tab you're on** — you don't have to be looking at the Weather tab. This is intentional: alerts are a safety feature, so they follow you across the whole app.

> **JARGON, IN PLAIN WORDS** — A **watch** means dangerous weather is *possible* — be prepared. A **warning** means it's *happening or imminent* — act now. The program treats warnings as more urgent than watches, and shows them more forcefully.


## The Two Alert Levels

How forcefully the program interrupts you depends on how serious the alert is:

| Severity | How it appears | What to do |
| --- | --- | --- |
| Serious (a warning / moderate-or-worse alert) | A **full-screen notice** that blocks the rest of the program until you clear it, showing the alert headline. | Read it and take it seriously. This is designed to stop you and make you look. |
| Minor (a lesser advisory) | A **banner** across the top of the window, without blocking your work. | Note it and carry on; keep an eye on developing conditions. |

The severity gate means routine minor advisories don't nag you with a full-screen block, while genuinely dangerous warnings are impossible to overlook.


## Clearing an Alert

A serious alert's full-screen notice **must be clicked to clear** — it won't dismiss itself. That's on purpose: it forces you to actually acknowledge the danger rather than let it flash by while you're typing. Once you've cleared a specific alert, the program remembers you've seen it and won't keep re-popping the same one at you; a **new** or changed alert will show again.

> **CLEARING THE NOTICE DOESN'T CLEAR THE WEATHER** — Dismissing the on-screen alert only acknowledges that you've read it. The actual weather danger is still out there. Use the alert as your prompt to check the sky, secure your gear, and — if it's a warning — get to safety. Radio can wait.


## Staying Safe in the Field

A few plain reminders the alert system is there to support: an antenna and mast are tall metal objects — get them down and get to shelter well before lightning arrives, not after. A summit is the worst place to be in a thunderstorm. When a warning fires, **pack up and leave**; the activation isn't worth it. The program surfaces the alert so you have as much lead time as possible to make that call.


## Troubleshooting

| Symptom | What to do |
| --- | --- |
| No forecast appears | Check your internet connection (a hotspot is fine) and that your location is set (Chapter 13). The forecast comes from an online weather service for your coordinates. |
| The forecast is for the wrong place | Your location is off — common on a cellular hotspot. Set your latitude/longitude or grid square accurately (Chapter 13). |
| A full-screen alert won't go away | Click it to acknowledge — serious alerts require a click by design. Then act on the weather itself, not just the notice. |
| I keep seeing the same alert | You should only be re-shown an alert if it's new or changed. If it persists, it's likely still active and being re-issued; treat it as ongoing. |
| I'm not getting alerts I expected | Alerts need an internet connection and a correct location, and are checked periodically (about every 10 minutes). Confirm you're online and your position is right; don't rely on the app as your only warning source. |


# 11. The Band Plan — Where You're Allowed to Operate

*A built-in, plain-language reference to the U.S. amateur band plan (FCC Part 97) so you can confirm a frequency is legal for your license and mode before you transmit — right beside your recommendations.*

> **QUICK VERSION** — The **Band Plan** tab is a quick reference to which frequencies you're allowed to use, broken down by license class (Technician, General, Amateur Extra) and mode (voice, Morse code, digital). Before you transmit on a frequency the planner suggested, glance here to confirm it's inside your privileges. It covers the U.S. HF bands through 70 centimeters.


## What This Is / What It Is For

In the United States, the Federal Communications Commission (FCC) — the government agency that regulates radio — sets rules (called **Part 97**) for exactly which frequencies each class of amateur license may use, and for which modes. Operating outside your privileges is against the rules. The **Band Plan** tab puts those rules right in the program, in plain language, so you can check a frequency without digging out a chart. It's a reference, not something you configure.

> **JARGON, IN PLAIN WORDS** — **Band plan:** the agreed-and-regulated map of a band — which slices are for voice, which for Morse code (CW), which for digital modes, and which license classes may use each slice. **Part 97:** the section of U.S. federal rules that governs amateur radio.


## Opening the Tab

Click **Band Plan** in the navigation bar. You'll see the amateur bands laid out from the lowest frequency (160 meters) up through the higher HF bands and into VHF/UHF (up to 70 centimeters), each with its sub-bands.


## Reading a Band

Each band is broken into **sub-bands** — segments set aside for particular modes and license classes. For each segment you'll see, in plain terms:

| Column | What it tells you |
| --- | --- |
| Frequency range | The start and end of the segment, in megahertz (MHz). |
| Mode(s) | What you may transmit there — for example CW (Morse code), phone (voice, such as SSB), or digital/data. |
| License class | Which license classes may use that segment (Technician, General, or Amateur Extra). |

So, for example, you can quickly confirm whether a voice frequency on 40 meters is open to a General-class operator, or whether a stretch of 20 meters is CW-only. The data reflects the current U.S. rules, including recent changes.


## License Classes at a Glance

U.S. amateur licenses come in three active classes, each with more privileges than the last:

- **Technician** — the entry-level license. Full privileges on VHF/UHF and limited HF privileges (notably some CW, and voice on part of 10 meters).
- **General** — substantial HF privileges across most bands, enough for worldwide operating.
- **Amateur Extra** — the top class, adding the exclusive bottom segments of several HF bands.

The tab shows which segments belong to which class so you can see, at a glance, what your license covers — and what upgrading would add.


## How It Works With Your Recommendations

The Band Plan tab pairs naturally with the planning screen. When the planner recommends a band and shows its **calling frequencies** (Chapter 4), pop over here to confirm those frequencies are within your license privileges and set for the mode you intend. The planner tells you *which band is best*; the band plan confirms *where on it you may legally operate*.


## Staying Legal and Current

> **YOU ARE RESPONSIBLE FOR OPERATING LEGALLY** — This tab is a helpful, good-faith reference built from the FCC Part 97 rules, but the **official FCC rules are the final authority.** Band plans can change, and this reference could lag a change. Always operate within your actual license privileges, and if in doubt, check the current official rules. The program never transmits for you — every transmission is your responsibility as the licensed operator.


## Troubleshooting

| Symptom | What to do |
| --- | --- |
| I'm not sure which class applies to me | Use the class you hold (Technician, General, or Amateur Extra). If you're unsure, check your FCC license record. |
| A calling frequency the planner showed isn't in my privileges | Then don't use that exact frequency — find a segment for your class and mode within the same band, or operate a mode your class allows there. |
| The band I want isn't shown | The tab covers the U.S. amateur bands from 160 meters through 70 centimeters. Bands outside that range aren't listed. |
| I think a segment is out of date | Treat the official FCC Part 97 rules as authoritative and operate accordingly; the tab is a convenience reference, not a legal document. |


# 12. The Battery Runtime Calculator — Will Your Power Last?

*Enter your battery and how you operate, and the program estimates how many hours you can run — so you don't get cut short in the field or haul more battery than you need.*

> **QUICK VERSION** — Open the **Battery** tab, enter your **battery size** (in amp-hours), your radio's **receive** and **transmit** current draw, and roughly **how much of the time you'll be transmitting**. The program estimates **how many hours** you can operate. Use it to pick the right battery for the length of activation you're planning.


## What This Is / What It Is For

In the field there's no wall outlet — your battery is all the power you have. Bring too little and your activation ends early; bring too much and you're carrying dead weight up a summit. The **Battery Runtime Calculator** does the arithmetic for you: given your battery and how you operate, it estimates how long you can keep going, so you can plan power with confidence.


## Opening the Tab

Click **Battery** in the navigation bar. You'll see a small set of input fields and an estimated runtime that updates as you fill them in.


## The Inputs, One at a Time

Each field has a plain-language hint in the program; here's what each one means and where to find the number.

| Field | What it means | Where to get the number |
| --- | --- | --- |
| Battery capacity (amp-hours, Ah) | How much energy your battery holds. A bigger number lasts longer. | Printed on the battery (e.g. "12 Ah"). It's often in the item's name if you entered power gear in your inventory. |
| Receive current (amps) | How much power the radio uses while just listening. This runs the whole time. | From the radio's manual/specs — often around 0.5–1 amp for a portable HF radio. |
| Transmit current (amps) | How much power the radio uses while transmitting. Much higher than receive. | From the radio's manual/specs at your operating power — e.g. a 100-watt radio can draw ~20 amps on transmit; a 5-watt QRP radio, ~1–2 amps. |
| Transmit duty cycle (%) | The share of your on-air time actually spent transmitting, versus listening. | An estimate — see the next section for realistic figures. |

> **JARGON, IN PLAIN WORDS** — **Amp-hour (Ah):** roughly, how many amps a battery can supply for one hour. A 12 Ah battery can give 12 amps for 1 hour, or 1 amp for about 12 hours. **Current draw (amps):** how fast a device drinks from the battery right now. **Duty cycle:** what fraction of the time you're transmitting.


## Why Receive, Transmit, and Duty Cycle All Matter

A radio sips power while listening and gulps it while transmitting, so how long your battery lasts depends heavily on **how much of the time you transmit** — the duty cycle. Ragchewing or casual operating might be 10–25% transmit; a fast activation pileup or a contest can be higher; some digital modes transmit steadily and run a high duty cycle. The calculator blends the low receive draw and the high transmit draw according to your duty cycle to get a realistic average — which is why a modest change in duty cycle can noticeably change the runtime.

> **WHEN IN DOUBT, BE PESSIMISTIC** — If you're not sure of your duty cycle, estimate a bit high. It's better to be pleasantly surprised by leftover power than to lose the last contacts of an activation. The same goes for transmit current — use the figure at the power level you'll actually run.


## Reading the Estimated Runtime

The program shows an **estimated number of hours** you can operate with the inputs you gave. Treat it as a solid planning estimate, not a guarantee — real batteries deliver a little less than their rated capacity, cold weather reduces capacity, and you generally shouldn't drain a battery all the way down. Build in a margin: if the estimate is 4 hours and you need 4, bring a bigger battery or plan a shorter session.


## Practical Tips for Making Power Last

- **Run less power.** Dropping from 100 watts to QRP (5–10 watts) dramatically cuts transmit draw — often the single biggest saving.
- **Listen more, call efficiently.** A lower duty cycle stretches the battery directly.
- **Keep batteries warm.** Cold cuts capacity; a cold battery on a winter summit won't deliver its full rating.
- **Don't fully drain it.** For battery health and a safety margin, plan to use most — not all — of the capacity.
- **Bring your power gear into the inventory.** If you entered your battery with its capacity, you'll have the number handy here.


## Troubleshooting

| Symptom | What to do |
| --- | --- |
| The runtime seems far too long | Check your **duty cycle** — leaving it very low, or entering transmit current as if it were receive, inflates the estimate. Use realistic figures. |
| The runtime seems far too short | Make sure battery capacity is in **amp-hours** and that you didn't enter transmit current where receive goes. Double-check the units. |
| I don't know my radio's current draw | Check the radio's manual or manufacturer specs (look for "current consumption" at receive and at transmit). As rough defaults: ~0.7 A receive, and transmit current scaling with power. |
| My battery is rated in watt-hours, not amp-hours | Divide watt-hours by the battery voltage (usually about 12) to get amp-hours — e.g. 120 Wh ÷ 12 V ≈ 10 Ah. |
| It lasted less than the estimate in real life | That's common — cold, aging batteries, and not fully draining all reduce usable capacity. Always plan with a margin. |


# 13. POTA — Live Spots and Self-Spotting

*See who's on the air right now for Parks on the Air, look up park information, and — when you're activating — post your own spot so chasers can find you, all with one manual press.*

> **QUICK VERSION** — The **POTA** tab shows a live list of activators on the air right now. When you're the one activating, fill in your park and frequency and press **Spot me** to post yourself so chasers can find you. One press posts one spot — nothing happens in the background, and you never need to log in.


## What This Is / What It Is For

**Parks on the Air (POTA)** is a popular program where operators ("activators") set up in a public park and make contacts, while "chasers" try to work them. A live **spotting** website shows who's on the air, where, and on what frequency. Activation Planner connects to POTA so you can both **see the spots** and, when you're activating, **spot yourself** so chasers know you're there.

> **JARGON, IN PLAIN WORDS** — **Spot:** a short public post saying "this operator is on the air at this park, on this frequency, right now." **Self-spot:** posting a spot for *yourself* when you're the activator, so chasers can find and work you.


## Viewing Live Spots and Park Info

Open the **POTA** tab to see the current spots — activators on the air now, with their callsign, park, frequency, and mode. You can also look up park information. This part is purely read-only: you're just viewing public information that POTA already publishes, which is handy for finding someone to contact or checking whether a park is already being activated.


## Self-Spotting — Putting Yourself on the Map

When **you** are the activator, self-spotting posts you to the live list so chasers can find you — especially valuable in a quiet park or on a band where nobody would stumble across you otherwise.

1. Enter the **park** you're activating and the **frequency** and **mode** you're on.
2. Press **Spot me**.
3. Your spot appears on the public POTA list. Press it again later (for example, when you change bands) to post an updated spot.

> **ONE PRESS = ONE SPOT** — Self-spotting is always a deliberate, manual action. The program never spots you automatically or in the background — it posts exactly once, each time you press the button, and only for you.


## The Good-Citizen Rules the Program Follows

POTA's spotting service is shared by the whole community, so Activation Planner is deliberately built to be a considerate guest on it. The program:

- **Only spots you** — it will never spot another operator. (The person spotting and the person being spotted are always the same: you.)
- **Only spots when you press the button** — no automatic, scheduled, or bulk spotting.
- **Identifies itself honestly** — each spot is tagged as coming from "Activation Planner" so POTA can see where it originated.

These constraints keep the feature useful to you without putting extra load on, or misusing, a community service.


## No Login Needed

Both viewing spots and self-spotting work **without any account or password** — they use POTA's open spotting service, so there's nothing to sign into. The only POTA activity that requires a login is **uploading your logs for award credit**, which is a completely separate task and is **not** something Activation Planner does. This program is a *planning and spotting* tool, not a logger.

> **LOGGING STAYS ELSEWHERE** — Activation Planner intentionally doesn't log your contacts — keep using your normal logging program for that. This tool helps you plan the activation and spot yourself; recording and submitting QSOs is a job for a dedicated logger.


## Troubleshooting

| Symptom | What to do |
| --- | --- |
| No spots are showing | Check your internet connection (a hotspot is fine). The spot list comes from POTA's online service. |
| My self-spot didn't appear | Confirm you're online, that the park and frequency are filled in, and press **Spot me** again. Each press posts one spot. |
| It asked me to log in | It shouldn't — spotting needs no login. If you hit a login prompt, that's for a separate service (like log upload), which this program doesn't do. |
| I changed bands and my spot is stale | Post a fresh spot with your new frequency by pressing **Spot me** again. |
| Can I spot a friend? | No — the program only ever spots you (self-spot only), by design. Your friend can spot themselves from their own setup. |


# 14. Location, GPS, and Grid Squares

*Everything the program does with your position — how it finds you, how to enter your location by hand or by grid square, and why getting it right matters for every prediction.*

> **QUICK VERSION** — Your location drives the whole program — predictions, grey line, weather, sunrise/sunset. Set it three ways: press **Use my location** to find you automatically, type your **latitude/longitude**, or type a **grid square** (like EM29) and press **Set from grid**. If you have a USB GPS receiver plugged in, the program uses it automatically for the best accuracy. In the field on a phone hotspot, the automatic guess can be way off — type your grid square instead.


## What This Is / What It Is For

Almost everything Activation Planner does depends on knowing **where you are**: the propagation predictions are computed for your exact path, the grey-line windows come from your sunrise and sunset, and the weather and alerts are for your area. Get the location right and everything downstream is right; get it wrong and every prediction is quietly off. This chapter covers every way to set it and how the program finds you.


## Three Ways to Set Your Location

You can set your position whichever way is easiest at the moment:

| Method | How | Best for |
| --- | --- | --- |
| Automatic | Press **Use my location**. | Quick setup when your automatic position is trustworthy (a wired/known connection, or a GPS receiver attached). |
| By latitude/longitude | Type the numbers in decimal degrees (e.g. 39.83, -98.58). South and West are negative. | When you know your exact coordinates. |
| By grid square | Type a Maidenhead grid (e.g. EM29) and press **Set from grid**. | The field — grids are short, easy to read off a map or memorize, and hard to mistype badly. |

The grid box and the latitude/longitude fields are linked **both ways**: set one and the other updates automatically (see the converter section below).


## How the Program Finds You

When you ask for your location automatically, the program tries the most accurate source available, in this order:

- **A hardware GPS receiver first.** If you have a GPS receiver connected over USB (a serial/NMEA device), the program uses it — this is the most accurate option and works even with no internet. It takes priority whenever it's plugged in, on a desktop or a laptop.
- **Geo-IP as a fallback.** With no GPS receiver, the program estimates your position from your internet connection. This is convenient but only roughly accurate — and it can be very wrong on a cellular hotspot (see below).

> **JARGON, IN PLAIN WORDS** — **GPS receiver:** a small device (often USB) that listens to positioning satellites and reports exactly where it is. **NMEA:** the standard "language" GPS receivers speak. **Geo-IP:** guessing your location from your internet connection's address — handy, but only a rough guess, and it doesn't use satellites.

> **NOT WI-FI/OS LOCATION SERVICES** — The automatic fallback uses geo-IP, not your operating system's Wi-Fi-based location service. For real accuracy in the field, a USB GPS receiver — or simply typing your grid square — beats the automatic guess every time.


## Refresh-on-Demand, Not Constant Tracking

The program checks your location **only when you ask it to** — when you press **Use my location** or refresh. It does **not** track you continuously in the background. This is deliberate: continuous tracking isn't needed for planning (you set up in one spot and operate), it would waste power, and refresh-on-demand respects your privacy. When you move to a new site, just refresh your location there.


## Grid Squares and the Two-Way Converter

A **Maidenhead grid square** (or "grid locator") is hams' shorthand for a location — a short code like **EM29** or **EM29qb** that names a rectangle on the map. Shorter codes name bigger rectangles; longer codes pin you down more precisely. Hams use grids constantly because they're compact and easy to exchange, and because a small typo produces an obviously-wrong grid rather than a subtly-wrong coordinate.

Activation Planner converts **both directions** automatically:

- **Grid → coordinates:** type a grid square and press **Set from grid**; the latitude and longitude fill in.
- **Coordinates → grid:** enter or update your latitude/longitude and the grid square updates to match.

So you can work in whichever is handier and always have the other. In the field, entering your grid is often the fastest, most reliable way to set an accurate location.

> **GRID IS YOUR FIELD FRIEND** — Off in a park or on a summit, you may not know your decimal coordinates, but your grid square is easy to get from a map, another app, or memory — and typing four or six characters is quick and hard to get badly wrong. When automatic location looks off, the grid box is the fix.


## Getting an Accurate Location on a Hotspot

In the field you'll often be online through your **phone's hotspot** — and this is exactly where automatic (geo-IP) location can be badly wrong, sometimes placing you in a different city, because it reflects the cellular network, not your true spot. Two good fixes:

- **Plug in a USB GPS receiver** — it uses satellites, ignores your internet entirely, and is accurate anywhere.
- **Type your grid square** (or exact latitude/longitude) — the simplest fix with no extra hardware.

> **ALWAYS SANITY-CHECK YOUR LOCATION IN THE FIELD** — Before trusting a plan in the field, glance at your location. If it looks wrong, fix it with your grid square — otherwise your predictions, grey-line times, and weather are all for the wrong place.


## Troubleshooting

| Symptom | What to do |
| --- | --- |
| My location is in the wrong city (on a hotspot) | That's geo-IP guessing from your cellular connection. Type your **grid square** and press **Set from grid**, or plug in a USB GPS receiver. |
| My USB GPS receiver isn't being used | Make sure it's plugged in and recognized by your computer before pressing Use my location. When present, it takes priority automatically. |
| The grid square looks wrong | Re-check the coordinates you entered; the grid is computed from them. Conversely, a mistyped grid gives wrong coordinates — retype it and press Set from grid. |
| The program isn't updating my position as I move | By design it only refreshes when you ask. Press **Use my location** (or re-enter your grid) each time you set up somewhere new. |
| I don't know my grid square | Get it from a paper map, another ham app, or by entering your latitude/longitude — the grid then fills in automatically. |


# 15. Navigation and Interface Reference

*A map of the whole program: every tab in the navigation bar, the always-on clock, the light/dark theme switch, and the visual cues that tell you where you are.*

> **QUICK VERSION** — Move around the program using the **navigation bar**. The tab you're on is highlighted in **blue**; the others are gray. A **clock** is always visible showing your **local time above UTC**. A **theme button** cycles the app between automatic, light, and dark. That's the whole interface — pick a tab and go.


## The Navigation Bar

The navigation bar is your main way around Activation Planner. Each button opens one screen. The button for the screen you're currently on is **highlighted in blue** so you always know where you are; the rest are a quieter gray until you hover or click them. Click any button to switch screens instantly — the program keeps your work as you move between tabs.


## Every Tab at a Glance

Here is every destination in the navigation bar and where to read about it in full:

| Tab | What it's for | Chapter |
| --- | --- | --- |
| Quick plan | Instant recommendations for right now, no setup. | Chapter 5 |
| Plan session | The main planning screen — full inputs and results. | Chapter 4 |
| Inventory | Your gear: radios, antennas, power, and the rest. | Chapter 3 |
| Mission | Operation type and your packing checklist. | Chapter 6 |
| Antenna Patterns | Radiation-pattern plots for your antennas. | Chapter 7 |
| Grey Line | Sunrise/sunset windows for long-distance low-band work. | Chapter 8 |
| Trend | Whether conditions are improving or fading. | Chapter 9 |
| Weather | Local forecast and safety alerts. | Chapter 10 |
| Band Plan | Which frequencies your license allows. | Chapter 11 |
| Battery | How long your power will last. | Chapter 12 |
| POTA | Live spots and self-spotting. | Chapter 13 |

> **YOUR TABS MAY DIFFER SLIGHTLY** — The exact set of tabs can grow as the program gains features. If you see a tab not listed here, it was added after this manual edition — its own on-screen hints will describe it, and a later edition will document it fully.


## The Always-On Clock

At the top of the window, a clock is **always visible**, no matter which tab you're on. It shows two times stacked:

- **Your local time**, on top — the time on your wrist.
- **UTC**, below it — Coordinated Universal Time, the worldwide standard time zone that ham radio and the propagation predictions use.

Both update live. Having UTC always in view matters because the planning screen's best-hour and heatmap times are in UTC — so you can match a predicted opening to your own local time at a glance.

> **JARGON, IN PLAIN WORDS** — **UTC (Coordinated Universal Time):** a single time zone the whole world's radio operators share, so everyone means the same instant regardless of where they are. It doesn't change for daylight saving. When a prediction says a band peaks at "18:00 UTC," the clock lets you see what that is in your local time.


## The Light/Dark Theme Switch

A **theme button** near the clock changes the program's color scheme. Pressing it cycles through three settings:

| Setting | What it does |
| --- | --- |
| Auto | Follows your computer's own light-or-dark setting automatically. |
| Light | Forces the light (bright background) theme. |
| Dark | Forces the dark (dim background) theme — easier on the eyes at night and in the field after sunset. |

The button shows which setting is active. Dark mode is especially handy for evening and nighttime operating, when a bright screen ruins your night vision. Your choice is remembered.


## The Look and Feel

You'll notice the program uses **floating cards** — panels that sit slightly raised off the background with soft shadows — a strong blue accent color, and gentle animations when you hover or press buttons. This isn't just decoration: the layering groups related things together and helps your eye find what matters. It's the same clean style throughout, so once you learn one screen, the rest feel familiar.


## Troubleshooting

| Symptom | What to do |
| --- | --- |
| I can't tell which tab I'm on | The active tab is highlighted **blue**; the others are gray. If they all look similar, try the theme switch — one theme may show the contrast more clearly on your screen. |
| The best-hour times don't match my clock | Prediction times are in **UTC**. Use the UTC line of the on-screen clock (or the local line beside it) to convert — the two are shown together for exactly this reason. |
| The screen is too bright/dark | Press the **theme button** near the clock to switch between Auto, Light, and Dark. |
| A screen's content runs off the edge | Every screen scrolls; scroll down (and the content clears the scrollbar). If something still looks clipped, try maximizing the window. |
| A tab I read about isn't there / one I don't recognize is | Tabs can change between versions. Use each tab's on-screen hints; features added after this edition will be documented in a later one. |


# 16. Troubleshooting and Frequently Asked Questions

*The problems people hit most often, with plain fixes, plus straight answers to the questions new users ask about what the program does and doesn't do.*

> **START HERE** — Most trouble comes down to one of three things: the propagation engine (VOACAP) isn't installed (you'll see a "sample data" banner), your location is wrong (common on a phone hotspot), or the program can't reach the internet for live data. Check those three first — they fix the majority of issues.


## Common Problems and Fixes

| Problem | Fix |
| --- | --- |
| An orange "sample data" banner shows on the plan | The propagation engine (VOACAP) isn't set up, so the numbers are placeholders. Follow the Installation Guide to install it; the banner then disappears and predictions become real. |
| My location is wrong (often a different city) | Automatic (geo-IP) location is only a rough guess and is often way off on a cellular hotspot. Type your **grid square** and press **Set from grid**, or plug in a USB GPS receiver (Chapter 14). |
| No live solar / weather / spot data | These need an internet connection (a phone hotspot is fine). Check you're online; for solar you can also type the sunspot number by hand. |
| Every band shows poor reliability | This may be genuine — a very long path in disturbed conditions can be dead. Check the K-index on the plan's Live line (a geomagnetic storm quiets the bands), and try a nearer target or a different hour. |
| Antenna patterns look wrong | Almost always the antenna's measurements. Re-check its dimensions on the Inventory screen (Chapter 3) — the model faithfully draws whatever you entered. |
| A printed checklist came out empty | You ticked the **packed** boxes, not the **🖨 printer** boxes. Only printer-ticked items print (Chapter 6). |
| Content runs off the right edge of a screen | Every screen scrolls; scroll to see it all. Maximizing the window also helps. |
| The setup wizard never came back | It only appears the first time you run the program. Change gear any time on the Inventory screen (Chapter 3). |


## Questions About Predictions and Accuracy

**How accurate are the band recommendations?** They come from VOACAP, a respected propagation-prediction engine, using real solar data and your actual path. Like any forecast, it's a strong guide, not a guarantee — the ionosphere has a mind of its own. Use it to know where to *start*, then trust your ears.

**Why is a band ranked low when I've worked it before?** The ranking is for *your path and the current conditions right now.* A band that's great at one hour or in one part of the solar cycle can be poor at another. Check its 24-hour heatmap for a better hour.

**What is UTC and why are the times in it?** UTC is the single worldwide time zone radio operators share so everyone means the same instant. The on-screen clock shows your local time above UTC so you can convert at a glance (Chapter 15).

**Do I need to understand the solar numbers?** No — the sunspot number is filled in for you and the plan already accounts for it. But Chapter 4 explains every solar figure in plain language if you're curious or want to interpret conditions yourself.


## Questions About Location and the Field

**Does the program track where I am?** No. It checks your location only when you ask (refresh-on-demand) and never tracks you in the background (Chapter 14).

**What's the best way to set location in the field?** A USB GPS receiver is most accurate; otherwise type your grid square. Don't rely on automatic location over a hotspot — it's often wrong (Chapter 14).

**Will it work with no internet?** Core planning works offline once the engines are installed and you set your location by hand (or via a GPS receiver). Live solar data, weather, and POTA spots need a connection.


## Questions About Gear and Antennas

**Do I have to enter my gear perfectly?** No — you can edit everything later on the Inventory screen. But antenna measurements do matter for accurate patterns and matching, so take care there (Chapter 3).

**What if my exact antenna isn't in the model list?** Pick the closest model as a starting point and adjust the numbers, or choose Custom / Home-brew and enter it yourself (Chapters 2 and 3).

**Why is my antenna's pattern marked "Approximate"?** It has coils, traps, or broadband matching that a simple wire model can't capture exactly. The pattern is a good guide to its general behavior, not an exact measurement (Chapter 7).


## Questions About What the Program Does and Doesn't Do

**Does it log my contacts?** No. Activation Planner is a planning and spotting tool; keep using your normal logging program for QSOs (Chapter 13).

**Does it control my radio?** No — it doesn't connect to or operate your transceiver. It plans; you operate.

**Can it spot my friend on POTA?** No — it only ever spots you (self-spot only), by design (Chapter 13).

**Does it keep a history of my past sessions?** No. It's stateless — every plan uses the current time and current conditions, with no saved session history.

**Can I run it on a phone or tablet?** The desktop app runs on Windows, macOS, and Linux (including Windows tablets). A dedicated phone/tablet version is a future project — see the Introduction and the roadmap.


## Where to Look Next

- **Installation problems** — the separate Installation Guide covers setup of the program and the two helper engines.
- **A specific feature** — each feature has its own chapter with its own Troubleshooting table at the end.
- **Unfamiliar terms** — see the Glossary (next chapter).
- **Licensing and credits** — see the Licenses & Credits chapter.


# 17. Glossary — Plain-Language Definitions

*Every term used in this manual and in the program, explained in one or two plain sentences. Radio jargon, propagation words, and the program's own labels are all here.*

> **HOW TO USE THIS** — Hit a word you don't know? Find it here. Terms are grouped by topic and defined in everyday language — no prior knowledge assumed. Each definition points you to the chapter where the term is used in depth.


## Radio and Operating Terms

| Term | Plain-language meaning |
| --- | --- |
| Amateur radio (ham radio) | A licensed hobby of two-way radio communication for personal, experimental, and emergency use. |
| Band | A range of frequencies set aside for amateur use, named by wavelength — e.g. "20 meters," "40 meters." |
| CW | Continuous Wave — Morse code. A very efficient mode that gets through when voice can't. |
| Phone | Voice modes, most commonly SSB (Single Sideband). "Phone" just means talking, as opposed to Morse or digital. |
| Digital mode | Sending data (like FT8) rather than voice or Morse; the computer does the sending and decoding. |
| QRP | Operating at low power — 5 to 10 watts. Popular for lightweight, battery-friendly field operating. |
| QSO | A two-way contact/conversation between two stations. |
| Calling frequency | A frequency where operators of a given mode gather to find each other. |
| License class | Your level of amateur license (Technician, General, or Amateur Extra), which sets what frequencies and modes you may use. |
| Duty cycle | The share of your on-air time actually spent transmitting versus listening — matters for battery life. |


## Propagation and Solar Terms

| Term | Plain-language meaning |
| --- | --- |
| Propagation | How radio signals travel from one place to another — which bands reach where, and when. |
| Ionosphere | A high layer of the atmosphere that bends shortwave signals back to Earth, letting them skip long distances. |
| Reliability (%) | The program's headline score for a band: roughly, the chance a contact will succeed. Higher and greener is better. (Chapter 4) |
| Best hour | The single time of day a band is predicted to be at its best, shown in UTC. (Chapter 4) |
| MUF | Maximum Usable Frequency — the highest frequency the ionosphere is bending back on your path right now. Bands below it tend to work; above it usually don't. (Chapter 4) |
| Sunspot number (SSN) | A count of sunspots, used as a simple measure of solar activity. Higher generally means better high-band conditions. (Chapter 4) |
| Solar flux index (SFI) | The sun's radio energy at 10.7 cm; moves with the sunspot number and tells the same story. Higher favors the high bands. (Chapter 4) |
| K-index | A near-real-time 0–9 gauge of how disturbed Earth's magnetic field is. Low is calm and good; 5+ is a geomagnetic storm that degrades the bands. (Chapter 4) |
| A-index | A once-a-day summary of geomagnetic disturbance, companion to the K-index. Low is good. (Chapter 4) |
| Grey line | The twilight band circling the Earth at sunrise/sunset, when low bands can reach unusually far. (Chapter 8) |
| NVIS | Near Vertical Incidence Skywave — sending signals nearly straight up so they rain back down over a nearby region; used for regional/emergency coverage. (Chapters 4, 6) |
| DX | Distant stations / long-distance contacts. |
| Long path | Reaching a station the long way around the globe instead of the direct route. (Chapter 4) |
| UTC | Coordinated Universal Time — the single worldwide time zone radio operators share. (Chapter 15) |


## Antenna Terms

| Term | Plain-language meaning |
| --- | --- |
| Radiation pattern | A map of an antenna's "loudness" in every direction. (Chapter 7) |
| Take-off angle | The angle above the horizon where an antenna radiates most strongly. Low = long distance; high = nearby/regional. (Chapter 7) |
| Gain | How much an antenna concentrates the signal in its best direction, versus a reference antenna. Measured in dBi. (Chapter 7) |
| Front-to-back | For a directional antenna, how much stronger it is forward than backward. (Chapter 7) |
| Azimuth plot | The top-down view of a pattern — which compass directions the signal favors. (Chapter 7) |
| Elevation plot | The side-on view of a pattern — at what angle above the horizon the signal leaves. (Chapter 7) |
| Dipole | A basic antenna: a wire fed in the center. Its height sets its take-off angle. (Chapter 3) |
| Vertical | An antenna that stands upright, often with radials; radiates low and all around — good for distance. (Chapter 3) |
| EFHW / end-fed | An end-fed wire antenna (End-Fed Half-Wave and similar), fed at one end rather than the middle. (Chapter 3) |
| Radials | Wires spread under a vertical that act as its electrical "ground." How many, how long, and how high (on the ground vs elevated a few feet) all affect performance. (Chapter 3) |
| Feed point | Where the coax cable connects to the antenna (center, end, off-center, or base). A required detail for modeling. (Chapter 3) |
| Measured vs Approximate | A flag on each antenna's pattern: Measured = trust it; Approximate = a good guide for loaded/broadband antennas a simple model can't capture exactly. (Chapter 7) |
| Wavelength (λ) | The physical length of one radio wave; antenna dimensions are often described as fractions of it (e.g. quarter-wave). |


## Program and Computer Terms

| Term | Plain-language meaning |
| --- | --- |
| VOACAP | The propagation-prediction engine the program runs behind the scenes to rank bands. (Chapters 2, 4) |
| NEC2++ | The antenna-modeling engine the program runs to compute radiation patterns. (Chapters 2, 7) |
| Setup wizard | The one-time, step-by-step screen that collects your gear the first time you run the program. (Chapter 2) |
| Inventory | Your saved list of owned gear, editable any time. (Chapter 3) |
| Mission type | The kind of operation you're planning (POTA, SOTA, Field Day, EMCOMM, General), which tailors gear and framing. (Chapter 6) |
| Quick Mode / Quick plan | The fast path that gives full recommendations with no setup. (Chapter 5) |
| Stateless / replanning | The program keeps no session history; each plan uses the current time and conditions. (Chapters 4, 9) |
| Grid square (Maidenhead) | A short code (like EM29) naming your location on the map; the program converts it to/from latitude and longitude. (Chapter 14) |
| Geo-IP | Guessing your location from your internet connection — convenient but only rough, and often wrong on a hotspot. (Chapter 14) |
| NMEA / GPS receiver | A device (often USB) that reports your exact position from satellites; the most accurate location source. (Chapter 14) |
| PDF | Portable Document Format — the file type the program exports plans and checklists to for printing or carrying. (Chapters 4, 6) |
| Amp-hour (Ah) | A measure of battery capacity — roughly, amps × hours it can supply. (Chapter 12) |
| Theme (light/dark) | The program's color scheme, switchable between Auto, Light, and Dark. (Chapter 15) |


# 18. Licenses and Credits

*The people and projects that make Activation Planner possible, and the license notices for the two helper programs it includes — reproduced here in full, as their licenses require.*

Activation Planner stands on the shoulders of two respected, freely-available engineering tools — **VOACAP** for propagation prediction and **NEC2++** for antenna modeling — plus public-domain map data. As explained in Chapter 2, the program **runs these as separate helper programs** rather than copying their code into itself. That keeps their results trustworthy and unmodified, and it honors their software licenses correctly. Those licenses require that we reproduce certain notices wherever the program is documented, so they appear in full below.

> **WHY THESE NOTICES ARE HERE** — Including someone else's licensed program the right way means passing along their license notice. These notices are not about Activation Planner's own terms — they credit and preserve the rights of the VOACAP and NEC2++ authors, exactly as those projects ask. You don't need to do anything with them; they're here for transparency and to meet the license terms.


## VOACAP — the Propagation Engine

The band recommendations are computed by **VOACAP** (Voice of America Coverage Analysis Program), specifically the `voacapl` port by J.A. Watson. The core VOACAP software was developed by a United States Government agency — the National Telecommunications and Information Administration's Institute for Telecommunication Sciences (NTIA/ITS) — and is not subject to U.S. copyright. J.A. Watson's port changes are dedicated to the public domain. Its required disclaimer, reproduced verbatim:

> **NTIA/ITS DISCLAIMER (VERBATIM)** — The software contained within was developed by an agency of the U.S. Government. NTIA/ITS has no objection to the use of this software for any purpose since it is not subject to copyright protection in the U.S. No warranty, expressed or implied, is made by NTIA/ITS or the U.S. Government as to the accuracy, suitability and functioning of the program and related material, nor shall the fact of distribution constitute any endorsement by the U.S. Government.

VOACAP source (voacapl port): https://github.com/jawatson/voacapl — VOACAP (NTIA/ITS): https://its.ntia.gov/


## NEC2++ — the Antenna Engine

The antenna radiation patterns are computed by **NEC2++** (the `necpp` implementation by Tim Molteno and contributors), a modern version of the Numerical Electromagnetics Code. NEC2++ is licensed under the **GNU General Public License, version 2 (GPLv2)**. Because Activation Planner runs it as a separate program and never builds its code in-process, including it is permitted and does not change Activation Planner's own terms. As GPLv2 requires, the full license text is included with the installation (in the `licenses/nec2++` folder), along with this offer of the corresponding source code:

> **NEC2++ CORRESPONDING-SOURCE OFFER** — This product includes NEC2++ (necpp), licensed under the GNU General Public License, version 2. The complete corresponding source code for the bundled NEC2++ binary is available at https://github.com/tmolteno/necpp . A copy is also included in the "licenses/nec2++" folder of this installation. You may also obtain the source from the project contact for a period of three years from the date of distribution.

With thanks to the necpp authors. NEC2++ source and GPLv2 text: https://github.com/tmolteno/necpp


## Map Data

The world map used on the grey-line view is drawn using **Natural Earth** public-domain map data. Natural Earth data is free to use with no permission or attribution required; we credit it here as a courtesy. Source: https://www.naturalearthdata.com/


## Thanks

Activation Planner is built for the amateur radio community — the POTA and SOTA activators, Field Day teams, and emergency communicators who take radio into the field. Thanks to the propagation and antenna-modeling projects above, to the amateur radio operators whose shared knowledge shaped this program, and to everyone who tests it and suggests improvements. If you find a problem or have an idea, that feedback is what makes the next version better.

> **NOT LEGAL ADVICE** — The licensing summaries here are provided in good faith for transparency. They are not legal advice. The authoritative license texts are those distributed with the respective tools and at the links above.
