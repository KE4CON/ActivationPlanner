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

        try
        {
            string inputPath = Path.Combine(runDir, InputFileName);
            string outputPath = Path.Combine(runDir, OutputFileName);
            await File.WriteAllTextAsync(inputPath, _writer.Write(geometry), ct).ConfigureAwait(false);

            var request = new ProcessRequest(
                _options.ExecutablePath,
                ["-i", inputPath, "-o", outputPath],
                WorkingDirectory: runDir);

            ProcessResult result = await _transport.RunAsync(request, ct).ConfigureAwait(false);
            if (result.ExitCode != 0)
                throw new NecExecutionException(
                    $"nec2++ exited with code {result.ExitCode}.", result.ExitCode, result.StandardError);

            if (!File.Exists(outputPath))
                throw new NecExecutionException(
                    $"nec2++ produced no output file at '{outputPath}'.", result.ExitCode, result.StandardError);

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
