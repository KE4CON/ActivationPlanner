using System.Threading;
using System.Threading.Tasks;

namespace ActivationPlanner.ProcessEngine;

/// <summary>
/// Boundary over raw external-process I/O (VOACAP / NEC2++ shell-outs). All
/// process access goes through this interface so the propagation layers can be
/// unit-tested without a real VOACAP/NEC2++ install on the test machine.
/// <para>
/// Placeholder for the architecture skeleton — the real surface (write input
/// deck, run process, read raw output) is defined in Phase 2.
/// </para>
/// </summary>
public interface IProcessTransport
{
    /// <summary>
    /// Run an external executable with the given arguments and working directory,
    /// returning its raw stdout. Phase 2 expands this contract.
    /// </summary>
    Task<string> RunAsync(
        string executablePath,
        string arguments,
        string workingDirectory,
        CancellationToken ct = default);
}
