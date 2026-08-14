# SOTA API Approval Request — draft

**Where to send:** the SOTA Reflector (reflector.sota.org.uk) — join the **"API-consumers"** group
and post in **Third Party Software**, and/or email the **SOTA Management Team**. Their Terms of
Service (https://api-db2.sota.org.uk/docs) require prior approval for AI-assisted software, joining
the API-consumers group, and naming a Designated Point of Contact. **From:** Jim, KE4CON.

---

**Subject:** Approval request — read-only SOTA API use for "Activation Planner" (free, non-commercial)

Hello SOTA Management Team,

My name is Jim (callsign **KE4CON**). I'm developing a **free, non-commercial** desktop application
called **Activation Planner** — an individual, pre-operation planning tool for amateur radio
operators. It recommends bands from real VOACAP propagation predictions, matches them to the
antennas the operator owns, and builds a packing checklist. It already integrates POTA's public
spot data, and I'd like to add **SOTA** support as well.

**What I'm requesting approval for:**
1. **Read-only** use of the SOTA API — displaying current SOTA **spots** (and summit lookups) inside
   the app so an operator can see activity while planning. This is the only access I need initially.
2. (Possibly later, and only if you're open to it) operator **self-spotting** — I understand that
   involves authentication and I would follow whatever process you require; I'm not asking for that
   now.

**Full disclosure, per your Terms of Service:** the app is being developed with **AI-assisted
tooling** (Claude Code), which I know your ToS flags for prior approval after past problems with
poorly-behaved AI-written bots. I want to be up front about that and be a good citizen:

- **Read-only and low volume.** An individual's planning tool, not a service — occasional spot
  fetches by one operator at a time, cached locally, no polling loops hammering your servers.
- **Human-reviewed and tested.** All code is reviewed and tested by me before it connects; I'll set
  a **descriptive User-Agent** identifying the app + my callsign so you can see and contact me.
- **I'll join the API-consumers group** and serve as the **Designated Point of Contact**, and I'll
  respect any rate limits, caching requirements, or endpoint guidance you specify.
- **Non-commercial.** The app is free; there is no compensation of any kind. If that ever changed,
  I understand a formal commercial licence would be required first.

Could you let me know whether read-only SOTA API use from an app like this is acceptable, and what
requirements or constraints you'd like me to follow? I'm happy to adjust anything, and equally happy
to hold off entirely until we're aligned.

Thank you for SOTA and for maintaining the API.

73,

Jim, KE4CON
*Activation Planner (in development) — free / non-commercial*
Email: jrospopo@gmail.com
