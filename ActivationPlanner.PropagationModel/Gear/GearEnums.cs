namespace ActivationPlanner.PropagationModel.Gear;

/// <summary>
/// Category of a non-antenna piece of owned gear. Mirrors the setup-wizard
/// steps (Item #10). Antennas are modeled separately by <see cref="AntennaProfile"/>
/// because they carry additional physical fields needed for propagation modeling.
/// </summary>
public enum GearCategory
{
    /// <summary>Transceiver / radio (e.g. IC-705, KX2).</summary>
    Radio,

    /// <summary>Power source — battery, solar panel, generator, PSU.</summary>
    Power,

    /// <summary>Digital-mode interface hardware (e.g. built-in USB codec, Digirig).</summary>
    DigitalInterface,

    /// <summary>EMCOMM-specific gear (go-kit items, ICS forms, etc.).</summary>
    Emcomm,

    /// <summary>Anything else the operator wants tracked.</summary>
    Other,
}

/// <summary>
/// Physical antenna family. The category name alone is NOT sufficient for
/// propagation matching (Item #9) — length/height are compared per-band — but it
/// selects the community library starting point and drives the Option A/B rules.
/// </summary>
public enum AntennaCategory
{
    /// <summary>Ground-mounted or elevated vertical.</summary>
    Vertical,

    /// <summary>Whip (mobile/portable), treated as a short vertical.</summary>
    Whip,

    /// <summary>End-fed half-wave.</summary>
    EndFedHalfWave,

    /// <summary>Center-fed or off-center-fed dipole.</summary>
    Dipole,

    /// <summary>Magnetic loop.</summary>
    MagneticLoop,

    /// <summary>
    /// NVIS crossed dipole — two dipoles crossed at 90°, apex/center-fed at the top of a short mast,
    /// legs sloping down toward ground stakes (the U.S. Army AS-2259/GR pattern, e.g. Chameleon's
    /// 4-wire NVIS). Radiates steeply upward for short-range regional / EMCOMM coverage. Has no
    /// community-library match, so it always routes to custom NEC modeling.
    /// </summary>
    NvisCrossedDipole,

    /// <summary>Homebrew / unusual design that maps poorly to any library category.</summary>
    Other,
}

/// <summary>
/// Where and how the feedline connects to the radiator. Required per Item #9 —
/// needed for accurate NEC modeling (source placement) and to tell apart antennas
/// that share a category name but behave differently.
/// </summary>
public enum FeedPointType
{
    CenterFed,
    EndFedHalfWave,
    EndFedRandomWire,
    OffCenterFed,
    BaseFed,
    Other,
}
