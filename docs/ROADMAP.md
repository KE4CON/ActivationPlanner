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
- **3D far-field antenna surface view — DONE (shipped in v1.0, 2026-08).** Formerly deferred
  because `Ab4d.SharpEngine.AvaloniaUI` was Avalonia-11-only. ab4d shipped Avalonia-12 support;
  the spike validated it builds + renders on Avalonia 12 (Vulkan), and it was promoted to a full
  feature: a 2D/3D toggle on the Antenna Patterns tab, a rotatable NEC-driven far-field surface
  (gain grid → colored/wireframed surface, ground plane + compass + zenith/horizon labels,
  take-off line), with a graceful fall-back to the 2D polar plot when no Vulkan GPU is present.
  Licensed under SharpEngine's **free open-source license** (tied to the public repo + the
  `ActivationPlanner.UI` assembly); added to the approved-NuGet list in CLAUDE.md.

- **Raspberry Pi / ARM support (advise-on-request).** The app (Avalonia + .NET) runs on Raspberry Pi
  OS desktop, so a Pi-in-a-laptop/netbook config (pi-top, CrowPi, DIY) can run it. The only extra work
  is **ARM builds of the helper programs**: VOACAP (voacapl) and NEC2++ compile from source on the Pi,
  and the app needs the .NET ARM runtime (or a self-contained ARM build). Not officially packaged —
  the audience is small — but if someone asks, advise: Pi 4 (4 GB+)/Pi 5, Pi OS desktop, and build the
  two helpers for ARM. Could be turned into prebuilt ARM binaries later if demand appears.

_(add more here as they come up)_
