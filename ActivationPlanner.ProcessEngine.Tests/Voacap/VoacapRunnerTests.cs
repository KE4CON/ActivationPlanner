using ActivationPlanner.ProcessEngine;
using ActivationPlanner.ProcessEngine.Tests.Fixtures;
using ActivationPlanner.ProcessEngine.Voacap;

namespace ActivationPlanner.ProcessEngine.Tests.Voacap;

/// <summary>
/// Exercises the runner's orchestration (write deck → invoke → read output → parse)
/// through a fake transport, so no real voacapl install is needed. The fake stands in
/// for voacapl: it verifies the invocation and drops the known-good output file where the
/// runner expects to read it.
/// </summary>
public sealed class VoacapRunnerTests
{
    private static VoacapDeckInput MinimalDeck() => new()
    {
        TxLatitudeDeg = 35.80,
        TxLongitudeDeg = -5.90,
        RxLatitudeDeg = 44.90,
        RxLongitudeDeg = 20.50,
        Year = 1994,
        MonthValue = 6.00,
        SunspotNumber = 100.0,
        TxAntenna = new VoacapAntenna("default/isotrope", BearingDeg: 0, PowerKw: 100),
        RxAntenna = new VoacapAntenna("default/isotrope", BearingDeg: 0, PowerKw: 100),
        FrequenciesMhz = [7.1, 14.1, 21.1, 28.3],
    };

    /// <summary>A transport that pretends to be voacapl: writes the fixture output, returns success.</summary>
    private sealed class FakeVoacapTransport : IProcessTransport
    {
        private readonly int _exitCode;
        private readonly bool _writeOutput;
        public ProcessRequest? LastRequest { get; private set; }

        public FakeVoacapTransport(bool writeOutput = true, int exitCode = 0)
        {
            _writeOutput = writeOutput;
            _exitCode = exitCode;
        }

        public Task<ProcessResult> RunAsync(ProcessRequest request, CancellationToken ct = default)
        {
            LastRequest = request;
            if (_writeOutput && request.WorkingDirectory is { } dir)
            {
                // voacapl writes <output> into the run directory; the run dir is the last
                // "--run-dir=" argument's value == the working directory here.
                string outPath = Path.Combine(dir, "planner.out");
                File.WriteAllText(outPath, VoacapFixtures.SampleOutputText());
            }
            return Task.FromResult(new ProcessResult(_exitCode, string.Empty, string.Empty));
        }
    }

    [Fact]
    public async Task Runs_deck_and_parses_output()
    {
        var transport = new FakeVoacapTransport();
        var runner = new VoacapRunner(transport, new VoacapRunnerOptions("voacapl", "/home/ham/itshfbc"));

        VoacapRawPrediction result = await runner.RunAsync(MinimalDeck());

        Assert.Equal(24, result.Hours.Count);
        Assert.Equal(100.0, result.SunspotNumber);
    }

    [Fact]
    public async Task Invokes_voacapl_with_expected_arguments()
    {
        var transport = new FakeVoacapTransport();
        var runner = new VoacapRunner(transport, new VoacapRunnerOptions("voacapl", "/home/ham/itshfbc"));

        await runner.RunAsync(MinimalDeck());

        var req = transport.LastRequest!;
        Assert.Equal("voacapl", req.ExecutablePath);
        Assert.Contains("--silent", req.Arguments);
        Assert.Contains("/home/ham/itshfbc", req.Arguments);
        Assert.Contains("planner.dat", req.Arguments);
        Assert.Contains("planner.out", req.Arguments);
        Assert.Contains(req.Arguments, a => a.StartsWith("--run-dir=", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Writes_input_deck_into_run_directory()
    {
        string runDir = Path.Combine(Path.GetTempPath(), "ap-runner-test-" + Guid.NewGuid().ToString("N"));
        var transport = new FakeVoacapTransport();
        var runner = new VoacapRunner(transport, new VoacapRunnerOptions("voacapl", "/itshfbc"));

        try
        {
            await runner.RunAsync(MinimalDeck(), runDirectory: runDir);
            string deck = await File.ReadAllTextAsync(Path.Combine(runDir, "planner.dat"));
            Assert.Contains("CIRCUIT", deck);
            Assert.Contains("METHOD", deck);
        }
        finally
        {
            if (Directory.Exists(runDir)) Directory.Delete(runDir, recursive: true);
        }
    }

    [Fact]
    public async Task Throws_when_voacapl_exits_nonzero()
    {
        var transport = new FakeVoacapTransport(writeOutput: false, exitCode: 2);
        var runner = new VoacapRunner(transport, new VoacapRunnerOptions("voacapl", "/itshfbc"));

        var ex = await Assert.ThrowsAsync<VoacapExecutionException>(() => runner.RunAsync(MinimalDeck()));
        Assert.Equal(2, ex.ExitCode);
    }

    [Fact]
    public async Task Throws_when_output_file_missing()
    {
        var transport = new FakeVoacapTransport(writeOutput: false, exitCode: 0);
        var runner = new VoacapRunner(transport, new VoacapRunnerOptions("voacapl", "/itshfbc"));

        await Assert.ThrowsAsync<VoacapExecutionException>(() => runner.RunAsync(MinimalDeck()));
    }

    [Fact]
    public async Task Cleans_up_its_own_temp_run_directory()
    {
        var transport = new FakeVoacapTransport();
        var runner = new VoacapRunner(transport, new VoacapRunnerOptions("voacapl", "/itshfbc"));

        await runner.RunAsync(MinimalDeck());

        // The auto-created temp dir is the working directory recorded by the fake.
        string usedDir = transport.LastRequest!.WorkingDirectory!;
        Assert.False(Directory.Exists(usedDir), "runner should delete the temp run directory it created");
    }
}
