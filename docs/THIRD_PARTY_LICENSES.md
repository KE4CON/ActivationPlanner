# Third-Party Tool Licensing & Redistribution

Activation Planner shells out to two external programs and **bundles them in the installer**
(Decisions Log Item #19). This document records the exact licensing basis and the obligations we
must satisfy. It is the authoritative source for the license notices that appear in the README and
in the installer's license-notices screen.

> **Not legal advice.** These findings were verified against the tools' actual license files
> (linked below). A formal license review is recommended before public or commercial distribution,
> primarily to confirm the NEC2++ (GPLv2) corresponding-source offer.

---

## How the app uses these tools (why bundling is clean)

Activation Planner runs VOACAP and NEC2++ **as separate processes** via `Process.Start` and
exchanges plain text files with them. It never links, embeds, or statically/dynamically includes
their code in-process. This "mere aggregation" of independent programs is what keeps GPLv2 NEC2++
from imposing its copyleft on Activation Planner's own AGPLv3/GPLv3 code. **This must not change** —
if either tool were ever linked in-process, that conclusion would flip.

---

## VOACAP

**Source used:** the `voacapl` port (https://github.com/jawatson/voacapl), whose `LICENSE` states:

- The **core VOACAP** software was developed by a U.S. Government agency (NTIA/ITS) and is **not
  subject to copyright protection in the U.S.**; NTIA/ITS "has no objection to the use of this
  software for any purpose." → effectively **public domain**, redistribution and commercial use
  permitted.
- J.A. Watson's modifications to the VOACAP source are released under **CC0** (public-domain
  dedication).

**Obligation:** include the NTIA/ITS disclaimer, reproduced verbatim below.

**Do not bundle** voacapl's two utility files `dst2csv.f90` and `dst2ascii.f90` — they are
**GPLv3** data-conversion tools the app does not use. Excluding them avoids GPLv3 obligations
entirely.

### NTIA/ITS disclaimer (include verbatim)

```
Disclaimer:

The software contained within was developed by an agency of the
U.S. Government. NTIA/ITS has no objection to the use of this
software for any purpose since it is not subject to copyright
protection in the U.S.

No warranty, expressed or implied, is made by NTIA/ITS or the
U.S. Government as to the accuracy, suitability and functioning
of the program and related material, nor shall the fact of
distribution constitute any endorsement by the U.S. Government.
```

---

## NEC2++

**Source used:** the `necpp` implementation (https://github.com/tmolteno/necpp), licensed under the
**GNU General Public License, version 2 (GPLv2)** (`COPYING`).

Redistribution is permitted **as a separate, shelled-out program** (see aggregation note above), so
it does not affect Activation Planner's own license. GPLv2 obligations we must meet when bundling
the nec2++ binary:

1. **Include the full GPLv2 license text** (`COPYING`) with the distribution.
2. **Provide the corresponding source, or a written offer to supply it.** necpp's source is public
   on GitHub; ship a source copy in the install, or include the written offer below plus the link.
3. **Attribution** to the necpp authors.

### NEC2++ corresponding-source offer (template)

```
This product includes NEC2++ (necpp), licensed under the GNU General Public License, version 2.
The complete corresponding source code for the bundled NEC2++ binary is available at
https://github.com/tmolteno/necpp . A copy is also included in the "licenses/nec2++" folder of
this installation. You may also obtain the source from <project contact> for a period of three
years from the date of distribution.
```

---

## Where these notices must appear

- **README** — a Licenses / Third-Party section summarizing both and linking here.
- **All project documentation** we produce.
- **The installer** — a license-notices screen displayed during installation, plus a `licenses/`
  folder containing the VOACAP disclaimer, the GPLv2 `COPYING`, and the NEC2++ source (or offer).

## Ship-time checklist

- [ ] NTIA/ITS disclaimer bundled and shown in the installer
- [ ] voacapl GPLv3 utility files (`dst2csv.f90`, `dst2ascii.f90`) excluded
- [ ] NEC2++ GPLv2 `COPYING` bundled and shown
- [ ] NEC2++ corresponding source (or written offer) bundled
- [ ] Notices displayed by the installer and present in README + all docs

## References
- voacapl LICENSE: https://github.com/jawatson/voacapl/blob/master/LICENSE
- necpp COPYING (GPLv2): https://github.com/tmolteno/necpp/blob/master/COPYING
- VOACAP (NTIA/ITS): https://its.ntia.gov/ ; original distribution notice via http://www.greg-hand.com/hf.html
