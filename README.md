# ActivationPlanner
Event Activation Planner — a pre-operation planning tool for ham radio operating sessions
(POTA, SOTA, Field Day, EMCOMM, or general operating). Recommends bands, matches antennas from
owned inventory, and builds packing checklists, grounded in real VOACAP propagation predictions.

Cross-platform (Windows, macOS, Linux, Raspberry Pi) — built with C# / .NET 10 and Avalonia.

## Licenses & Third-Party Software

Activation Planner's own code is licensed under AGPLv3 / GPLv3.

It runs two external tools as **separate processes** (never linked in-process) and **bundles them
in the installer**:

- **VOACAP** — a U.S. Government (NTIA/ITS) work, not subject to U.S. copyright, with port
  modifications under CC0. Redistribution is permitted; the required **NTIA/ITS disclaimer** is
  included with the distribution.
- **NEC2++ (necpp)** — licensed under the **GNU GPL v2**. Redistributed as a separate, shelled-out
  program; its GPLv2 license text and a corresponding-source offer are included.

Full details, the verbatim disclaimer, and the compliance checklist are in
[docs/THIRD_PARTY_LICENSES.md](docs/THIRD_PARTY_LICENSES.md). These notices are also shown during
installation.
