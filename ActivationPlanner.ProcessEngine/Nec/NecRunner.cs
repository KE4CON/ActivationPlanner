namespace ActivationPlanner.ProcessEngine.Nec;

/// <summary>Locates the user's NEC2++ install (installed by the user, like VOACAP).</summary>
/// <param name="ExecutablePath">Path to the nec2++ executable.</param>
public sealed record NecRunnerOptions(string ExecutablePath);

/// <summary>
/// Runs a NEC2 antenna model from a geometry deck. Abstracted so the propagation-model layer can
/// be tested with a fake in place of a real nec2++ install.
/// </summary>
public interface INecRunner
{
    /// <summary>Render, invoke nec2++, and parse — see <see cref="NecRunner.RunAsync"/>.</summary>
    Task<NecRawResult> RunAsync(
        NecGeometryInput geometry, string? runDirectory = null, CancellationToken ct = default);
}

/// <summary>
/// Drives a single NEC2 model end to end: render the geometry deck, invoke nec2++ through the
/// (mockable) <see cref="IProcessTransport"/>, then parse its output. Each run uses its own
/// throwaway directory so concurrent models never collide. Layer 1: raw I/O and process
/// orchestration only.
/// </summary>
public sealed class NecRunner : INecRunner
{
    private const string InputFileName = "antenna.nec";
    private const string OutputFileName = "antenna.out";

    private readonly IProcessTransport _transport;
    private readonly NecRunnerOptions _options;
    private readonly NecDeckWriter _writer = new();
    private readonly NecOutputParser _parser = new();

    public NecRunner(IProcessTransport transport, NecRunnerOptions options)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    /// <exception cref="NecExecutionException">nec2++ exited non-zero or wrote no output file.</exception>
    public async Task<NecRawResult> RunAsync(
        NecGeometryInput geometry, string? runDirectory = null, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        string runDir = runDirectory ?? Path.Combine(Path.GetTempPath(), "activationplanner-nec-" + Guid.NewGuid().ToString("N"));
        bool ownsRunDir = runDirectory is null;
        Directory.CreateDirectory(runDir);

        string deckText = _writer.Write(geometry);
        try
        {
            string inputPath = Path.Combine(runDir, InputFileName);
            string outputPath = Path.Combine(runDir, OutputFileName);
            await File.WriteAllTextAsync(inputPath, deckText, ct).ConfigureAwait(false);

            // Pass short, run-directory-relative filenames (not the absolute temp path): nec2c has a
            // hardcoded input-filename length limit and aborts on a long path. The working directory
            // is the run directory, so relative names resolve to the same files we read below.
            var request = new ProcessRequest(
                _options.ExecutablePath,
                ["-i", InputFileName, "-o", OutputFileName],
                WorkingDirectory: runDir);

            ProcessResult result = await _transport.RunAsync(request, ct).ConfigureAwait(false);
            if (result.ExitCode != 0)
            {
                WriteFailureDiagnostic(deckText, result.StandardError, outputPath);
                throw new NecExecutionException(
                    $"nec2++ exited with code {result.ExitCode}.", result.ExitCode, result.StandardError);
            }

            if (!File.Exists(outputPath))
            {
                WriteFailureDiagnostic(deckText, result.StandardError, outputPath);
                throw new NecExecutionException(
                    $"nec2++ produced no output file at '{outputPath}'.", result.ExitCode, result.StandardError);
            }

            string outputText = await File.ReadAllTextAsync(outputPath, ct).ConfigureAwait(false);
            return _parser.Parse(outputText);
        }
        finally
        {
            if (ownsRunDir)
                TryCleanup(runDir);
        }
    }

    private static void TryCleanup(string runDir)
    {
        try
        {
            if (Directory.Exists(runDir))
                Directory.Delete(runDir, recursive: true);
        }
        catch
        {
            // A stray temp directory is not worth failing a completed model over.
        }
    }

    /// <summary>
    /// On a NEC failure, drop the exact deck that failed plus nec2c's own output (which carries the
    /// real reason, e.g. "GEOMETRY DATA ERROR") to a stable file, since the run directory is about
    /// to be deleted. Best-effort — diagnostics must never mask the original failure.
    /// </summary>
    private static void WriteFailureDiagnostic(string deckText, string stdErr, string outputPath)
    {
        try
        {
            string necOutput = File.Exists(outputPath) ? File.ReadAllText(outputPath) : "(no output file written)";
            string report =
                "=== Activation Planner — NEC run failure ===\n\n" +
                "--- Input deck (.nec) ---\n" + deckText + "\n" +
                "--- nec2c stderr ---\n" + stdErr + "\n" +
                "--- nec2c output (.out) ---\n" + necOutput + "\n";
            File.WriteAllText(
                Path.Combine(Path.GetTempPath(), "activationplanner-nec-last-failure.txt"), report);
        }
        catch
        {
            // Diagnostics are a convenience; never let them throw over the real error.
        }
    }
}

/// <summary>Thrown when nec2++ runs but fails to produce a usable result.</summary>
public sealed class NecExecutionException : Exception
{
    public NecExecutionException(string message, int exitCode, string standardError) : base(message)
    {
        ExitCode = exitCode;
        StandardError = standardError;
    }

    /// <summary>nec2++'s process exit code.</summary>
    public int ExitCode { get; }

    /// <summary>Captured stderr, to aid diagnosis.</summary>
    public string StandardError { get; }
}
