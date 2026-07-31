namespace ActivationPlanner.ProcessEngine.Voacap;

/// <summary>
/// One frequency's predicted parameters within a single hour, as read from a VOACAP
/// METHOD 30 output block. Layer-1 raw values (VOACAP's own units and sign conventions);
/// the planner domain layer elevates these into band recommendations.
/// <para>
/// Every field is nullable because VOACAP prints "-" for a frequency slot it did not
/// evaluate. <see cref="RawRow"/> preserves the full set of parameter rows for this
/// frequency (keyed by VOACAP's row label) so nothing read from the file is lost.
/// </para>
/// </summary>
public sealed record VoacapFrequencySample
{
    /// <summary>Frequency in MHz, as printed on the FREQ row (rounded to 0.1 MHz by VOACAP).</summary>
    public required double FrequencyMhz { get; init; }

    /// <summary>Dominant propagation mode, e.g. "1F2" (MODE row). Null if not evaluated.</summary>
    public string? Mode { get; init; }

    /// <summary>Circuit reliability, 0-1 (REL row) — the primary band-quality figure.</summary>
    public double? Reliability { get; init; }

    /// <summary>Signal-to-noise ratio, dB (SNR row).</summary>
    public double? Snr { get; init; }

    /// <summary>Median signal power, dBW (S DBW row).</summary>
    public double? SignalPowerDbw { get; init; }

    /// <summary>Median noise power, dBW (N DBW row).</summary>
    public double? NoisePowerDbw { get; init; }

    /// <summary>Median path loss, dB (LOSS row).</summary>
    public double? PathLossDb { get; init; }

    /// <summary>Fraction of days the frequency is below the MUF, 0-1 (MUFday row).</summary>
    public double? MufDays { get; init; }

    /// <summary>Take-off (elevation) angle, degrees (TANGLE row).</summary>
    public double? TakeoffAngleDeg { get; init; }

    /// <summary>SNR at the required reliability, dB (SNRxx row).</summary>
    public double? SnrAtRequiredReliabilityDb { get; init; }

    /// <summary>Every parameter row for this frequency, keyed by VOACAP row label ("-" → null).</summary>
    public required IReadOnlyDictionary<string, string?> RawRow { get; init; }
}

/// <summary>One hour's VOACAP prediction: the MUF plus a sample per evaluated frequency.</summary>
public sealed record VoacapHourBlock
{
    /// <summary>UTC hour of the prediction (VOACAP prints e.g. 1.0 .. 24.0).</summary>
    public required double HourUtc { get; init; }

    /// <summary>Maximum usable frequency for the hour, MHz (the MUF column of the FREQ row).</summary>
    public required double MufMhz { get; init; }

    /// <summary>One sample per frequency VOACAP actually evaluated this hour.</summary>
    public required IReadOnlyList<VoacapFrequencySample> Samples { get; init; }
}

/// <summary>
/// A parsed VOACAP METHOD 30 run: the constant circuit metadata plus one block per hour.
/// This is the raw Layer-1 result; <c>PropagationModel</c> maps it to planner-domain
/// band predictions.
/// </summary>
public sealed record VoacapRawPrediction
{
    /// <summary>Transmit-site label echoed by VOACAP, if found.</summary>
    public string? TransmitterLabel { get; init; }

    /// <summary>Receive-site label echoed by VOACAP, if found.</summary>
    public string? ReceiverLabel { get; init; }

    /// <summary>Smoothed sunspot number echoed by VOACAP, if found.</summary>
    public double? SunspotNumber { get; init; }

    /// <summary>Per-hour prediction blocks, in file order.</summary>
    public required IReadOnlyList<VoacapHourBlock> Hours { get; init; }
}
