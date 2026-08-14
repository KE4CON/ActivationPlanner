using System.Collections.Generic;

namespace ActivationPlanner.PropagationModel.Bands;

/// <summary>
/// The US amateur band-privileges data, from FCC Part 97 (§97.301/.303/.305/.307/.313) — facts, not
/// the ARRL's copyrighted chart. License labels: General = General/Advanced/Extra unless split out.
/// Default power is 1500 W PEP unless a segment notes otherwise; always use the minimum needed.
/// A summary reference, not a legal authority — follow the current FCC rules. Verified 2026-08-14
/// (includes the 60 m change effective 2026-02-13).
/// </summary>
internal static class UsBandPlanData
{
    private static BandPlanSegment Seg(string range, string modes, string licenses) => new(range, modes, licenses);

    internal static IReadOnlyList<BandPlanBand> Bands { get; } =
    [
        new BandPlanBand("160m", "1.800–2.000 MHz",
            "General and up get the whole band, any mode. No Technician access.",
            [
                Seg("1.800–2.000", "CW / RTTY / data / phone / image (all modes)", "General, Adv, Extra"),
            ]),

        new BandPlanBand("80m", "3.500–4.000 MHz",
            "Extra gets phone down to 3.600, Advanced from 3.700, General from 3.800. Technicians get a CW-only slice (3.525–3.600) at 200 W.",
            [
                Seg("3.500–3.525", "CW only", "Extra only"),
                Seg("3.525–3.600", "CW / RTTY / data", "General+  ·  Tech (CW, 200 W)"),
                Seg("3.600–3.700", "CW / phone / image", "Extra only"),
                Seg("3.700–3.800", "CW / phone / image", "Advanced, Extra"),
                Seg("3.800–4.000", "CW / phone / image", "General, Adv, Extra"),
            ]),

        new BandPlanBand("60m", "5.3 MHz (channels + segment)",
            "General and up only. Four legacy channels at 100 W ERP, plus a low-power (~9 W) segment added Feb 2026. Keep it narrow — 2.8 kHz max.",
            [
                Seg("5332 / 5348 / 5373 / 5405 kHz", "USB / CW / data, 2.8 kHz (100 W ERP)", "General, Adv, Extra"),
                Seg("5351.5–5366.5 kHz", "Any mode ≤ 2.8 kHz (9.15 W ERP)", "General, Adv, Extra"),
            ]),

        new BandPlanBand("40m", "7.000–7.300 MHz",
            "General SSB runs 7.175–7.300; Extra and Advanced reach down to 7.125. Technicians get CW only (7.025–7.125) at 200 W.",
            [
                Seg("7.000–7.025", "CW only", "Extra only"),
                Seg("7.025–7.125", "CW / RTTY / data", "General+  ·  Tech (CW, 200 W)"),
                Seg("7.125–7.175", "CW / phone / image", "Advanced, Extra"),
                Seg("7.175–7.300", "CW / phone / image", "General, Adv, Extra"),
            ]),

        new BandPlanBand("30m", "10.100–10.150 MHz",
            "General and up, CW and digital only — no voice, ever. Capped at 200 W. Secondary band.",
            [
                Seg("10.100–10.150", "CW / RTTY / data — no phone (200 W)", "General, Adv, Extra"),
            ]),

        new BandPlanBand("20m", "14.000–14.350 MHz",
            "General SSB from 14.225 up; Advanced from 14.175; Extra from 14.150. No Technician access.",
            [
                Seg("14.000–14.025", "CW only", "Extra only"),
                Seg("14.025–14.150", "CW / RTTY / data", "General, Adv, Extra"),
                Seg("14.150–14.175", "CW / phone / image", "Extra only"),
                Seg("14.175–14.225", "CW / phone / image", "Advanced, Extra"),
                Seg("14.225–14.350", "CW / phone / image", "General, Adv, Extra"),
            ]),

        new BandPlanBand("17m", "18.068–18.168 MHz",
            "General and up. CW/digital in the low 42 kHz, voice above 18.110. No Technician access.",
            [
                Seg("18.068–18.110", "CW / RTTY / data", "General, Adv, Extra"),
                Seg("18.110–18.168", "CW / phone / image", "General, Adv, Extra"),
            ]),

        new BandPlanBand("15m", "21.000–21.450 MHz",
            "General SSB from 21.275; Advanced 21.225; Extra 21.200. Technicians get CW only (21.025–21.200) at 200 W.",
            [
                Seg("21.000–21.025", "CW only", "Extra only"),
                Seg("21.025–21.200", "CW / RTTY / data", "General+  ·  Tech (CW, 200 W)"),
                Seg("21.200–21.225", "CW / phone / image", "Extra only"),
                Seg("21.225–21.275", "CW / phone / image", "Advanced, Extra"),
                Seg("21.275–21.450", "CW / phone / image", "General, Adv, Extra"),
            ]),

        new BandPlanBand("12m", "24.890–24.990 MHz",
            "General and up. CW/digital low, voice above 24.930. No Technician access.",
            [
                Seg("24.890–24.930", "CW / RTTY / data", "General, Adv, Extra"),
                Seg("24.930–24.990", "CW / phone / image", "General, Adv, Extra"),
            ]),

        new BandPlanBand("10m", "28.000–29.700 MHz",
            "General and up get the whole band. Technicians get CW/data below 28.300 and SSB voice 28.300–28.500, at 200 W.",
            [
                Seg("28.000–28.300", "CW / RTTY / data", "General+  ·  Tech (200 W)"),
                Seg("28.300–28.500", "SSB phone (+ CW / image)", "General+  ·  Tech (SSB, 200 W)"),
                Seg("28.500–29.700", "CW / phone / image", "General, Adv, Extra"),
            ]),

        new BandPlanBand("6m", "50.0–54.0 MHz",
            "Everyone Technician and up gets it all. CW-only in the bottom 100 kHz, all modes above.",
            [
                Seg("50.0–50.1", "CW only", "All (Tech and up)"),
                Seg("50.1–54.0", "CW / phone / image / data (all modes)", "All (Tech and up)"),
            ]),

        new BandPlanBand("2m", "144–148 MHz",
            "Full privileges for everyone Technician and up. CW-only in the bottom 100 kHz, all modes above.",
            [
                Seg("144.0–144.1", "CW only", "All (Tech and up)"),
                Seg("144.1–148.0", "CW / phone / image / data (all modes)", "All (Tech and up)"),
            ]),

        new BandPlanBand("1.25m", "222–225 MHz",
            "Full privileges, all modes, for everyone Technician and up.",
            [
                Seg("222.0–225.0", "CW / phone / image / data (all modes)", "All (Tech and up)"),
            ]),

        new BandPlanBand("70cm", "420–450 MHz",
            "Full privileges, all modes, everyone Technician and up. Watch for local/secondary-use restrictions near borders.",
            [
                Seg("420.0–450.0", "CW / phone / image / data (all modes)", "All (Tech and up)"),
            ]),
    ];
}
