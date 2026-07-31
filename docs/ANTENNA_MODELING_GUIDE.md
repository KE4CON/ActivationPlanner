# Entering Your Antennas — What Each Field Means

Activation Planner models your antennas to predict their radiation pattern and pick bands. The
model is only as good as what you type in, so this page explains **exactly what to enter** for each
antenna type. Every field also shows a short version of this help right in the app.

> **The golden rule:** if you don't know a number, **leave it 0**. The planner fills in a sensible
> resonant estimate and clearly labels the pattern as an estimate — it never silently guesses.

All lengths and heights are in **feet**.

---

## The three fields

| Field | What it means |
|---|---|
| **Length** | The size of the radiating wire/element. *What exactly* depends on the antenna type — see below. |
| **Height** | How high the **feed point** is above the ground. |
| **Radials** | (Verticals only) The on-ground wires spread out under the antenna. |

The **Length** label changes with the antenna type so you always know what's being asked.

---

## By antenna type

### Dipole
- **Length — tip to tip:** the whole dipole end to end, **both legs together** (e.g. a 40 m dipole ≈ 66 ft).
- **Height:** height of the **center** (the feed point) above ground.
- *Leave Length 0* → modeled as a resonant half-wave for the band.

### End-fed half-wave (EFHW)
- **Wire length:** the total length of the wire (e.g. a 40 m EFHW ≈ 66 ft).
- **Height:** height of the fed end above ground (for a sloper, the high end).
- *Leave Length 0* → modeled as a resonant half-wave for the band.

### Vertical / Whip
- **Element length:** just the **vertical element**, not the radials. For a **loaded or modular**
  antenna where you don't know the electrical length — a Chameleon MPAS, a screwdriver, a Wolf River
  coil — **leave it 0** and the planner estimates a resonant quarter-wave.
- **Height:** height of the **base (feed point)** above ground. Sitting on the ground? Enter **0**.
- **Radials:** how many on-ground wires, and how long each is. No radials? Leave both 0.

### NVIS crossed dipole (e.g. Chameleon 4-wire NVIS / U.S. Army AS-2259)
This is **two dipoles crossed at 90°**, fed at the center on top of a short mast, with the four legs
sloping down to ground stakes. It radiates **straight up** for short-range regional / EMCOMM work.

- **Leg length:** the length of **ONE of the four wires**, measured from the center feed out to its
  far (staked) end — **not** all four added together. The Chameleon 4-wire NVIS uses **~45 ft** legs.
  *Leave 0* → modeled as a resonant quarter-wave leg.
- **Height:** the height of the **center feed at the top of the mast** (the apex). The legs slope down
  from here. A typical NVIS mast is **~15 ft**.

> **Worked example — Chameleon 4-wire NVIS:** Category = *NVIS crossed dipole*, Leg length = `45`,
> Height = `15`. The pattern should peak near straight-up (high take-off angle) — exactly what you
> want for regional NVIS on 40/60/80 m.

### Magnetic loop / Other
No automatic geometry yet — these need a hand-built model. The planner will tell you custom modeling
is required rather than guess.

---

## What "estimate" means on the plot
When you leave a length at 0, the antenna tab shows a blue note like *"Length not set — modeled as a
resonant quarter-wave…"*. The shape of the pattern is still representative; only the exact dimensions
were assumed. Enter the real length and the note disappears.

## Why the pattern matters
- **High take-off angle (energy up high):** short-range / NVIS / regional (good for EMCOMM nets).
- **Low take-off angle (energy toward the horizon):** long-range DX.

The planner uses this together with VOACAP propagation to recommend bands for your session.
