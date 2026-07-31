namespace ActivationPlanner.ProcessEngine.Voacap;

/// <summary>
/// Configuration for locating the user's VOACAP install. VOACAP is never bundled — the
/// user installs it themselves (voacapl on Linux/macOS, VOACAPW.EXE on Windows), which is
/// what keeps NTIA's terms off this project. These paths point at that install.
/// </summary>
/// <param name="ExecutablePath">Path to the voacapl / VOACAPW executable.</param>
/// <param name="ItshfbcDirectory">
/// Path to the VOACAP <c>itshfbc</c> data directory (coefficients, antenna patterns).
/// Passed to voacapl as its required <c>voacap_directory</c> argument.
/// </param>
public sealed record VoacapRunnerOptions(string ExecutablePath, string ItshfbcDirectory);

/// <summary>
/// Runs a VOACAP point-to-point prediction from a rendered deck. Abstracted so the
/// propagation-model layer can be tested with a fake in place of a real voacapl install.
/// </summary>
public interface IVoacapRunner
{
    /// <summary>Render, invoke voacapl, and parse — see <see cref="VoacapRunner.RunAsync"/>.</summary>
    Task<VoacapRawPrediction> RunAsync(
        VoacapDeckInput deck, string? runDirectory = null, CancellationToken ct = default);
}

/// <summary>
/// Drives a single VOACAP point-to-point prediction end to end: render the input deck,
/// invoke voacapl through the (mockable) <see cref="IProcessTransport"/>, then parse the
/// output file it writes.
/// <para>
/// Each run uses its own throwaway directory (voacapl's <c>--run-dir</c>) so concurrent
/// predictions — e.g. the background propagation-trend sampler — never collide on shared
/// files. The deck is written to <c>&lt;runDir&gt;/planner.dat</c> and voacapl is told to
/// write <c>planner.out</c> there. This is Layer 1: raw I/O and process orchestration only.
/// </para>
/// </summary>
public sealed class VoacapRunner : IVoacapRunner
{
    private const string InputFileName = "planner.dat";
    private const string OutputFileName = "planner.out";

    private readonly IProcessTransport _transport;
    private readonly VoacapRunnerOptions _options;
    private readonly VoacapDeckWriter _writer = new();
    private readonly VoacapOutputParser _parser = new();

    public VoacapRunner(IProcessTransport transport, VoacapRunnerOptions options)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Run a prediction for <paramref name="deck"/>. Creates an isolated run directory,
    /// invokes voacapl, parses its output, and cleans up. The run directory can be
    /// overridden (e.g. for diagnostics); otherwise a unique temp directory is used.
    /// </summary>
    /// <exception cref="VoacapExecutionException">voacapl exited non-zero or wrote no output file.</exception>
    public async Task<VoacapRawPrediction> RunAsync(
        VoacapDeckInput deck,
        string? runDirectory = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(deck);

        string runDir = runDirectory ?? Path.Combine(Path.GetTempPath(), "activationplanner-voacap-" + Guid.NewGuid().ToString("N"));
        bool ownsRunDir = runDirectory is null;
        Directory.CreateDirectory(runDir);

        try
        {
            string deckText = _writer.Write(deck);
            string inputPath = Path.Combine(runDir, InputFileName);
            await File.WriteAllTextAsync(inputPath, deckText, ct).ConfigureAwait(false);

            var request = new ProcessRequest(
                _options.ExecutablePath,
                [
                    "--silent",
                    "--run-dir=" + runDir,
                    _options.ItshfbcDirectory,
                    InputFileName,
                    OutputFileName,
                ],
                WorkingDirectory: runDir);

            ProcessResult result = await _transport.RunAsync(request, ct).ConfigureAwait(false);
            if (result.ExitCode != 0)
                throw new VoacapExecutionException(
                    $"voacapl exited with code {result.ExitCode}.", result.ExitCode, result.StandardError);

            string outputPath = Path.Combine(runDir, OutputFileName);
            if (!File.Exists(outputPath))
                throw new VoacapExecutionException(
                    $"voacapl produced no output file at '{outputPath}'.", result.ExitCode, result.StandardError);

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
            // A stray temp directory is not worth failing a completed prediction over.
        }
    }
}

/// <summary>Thrown when voacapl runs but fails to produce a usable prediction.</summary>
public sealed class VoacapExecutionException : Exception
{
    public VoacapExecutionException(string message, int exitCode, string standardError)
        : base(message)
    {
        ExitCode = exitCode;
        StandardError = standardError;
    }

    /// <summary>voacapl's process exit code.</summary>
    public int ExitCode { get; }

    /// <summary>Captured stderr, to aid diagnosis (bad itshfbc path, missing coefficients, etc.).</summary>
    public string StandardError { get; }
}
