# Bug-Hunting Playbook — Activation Planner

**Read this at the start of a debugging / hardening session and follow it.** The goal is to drive the
bug count toward zero *systematically*, not reactively. Getting the propagation math right is only
**one pillar** — most bugs live in the layers around it (UI, services, integrations, persistence).

For an operating-planning tool the insidious failure mode is **wrong-but-confident output**: a band
ranked well that shouldn't be, an antenna matched to the wrong model, a checklist missing gear. Those
don't crash — they quietly mislead an operator in the field. This playbook targets exactly that.

---

## The one rule that makes everything stick

> **Every bug found — from any method below — gets a regression test before you move on.**

A fix without a test can silently regress. A fix *with* a test stays dead. No exceptions. When a test
you wrote to reproduce a bug *passes* unexpectedly, the bug is elsewhere — keep the test and keep looking.

---

## The mental model: pillars, not a single dial

Bugs cluster by layer. The compiler-enforced layer boundaries help, but each layer fails differently.

| Pillar | What it covers | Primary method |
|---|---|---|
| **ProcessEngine** | VOACAP / NEC2++ shell-out I/O; fixed-column input-deck & geometry writing | oracle diff vs real VOACAP/NEC2++; deck-format tests |
| **PropagationModel** | parsing raw tool output into `BandPrediction`/`AntennaProfile`/etc. | oracle diff + fuzz on real/garbled output |
| **Antenna Option A/B logic** | height/length-to-wavelength math, distortion-zone triggers, provisional dipole threshold | exhaustive boundary tests + empirical validation |
| **Services** | Checklist, MissionType, GearInventory, POTA (spots/self-spot) | unit + scenario tests |
| **Integrations / I/O** | POTA HTTP, geo-IP, external NMEA GPS (serial), PDF export | real-endpoint + fault injection |
| **UI** | Avalonia 12 views + CommunityToolkit.Mvvm viewmodels (wizard, editors, plots) | exploratory use + headless VM tests |
| **Persistence** | `System.Text.Json` gear/checklist/settings | corruption + round-trip tests |
| **Correctness-critical** | that a prediction/recommendation is *right* (bad advice = bad ops decision) | oracle diff + property tests |
| **Licensing (Item #19)** | VOACAP/NEC2++ notices bundled & shown; GPLv3 utils excluded | ship-time checklist |

---

## The method pipeline (ordered by bug-yield per unit of effort)

Work top-down. Tier 1 is cheap and runs forever after; don't skip it to chase a manual bug.

### Tier 1 — Cheap, automated, catches whole *classes*
1. **Static analysis.** Turn on `EnableNETAnalyzers` + `AnalysisMode=Recommended` in
   `Directory.Build.props`; triage; move toward warnings-as-errors. Catches null-derefs, undisposed
   resources, bad async (all I/O here is `async`/`CancellationToken`) across every project for free.
2. **Work the known-latent backlog.** `docs/Activation_Planner_Decisions_Log.md` records decisions and
   caveats (e.g. the **provisional dipole Option-B threshold to validate empirically**) — mine it for
   things flagged as "verify/tune later" and close them.
3. **Coverage → attack the gaps.** Uncovered lines (VOACAP error paths, tool-missing/timeout handling,
   POTA HTTP failures) are where bugs hide. Add tests there.

### Tier 2 — Prove correctness with an oracle / randomness
4. **Oracle diff — VOACAP & NEC2++ are the ground truth.** The app's whole premise is real VOACAP.
   Feed known circuits/inputs through the app *and* through real `voacapl` (and NEC2++ for antennas),
   then diff the parsed/framed results field-by-field. This is *the* correctness proof for this app.
   The **dipole empirical comparison harness** (per CLAUDE.md) is exactly this — build it out.
5. **Deck-format tests + fuzz.** VOACAP input decks are **fixed-column and bug-prone**: assert exact
   column layout, and fuzz the output parser with real + garbled VOACAP/NEC2++ text (assert no crash,
   no hang). Mock via `IProcessTransport` so this needs no installed tools.
6. **Round-trips.** Gear inventory / checklist JSON: write → read → assert equal. PDF export content
   selection: selected sections in → present in output.

### Tier 3 — Hard-to-trigger, high-consequence
7. **Correctness-critical property tests.** Option A/B triggers at every boundary (0.25λ–1.25λ vertical
   distortion zone; the ±0.05λ dipole-height threshold; length-relative treatment per band). Ground
   conductivity auto-lookup mapping to the right VOACAP preset. Grey-line correlation not double-counting.
8. **External-tool failure handling.** VOACAP/NEC2++ missing, timing out, returning garbage, or wrong
   version — every path should degrade cleanly, never hang or crash. GPS: no fix, unplugged mid-read.
9. **Soak / long-run.** Propagation trend view samples VOACAP every ~15–30 min — run it for hours;
   watch memory, process handles (are shelled-out processes always reaped?), CPU.

### Tier 4 — Human + real world
10. **Systematic click-through:** the first-use wizard (Back/Next/skip/Finish), gear editing, each plan
    screen, the 2D polar plots, grey-line tab, Quick Mode, PDF export — once each, on a checklist.
11. **Field use / beta:** real operators, real locations, real gear inventories. Convert every finding
    to a test.

---

## Per-session workflow

1. **Pick a lane** — one pillar or one tier per session. State it at the top.
2. **Baseline green** — run the suite; confirm it passes before changing anything.
3. **Hunt.** On finding a bug: failing test (red) → smallest fix (green) → suite still green → commit
   on a branch naming the bug + root cause.
4. **Log residuals** you won't fix now (tracked, with detail; pin current behavior with a test).
5. **Update status docs** (the decisions log) so they stay truthful.

---

## Progress tracker & session log — DO NOT re-run the whole file each session

Consult this first, skip what's done, pick **one lane**. Kinds: **one-time foundations** (do once, then
automatic), **standing/automatic** (every build), **continuous lanes** (never done; one per session).

### One-time foundations (check off as completed)
- [ ] Real VOACAP (`voacapl`) + NEC2++ installed locally as the oracle for integration diffing
- [ ] Static analysis enabled (`EnableNETAnalyzers` + `AnalysisMode=Recommended`) and triaged
- [ ] Decisions-Log "verify/tune later" items worked (esp. the provisional dipole Option-B threshold)
- [ ] VOACAP/NEC2++ oracle corpus diff run (app output vs real tool output)
- [ ] Fixed-column input-deck format tests complete (exact column assertions)
- [ ] Option A/B trigger boundary tests exhaustive (both verticals and dipoles)
- [ ] External-tool failure-path pass (missing/timeout/garbage/wrong-version, GPS unplugged)
- [ ] Soak test run (trend-view sampling for hours; process-handle leak check)

### Standing / automatic (keep green — not a session task)
Full suite on every change · output-parser fuzz harness · analyzers (once on) · coverage watched.

### Session log (append ONE line per session; newest last)
Format: `YYYY-MM-DD — lane: <pillar/method> — found/fixed: <summary> — branch/sha`
- _(none yet — first hardening session appends here)_

---

## Project hooks

- **Reference oracle:** real **`voacapl`** (VOACAP) and **NEC2++** output — the ground truth the app is
  built on. Mock the *transport* (`IProcessTransport`) for unit tests; use the real binaries for the
  correctness diff. (These are also bundled with the installer — Item #19.)
- **Run tests:** `dotnet test` across `ActivationPlanner.ProcessEngine.Tests`,
  `ActivationPlanner.PropagationModel.Tests`, `ActivationPlanner.Services.Tests`.
- **Status / decisions:** `docs/Activation_Planner_Decisions_Log.md`; antenna details in
  `docs/ANTENNA_MODELING_GUIDE.md`.
- **Licensing obligations:** `docs/THIRD_PARTY_LICENSES.md` + the Item #19 ship-time checklist in
  `CLAUDE.md` (verify at release, not during feature work).
- **Environment gotchas:** the Avalonia app locks its DLLs while running — close it before
  `dotnet build`/`test` of the UI project. Integration (non-mocked) tests need VOACAP/NEC2++ installed;
  keep them separable from the mocked unit tests so CI without the tools stays green.

---

## What "done" honestly means

There is no "all bugs fixed." The realistic target: **every pillar has an active method pointed at it,
every fix has a test, and known gaps are tracked, not hidden.** For a planning tool, *trustworthy
output* is the whole point — a confident wrong band ranking is worse than a crash.
