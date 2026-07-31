using System;
using System.IO;

namespace ActivationPlanner.UI.Composition;

/// <summary>Resolved on-disk paths for the bundled VOACAP install, if one was found.</summary>
/// <param name="ExecutablePath">The voacapl / VOACAPW executable.</param>
/// <param name="ItshfbcDirectory">The VOACAP <c>itshfbc</c> data directory (coeffs, antennas).</param>
public sealed record VoacapToolPaths(string ExecutablePath, string ItshfbcDirectory);

/// <summary>Resolved on-disk path for the bundled NEC2 engine, if one was found.</summary>
/// <param name="ExecutablePath">The nec2++ / nec2c executable.</param>
public sealed record NecToolPaths(string ExecutablePath);

/// <summary>
/// Locates the external propagation/antenna engines the composition root shells out to. Neither
/// tool is linked in-process (that is what keeps NEC2's GPLv2 and VOACAP's terms clear of the
/// planner's own license); we only need their file paths.
/// <para>
/// Resolution order, first hit wins:
/// <list type="number">
///   <item>Environment overrides — <c>ACTIVATIONPLANNER_VOACAP_EXE</c> / <c>_ITSHFBC</c> /
///     <c>_NEC_EXE</c>. Lets a developer point at a local build without touching the install.</item>
///   <item>A <c>tools/</c> folder beside the app, where the installer drops the bundled binaries
///     (Item #19): <c>tools/voacap/{voacapl.exe, itshfbc/}</c> and <c>tools/nec/nec2c.exe</c>.</item>
/// </list>
/// When a tool is not found, the corresponding <c>Try…</c> method returns <c>null</c> and the
/// composition root keeps the offline sample provider for that engine — so the app always launches,
/// installed tools or not.
/// </para>
/// <para>
/// Note for the installer (Item #19): the shipped voacapl/nec2c are Cygwin builds and need their
/// Cygwin runtime DLLs (e.g. <c>cygwin1.dll</c>, and voacapl's gfortran runtime) placed beside the
/// executable, or a native (non-Cygwin) build substituted.
/// </para>
/// </summary>
public static class ExternalToolLocator
{
    // Both Windows (.exe) and Unix (no suffix) build outputs are accepted.
    private static readonly string[] VoacapExeNames = ["voacapl.exe", "voacapl", "VOACAPW.EXE"];
    private static readonly string[] NecExeNames = ["nec2++.exe", "nec2++", "nec2c.exe", "nec2c"];

    /// <summary>Resolve the VOACAP install, or <c>null</c> if neither an override nor a bundle is present.</summary>
    public static VoacapToolPaths? TryLocateVoacap()
    {
        string? exe = FirstExisting(Environment.GetEnvironmentVariable("ACTIVATIONPLANNER_VOACAP_EXE"))
            ?? FindInDirectory(BundleDir("voacap"), VoacapExeNames);

        string? data = FirstExistingDirectory(Environment.GetEnvironmentVariable("ACTIVATIONPLANNER_ITSHFBC"))
            ?? FirstExistingDirectory(Path.Combine(BundleDir("voacap"), "itshfbc"));

        return exe is not null && data is not null ? new VoacapToolPaths(exe, data) : null;
    }

    /// <summary>Resolve the NEC2 engine, or <c>null</c> if neither an override nor a bundle is present.</summary>
    public static NecToolPaths? TryLocateNec()
    {
        string? exe = FirstExisting(Environment.GetEnvironmentVariable("ACTIVATIONPLANNER_NEC_EXE"))
            ?? FindInDirectory(BundleDir("nec"), NecExeNames);

        return exe is not null ? new NecToolPaths(exe) : null;
    }

    private static string BundleDir(string tool) =>
        Path.Combine(AppContext.BaseDirectory, "tools", tool);

    private static string? FirstExisting(string? path) =>
        !string.IsNullOrWhiteSpace(path) && File.Exists(path) ? path : null;

    private static string? FirstExistingDirectory(string? path) =>
        !string.IsNullOrWhiteSpace(path) && Directory.Exists(path) ? path : null;

    private static string? FindInDirectory(string directory, string[] names)
    {
        if (!Directory.Exists(directory))
            return null;
        foreach (string name in names)
        {
            string candidate = Path.Combine(directory, name);
            if (File.Exists(candidate))
                return candidate;
        }
        return null;
    }
}
