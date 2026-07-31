namespace ActivationPlanner.ProcessEngine.Voacap;

/// <summary>
/// One end's antenna as it appears on a VOACAP <c>ANTENNA</c> card. These are the
/// wire-format parameters only (a reference to an antenna pattern file the user's
/// VOACAP install already holds, plus bearing and power) — not a modeled antenna.
/// Real antenna modeling / library selection is Phase 3 and Phase 8; Phase 2 wires
/// up whatever pattern file the caller names.
/// </summary>
/// <param name="AntennaFile">
/// Pattern file relative to the VOACAP <c>antennas</c> directory, e.g.
/// "default/isotrope" or "samples/sample.00". Emitted left-justified in the 21-char
/// bracketed field; must not exceed <see cref="VoacapCardFormat.AntennaFileWidth"/>.
/// </param>
/// <param name="BearingDeg">Main-beam azimuth in degrees (0 = true north). ANTENNA f5.1 field.</param>
/// <param name="PowerKw">Transmitter power in kW (RX end conventionally carries the RX "power" slot). ANTENNA f10.4 field.</param>
/// <param name="GainOffsetDbi">Constant gain offset added to the pattern, dBi. ANTENNA f10.3 field.</param>
/// <param name="MinFrequencyMhz">Lower valid frequency for the pattern, MHz (ANTENNA I5 field 3).</param>
/// <param name="MaxFrequencyMhz">Upper valid frequency for the pattern, MHz (ANTENNA I5 field 4).</param>
public sealed record VoacapAntenna(
    string AntennaFile,
    double BearingDeg,
    double PowerKw,
    double GainOffsetDbi = 0.0,
    int MinFrequencyMhz = 2,
    int MaxFrequencyMhz = 30);

/// <summary>
/// Great-circle path direction for the <c>CIRCUIT</c> card.
/// </summary>
public enum VoacapPath
{
    /// <summary>Short great-circle path ('S').</summary>
    Short,

    /// <summary>Long great-circle path ('L').</summary>
    Long,
}

/// <summary>
/// foF2 map coefficient set for the <c>COEFFS</c> card.
/// </summary>
public enum VoacapCoefficients
{
    /// <summary>CCIR (Oslo) coefficients — the common default.</summary>
    Ccir,

    /// <summary>URSI88 coefficients.</summary>
    Ursi88,
}

/// <summary>
/// All parameters needed to render one VOACAP point-to-point (METHOD 30) input deck.
/// <para>
/// This is a Layer-1 wire-format DTO: primitive numbers positioned into VOACAP cards,
/// with no planner-domain concepts. Coordinates are signed decimal degrees
/// (North/East positive); the writer converts them to VOACAP magnitude + hemisphere
/// letter. Defaults mirror the standard VOACAP SYSTEM/FPROB/METHOD values so callers
/// only supply what varies per query.
/// </para>
/// </summary>
public sealed record VoacapDeckInput
{
    // ---- CIRCUIT (geography) ----

    /// <summary>Transmit-site latitude, signed decimal degrees (North positive).</summary>
    public required double TxLatitudeDeg { get; init; }

    /// <summary>Transmit-site longitude, signed decimal degrees (East positive).</summary>
    public required double TxLongitudeDeg { get; init; }

    /// <summary>Receive-site latitude, signed decimal degrees (North positive).</summary>
    public required double RxLatitudeDeg { get; init; }

    /// <summary>Receive-site longitude, signed decimal degrees (East positive).</summary>
    public required double RxLongitudeDeg { get; init; }

    /// <summary>Great-circle path direction.</summary>
    public VoacapPath Path { get; init; } = VoacapPath.Short;

    // ---- LABEL ----

    /// <summary>Transmit-site label (LABEL card, columns 11-30). Truncated to 20 chars.</summary>
    public string TxLabel { get; init; } = "TX";

    /// <summary>Receive-site label (LABEL card, columns 31-50). Truncated to 20 chars.</summary>
    public string RxLabel { get; init; } = "RX";

    // ---- TIME / MONTH / SUNSPOT ----

    /// <summary>First UTC hour to predict, inclusive (TIME I5 field 1).</summary>
    public int StartHourUtc { get; init; } = 1;

    /// <summary>Last UTC hour to predict, inclusive (TIME I5 field 2).</summary>
    public int StopHourUtc { get; init; } = 24;

    /// <summary>Hour increment (TIME I5 field 3).</summary>
    public int HourIncrement { get; init; } = 1;

    /// <summary>Four-digit year (MONTH columns 11-14).</summary>
    public required int Year { get; init; }

    /// <summary>
    /// Month value as VOACAP expects it: the month number with a fractional part,
    /// e.g. 6.00 for June (MONTH F5.2 field). Whole-month values are the norm.
    /// </summary>
    public required double MonthValue { get; init; }

    /// <summary>Smoothed sunspot number (SUNSPOT card).</summary>
    public required double SunspotNumber { get; init; }

    // ---- COEFFS ----

    /// <summary>foF2 coefficient set (COEFFS card).</summary>
    public VoacapCoefficients Coefficients { get; init; } = VoacapCoefficients.Ccir;

    // ---- SYSTEM (all six documented fields; defaults match a typical residential setup) ----

    /// <summary>Man-made noise at 3 MHz, entered positive (VOACAP negates it): SYSTEM field, dBW.</summary>
    public double NoiseDbw { get; init; } = 145.0;

    /// <summary>Minimum take-off angle, degrees (SYSTEM field <c>amind</c>).</summary>
    public double MinTakeoffAngleDeg { get; init; } = 3.0;

    /// <summary>Required circuit reliability, percent (SYSTEM field <c>xlufp</c>).</summary>
    public double RequiredReliabilityPercent { get; init; } = 90.0;

    /// <summary>Required signal-to-noise ratio, dB-Hz (SYSTEM field <c>rsn</c>).</summary>
    public double RequiredSnrDb { get; init; } = 73.0;

    /// <summary>Multipath power tolerance, dB (SYSTEM field <c>pmp</c>).</summary>
    public double MultipathPowerToleranceDb { get; init; } = 3.0;

    /// <summary>Multipath delay tolerance, ms (SYSTEM field <c>dmpx</c>).</summary>
    public double MultipathDelayToleranceMs { get; init; } = 0.1;

    // ---- FPROB ----

    /// <summary>
    /// The four FPROB values (E, F1, F2 layer probabilities and an above-the-MUF term).
    /// VOACAP default 1.00 / 1.00 / 1.00 / 0.00.
    /// </summary>
    public IReadOnlyList<double> LayerProbabilities { get; init; } = [1.00, 1.00, 1.00, 0.00];

    // ---- ANTENNA ----

    /// <summary>Transmit-end antenna (ANTENNA card 1).</summary>
    public required VoacapAntenna TxAntenna { get; init; }

    /// <summary>Receive-end antenna (ANTENNA card 2).</summary>
    public required VoacapAntenna RxAntenna { get; init; }

    // ---- FREQUENCY ----

    /// <summary>
    /// Frequencies to evaluate, MHz. At most <see cref="VoacapCardFormat.MaxFrequencies"/>;
    /// unused slots are written as 0.00. These are the bands the planner is testing.
    /// </summary>
    public required IReadOnlyList<double> FrequenciesMhz { get; init; }

    // ---- METHOD ----

    /// <summary>VOACAP method number (METHOD I5 field 1). 30 = point-to-point (the only Phase 2 mode).</summary>
    public int Method { get; init; } = 30;

    /// <summary>Lines-per-page paging value (LINEMAX card).</summary>
    public int LinesPerPage { get; init; } = 55;
}
