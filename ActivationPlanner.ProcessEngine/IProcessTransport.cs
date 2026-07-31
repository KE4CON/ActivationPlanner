namespace ActivationPlanner.ProcessEngine;

/// <summary>
/// A request to run an external executable. Arguments are passed as a list (not a single
/// string) so the transport can quote/escape each one correctly for the host OS.
/// </summary>
/// <param name="ExecutablePath">Full path to the executable (e.g. the user's voacapl / VOACAPW.EXE).</param>
/// <param name="Arguments">Ordered argument list, one element per argument.</param>
/// <param name="WorkingDirectory">Working directory for the process, or null to inherit the current one.</param>
public sealed record ProcessRequest(
    string ExecutablePath,
    IReadOnlyList<string> Arguments,
    string? WorkingDirectory = null);

/// <summary>The captured result of a finished external process.</summary>
/// <param name="ExitCode">Process exit code (0 conventionally means success).</param>
/// <param name="StandardOutput">Full captured stdout.</param>
/// <param name="StandardError">Full captured stderr.</param>
public sealed record ProcessResult(
    int ExitCode,
    string StandardOutput,
    string StandardError);

/// <summary>
/// Boundary over raw external-process I/O (the VOACAP / NEC2++ shell-outs). All process
/// access goes through this interface so the propagation layers can be unit-tested with a
/// fake transport — no real VOACAP/NEC2++ install required on the test machine.
/// <para>
/// This is Layer 1: it launches a process and hands back its exit code and captured
/// streams. It has no knowledge of VOACAP file formats or planner domain concepts.
/// </para>
/// </summary>
public interface IProcessTransport
{
    /// <summary>
    /// Run <paramref name="request"/> to completion, capturing stdout/stderr. The returned
    /// task completes when the process exits. Cancellation kills the process.
    /// </summary>
    Task<ProcessResult> RunAsync(ProcessRequest request, CancellationToken ct = default);
}
