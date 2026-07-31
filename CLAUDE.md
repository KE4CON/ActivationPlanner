# Activation Planner — Project Context

## Project Identity
Name: Activation Planner
Author: Jim, KE4CON
Language: C# (.NET 10)
UI Framework: Avalonia 12 (cross-platform — Windows, macOS, Linux)
Purpose: Pre-operation (and re-invokable) individual planning tool for ham radio operating sessions — POTA, SOTA, Field Day, EMCOMM, or general operating. Recommends bands, matches antennas from owned inventory, and builds packing checklists, grounded in real VOACAP propagation predictions rather than guesswork.

## Related Programs — Do Not Merge
- **IcomRigControl** — separate program, in-the-moment rig control/logging. Shares tech stack and architecture pattern with this project. No shared code.
- **FieldCommand IMS** — separate incident-management platform (Raspberry Pi/ICS-NIMS). No integration with this project.
- Activation Planner is scoped to **individual pre-operation planning only**. It does not do QSO logging (stays in IcomRigControl) and does not do incident/team/resource management (stays in FieldCommand IMS).

## Architecture Layers (never mix concerns across layers)
Layer 1 — **ProcessEngine**: Raw external-process I/O. Shells out to VOACAP and NEC2++ via `Process.Start`, writes input decks/geometry files, reads raw text output. No domain knowledge, no UI. All process access goes through an `IProcessTransport` interface to allow mocking in tests.

Layer 2 — **PropagationModel**: Clean C# domain classes (`BandPrediction`, `AntennaProfile`, `CircuitQuery`, `GearItem`) exposing ProcessEngine's raw output as real objects/events. Consumes ProcessEngine only.

Layer 3 — **Services**: Multiple independent peer services, each consuming PropagationModel only:
- `ChecklistService` — Template vs. Instance checklist logic
- `MissionTypeService` — mission type selection (POTA/SOTA/Field Day/EMCOMM/general); drives gear suggestions and propagation framing (EMCOMM → NVIS/regional CIRCUIT setup — changes the *question* asked of VOACAP, never the physics-based answer)
- `GearInventoryService` — owned gear, antenna category-mapping, Option A/B trigger logic
- `PotaService` — split internally into (a) read-only spot/park data client, and (b) self-spotting. **Neither requires authentication** — POTA's spot endpoints (read and post) are plain unauthenticated HTTP calls; only *log upload for award credit* (a separate, out-of-scope operation) uses Cognito auth. Confirmed endpoints:
  ```
  GET  https://api.pota.app/spot/activator
  GET  https://api.pota.app/spot/comments/{act}/{park}
  GET  https://api.pota.app/stats/user/{call}
  GET  https://api.pota.app/park/{park}
  GET  https://api.pota.app/location/parks/{loc}
  GET  https://api.pota.app/programs/locations/
  POST https://api.pota.app/spot/   (self-spot = spotter == activator)
  ```

Layer 4 — **UI**: Avalonia views and viewmodels. Consumes Services and PropagationModel only.

## Coding Standards
- C# 12 features, .NET 10 target
- Nullable reference types enabled
- async/await for all I/O (process shell-outs, POTA HTTP calls) — no blocking calls on the UI thread
- CancellationToken passed through all async paths
- Records for immutable data (`BandPrediction`, `AntennaProfile`, `CircuitQuery`, `GearItem`, `MissionTemplate`)
- No magic numbers — VOACAP card format constants and NEC2++ geometry constants centralized in dedicated constants files
- MVVM: **CommunityToolkit.Mvvm** (`ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`) — not ReactiveUI
- Unit tests required for: VOACAP input deck formatting (fixed-column, bug-prone), VOACAP output parsing, Option A/B antenna trigger logic (height/length-to-wavelength math), the dipole empirical comparison harness, POTA auth flow

## Solution Structure
Separate `.csproj` per layer, compiler-enforced boundaries via `ProjectReference`:
- `ActivationPlanner.ProcessEngine` (+ `.Tests`)
- `ActivationPlanner.PropagationModel` (+ `.Tests`)
- `ActivationPlanner.Services` (+ `.Tests`)
- `ActivationPlanner.UI`

## Approved NuGet Packages (list before adding — see "What NOT to Do")
- **Avalonia** 12.x (UI) — `Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent`, `Avalonia.Fonts.Inter`; `Avalonia.Diagnostics` (Debug only)
- **CommunityToolkit.Mvvm** 8.x (UI MVVM — `ObservableObject`/`[ObservableProperty]`/`[RelayCommand]`)
- **xUnit** + `xunit.runner.visualstudio` + `Microsoft.NET.Test.Sdk` (test projects only)
- **QuestPDF** 2026.x (Services — PDF plan export). Community license (free for small orgs/individuals), set once at startup via `QuestPDF.Settings.License = LicenseType.Community`.
- **System.IO.Ports** 10.x (Services — external NMEA GPS receiver over USB/serial, Item #18; cross-platform incl. Linux/Raspberry Pi).
- Persistence uses framework `System.Text.Json`; network I/O (POTA, geo-IP) uses framework `System.Net.Http.HttpClient` — no packages needed.
- **Future (not now):** `Ab4d.SharpEngine.AvaloniaUI` for the 3D far-field view — Avalonia-11-only today; revisit when an Avalonia-12 build exists.

## Feature Priorities (build in this order)
Phase 1: Gear inventory — guided/required first-use setup wizard (step-by-step, Back/Next, progress indicator, skippable categories, Finish summary). Antenna entry uses a sub-list-and-detail pattern (add one antenna at a time to a running list, not a flat form). Data persists and is fully editable afterward via a separate non-wizard screen.
Phase 2: VOACAP shell-out (ProcessEngine + PropagationModel) — input deck writer, `Process.Start` invocation, output parser
Phase 3: Antenna category-mapping + Option A/B trigger logic (community library vs. NEC2++ custom modeling)
Phase 4: Avalonia UI — core planning screen (band/antenna recommendations, combined list + chart/graphical view)
Phase 5: Checklist/Template engine + Mission Type selection
Phase 6: GPS/location integration (refresh-on-demand, not continuous tracking)
Phase 7: POTA integration — read-only spots/park data and self-spotting (both are plain unauthenticated HTTP calls; no login flow needed) — but **do not ship self-spotting without direct confirmation from POTA** that third-party automated use is acceptable
Phase 8: NEC2++ shell-out (Option B custom antenna modeling)

## Key Domain Rules

**Antenna Option A/B trigger logic:**
- Verticals: if actual height falls within 0.25λ–1.25λ at the band being evaluated (documented distortion zone), trigger Option B regardless of library match.
- Dipoles: if actual height differs from the library file's assumed height by more than ~0.05λ at that band, trigger Option B. **This threshold is provisional** — validate empirically against real VOACAP output once the shell-out exists, and tune before treating it as final.
- Length gets the same wavelength-relative treatment as height (an EFHW's electrical length varies by design band even though the category name is the same).
- Ground conductivity is auto-looked-up from FCC data by location (not operator-entered), mapped to the nearest standard VOACAP ground preset (good/average/poor).
- Radial count/length for verticals: operator-entered.
- Feed point type (center-fed, end-fed/EFHW, end-fed random wire, off-center-fed, base-fed) is a required gear field — needed for accurate modeling and to distinguish antennas that share a category name.

**Gear/checklist suggestion logic:** always owned-inventory-first, unowned-items-secondary. Never mix "you own this" and "you should acquire this" into one undifferentiated list.

**Replanning:** stateless — no session/activation history tracked. Each replan uses current time + current solar data. Manual per-query band-exclude toggle available as a non-persisted filter.

**Location:** refresh-on-demand GPS only. No continuous/background tracking.

## What NOT to Do
- Do not implement features out of phase order without explicit instruction
- Do not add NuGet packages without listing them here first
- Do not put VOACAP/NEC2++ shell-out logic in ViewModels or Services
- Do not put UI code in ProcessEngine or PropagationModel
- Do not use `Thread.Sleep` — use `Task.Delay` with `CancellationToken`
- Do not swallow exceptions silently
- Shell out to VOACAP and NEC2++ via `Process.Start` — never link or embed them in-process (this is what keeps GPLv2 NEC2++ from reaching the planner's own AGPLv3/GPLv3 code). Do not modify either tool's source. Redistribution/bundling **is** allowed (Item #19) — follow the obligations in "Third-Party Tool Licensing & Redistribution" below **to the letter**.
- Do not ship self-spotting before POTA has been contacted directly for confirmation (their sanctioning of third-party automated use is still unconfirmed, even though the endpoint itself is technically open)

## Third-Party Tool Licensing & Redistribution (Item #19 — follow to the letter)
The installer bundles VOACAP and NEC2++. Obligations below were verified against the actual license files; full detail and quoted texts live in `docs/THIRD_PARTY_LICENSES.md`. **Not legal advice — a license review before public/commercial distribution is recommended.**
- **VOACAP** (voacapl core): US-Government work, not subject to U.S. copyright ("NTIA/ITS has no objection to the use of this software for any purpose"); J.A. Watson's port modifications are CC0. Redistribution and commercial use are permitted. **Obligation: include the NTIA/ITS disclaimer text.** Do **not** bundle voacapl's two GPLv3 utility files (`dst2csv.f90`, `dst2ascii.f90`) — unused data-conversion tools; excluding them avoids GPLv3 entirely.
- **NEC2++** (necpp): **GPLv2.** Redistribution is permitted as a **separate, shelled-out program** (aggregation — does not affect the planner's own license because it is never linked in-process). **Obligations:** (1) include the **GPLv2 license text**; (2) provide the **corresponding source or a written offer** (necpp is public on GitHub — ship a source copy or a clear offer + link); (3) attribution.
- **These notices must appear in ALL of:** the project **README**, **every piece of project documentation we produce**, and **a license-notices screen shown during installation** (plus a notices folder in the install).
- **Ship-time checklist (do not skip):** ☐ NTIA disclaimer bundled & shown ☐ voacapl GPLv3 utility files excluded ☐ NEC2++ GPLv2 text bundled & shown ☐ NEC2++ corresponding-source offer bundled ☐ notices displayed by the installer ☐ notices in README + all docs.

## Version Scope
**v1.0** covers everything in this document. **Future / not in scope now:**
- **Multi-park/route planning (v2.0)** — sequencing band/antenna plans across multiple stops in one day, with inter-stop timing. Deliberately deferred until v1.0's single-session planning model is built and proven; it requires a different planning unit (a route, not a single session) rather than an extension of the current data model.
- **3D far-field antenna surface view** — deferred because the intended engine (`Ab4d.SharpEngine.AvaloniaUI`) is Avalonia-11-only and we are staying on Avalonia 12. The v1.0 2D polar plots (Item #17) render the same data; revisit 3D when an Avalonia-12-compatible engine is available. (2D is in scope for v1.0.)

## Additional v1.0 Features (beyond core planning)
- **Export plan as PDF** — operator-selectable content (bands/antenna/checklist, any combination), triggered via a dedicated Export button
- **Propagation trend view** — rolling few-hour window, automatic background VOACAP sampling every ~15–30 min, session-local only (no persistence, consistent with the stateless-replanning rule)
- **Grey-line indicator** — real sunrise/sunset from lat/lon+date; highlights when a band VOACAP already ranks well coincides with the grey-line window — never a separate ranking boost, to avoid double-counting an effect VOACAP's own model likely already reflects. **Presented as its own dedicated tab** (revised — the subtle chart overlay proved too hard to see; see the "Added Mid-Build" note and Decisions Log Item #13 revision), **not** an overlay on the heatmap.
- **Quick Mode** — fast-access entry point reusing the same replanning logic (Item #4), skips setup screens and lands directly on the full (not truncated) band/antenna recommendation view


## Additional v1 Features — Added Mid-Build
- **Local + UTC time/date display** — persistent header/status bar, local above UTC, both live
- **Antenna far-field pattern plots (2D — v1.0)** — 2D polar cuts (azimuth/elevation) via Avalonia's native Skia rendering, no extra dependency; data sourced from existing NEC2++/type-13 antenna data (no new data collection). Own dedicated tab. **3D surface view is deferred to the future-add category** — the intended engine (`Ab4d.SharpEngine.AvaloniaUI`) is Avalonia-11-only, and we are staying on Avalonia 12; revisit when it ships an Avalonia-12 build.
- **GPS priority:** external hardware GPS receiver (USB/serial NMEA) takes priority when connected, on either desktop or laptop machines; falls back to geo-IP otherwise. Not OS-level Wi-Fi-based location services.
- **Installer bundles VOACAP and NEC2++ directly** — both are redistributable per their respective terms (see Item #3 in Decisions Log for VOACAP; NEC2++ is GPLv2). Include a license notices screen/folder satisfying both.
- **Grey-line indicator is its own dedicated tab**, not a subtle chart overlay — same underlying correlation-highlight logic from Item #13, given clearer visual presentation.
- **Visual design direction:** flat surfaces with intentional depth — soft drop shadows, panel elevation/layering, a strong accent color, real hover/press micro-animations. Avoid literal skeuomorphic 3D/bevelled buttons (dated style).


## Reference
Full planning history and reasoning: `Activation_Planner_Decisions_Log.md` / `.pdf`, `Activation_Planner_Kickoff_Memo.pdf`, `Activation_Planner_Reference.pdf`.
