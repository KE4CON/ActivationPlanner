# Activation Planner — Roadmap

Post-v1.0 ideas, recorded so they aren't lost. Nothing here is in v1.0 scope. The
authoritative v1.0 scope and the "Future / not in scope now" list live in
[`CLAUDE.md`](../CLAUDE.md); this file expands on the larger future efforts.

---

## Mobile / tablet companion app (iOS & Android) — future, real effort

**Interest:** strong likely demand for a phone version; tablets too.

**What runs where today:**
- **Windows, macOS, Linux — desktop and laptop:** the current Avalonia app runs as-is.
- **Windows tablets (e.g. Surface):** run the desktop app as-is (it's just Windows).
- **iPad / Android tablets and phones:** not supported yet — see the blocker below.

**The core blocker — why it's not a simple recompile.** The whole planner is built on
**shelling out to VOACAP and NEC2++ as external programs** via `Process.Start`
(ProcessEngine, Layer 1). **iOS and Android do not permit an app to launch separate
native executables**, and we deliberately cannot link NEC2++ in-process instead —
its GPLv2 license is the reason we keep it as a separate shelled-out program (see
[`THIRD_PARTY_LICENSES.md`](THIRD_PARTY_LICENSES.md) and the licensing rules in CLAUDE.md).
So the propagation/antenna engines cannot run the same way on a phone or non-Windows tablet.

**The clean path when we do it:**
1. **Thin client + backend engine.** A touch-first Avalonia (or native) app that talks over
   **HTTPS to a small backend service** which runs VOACAP/NEC2++ and returns results. The
   phone/tablet is the UI; the heavy math lives on a server (cloud, or the operator's own
   home PC). This *preserves* the GPL separation cleanly — the shell-out stays server-side.
2. **Touch-first UI rework.** Today's layout uses wide side panels and multi-column grids
   built for a large display; small screens need a redesigned, single-column, touch layout.
3. **Reuse the portable core.** Most C# (PropagationModel domain types, Services logic,
   viewmodels) is portable and would carry over; ProcessEngine is the part that moves to the
   backend.

**Rough shape:** a genuine v2-class effort (backend service + mobile UI), not a checkbox.
Sequencing note: this pairs naturally with any other feature that wants a server component.

---

## Already recorded in CLAUDE.md (Version Scope) — summarized here for one place

- **Multi-park / route planning (v2.0)** — sequencing band/antenna plans across several stops
  in one day with inter-stop timing; needs a route-based planning unit, not an extension of the
  single-session model.
- **3D far-field antenna surface view** — deferred until an Avalonia-12-compatible 3D engine
  exists (the intended `Ab4d.SharpEngine.AvaloniaUI` is Avalonia-11 only). The v1.0 2D polar
  plots render the same data.

_(add more here as they come up)_
