# POTA Self-Spot Permission Request — draft to send

**Suggested recipient:** POTA support / help desk (help@pota.app) — or post in the POTA
Discord's developer/API channel. **From:** Jim, KE4CON.

---

**Subject:** Permission request — self-spotting from "Activation Planner" (individual operator planning app)

Hello POTA Team,

My name is Jim (callsign **KE4CON**). I'm developing a free, cross-platform application
(Windows, macOS, and Linux — desktop and laptop) called
**Activation Planner** — an individual, pre-operation planning tool for amateur radio
operators. It helps a single operator plan one operating session: it recommends bands
using real VOACAP propagation predictions, matches those bands to the antennas the
operator actually owns, and builds a tailored gear/packing checklist. It supports several
activity types, POTA among them (also SOTA, Field Day, EMCOMM, and general operating).

The app already uses your **public, read-only** endpoints to show current activator spots
and park information (`GET /spot/activator`, `/park/{ref}`, etc.), which has been very
helpful for operators planning around a park.

**What I'd like your permission for:** allowing an operator to **self-spot** to the POTA
spot feed from within the app (`POST /spot/`, with the spotter and activator both being
the operator's own callsign). I know the endpoint is open and that several tools already
post to it, but I'd rather ask first and operate with your blessing than assume — so I
want to be sure third-party self-spotting is acceptable to you, and to follow whatever
rules you'd like.

If you approve, here is exactly how the feature will behave — I've designed it to be a
good citizen on a shared resource:

- **Self-spot only.** The app will only ever post a spot where the spotter callsign equals
  the activator callsign (the operator's own). It will never spot anyone else.
- **Manual only — one button, one spot.** A spot is posted only when the operator clicks a
  button. There is no background, automated, scheduled, or bulk spotting of any kind.
- **Clearly identified.** Every post will set `source` to `"ActivationPlanner"` and send a
  descriptive `User-Agent` (e.g. `ActivationPlanner/1.0 (KE4CON)`), so you can always see
  which app and author the traffic is coming from, and reach me if anything looks wrong.
- **Low volume.** It's an individual operator's planning tool, not a network service —
  traffic is limited to occasional single self-spots by one operator at a time.

I'm happy to adopt any requirements you have — rate limits, a specific `source`/User-Agent
string, attribution, a naming convention, or anything else — and equally happy to hold the
feature off entirely if you'd prefer I not post to the endpoint. I'd rather do this the way
you want it done.

Could you let me know whether third-party self-spotting from an app like this is acceptable,
and any conditions you'd like me to follow?

Thank you for POTA and for maintaining the spot infrastructure — it's a great program.

73,

Jim, KE4CON
*Activation Planner (in development)*
Email: jrospopo@gmail.com
