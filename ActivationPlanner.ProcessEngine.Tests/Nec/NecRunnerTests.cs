using ActivationPlanner.ProcessEngine;
using ActivationPlanner.ProcessEngine.Nec;
using ActivationPlanner.ProcessEngine.Tests.Fixtures;

namespace ActivationPlanner.ProcessEngine.Tests.Nec;

public sealed class NecRunnerTests
{
    private static NecGeometryInput Geometry() => new()
    {
        Wires = [new NecWire(0, 21, -5.03, 0, 10, 5.03, 0, 10, 0.001)],
        FrequencyMhz = 14.1,
        Ground = new NecGround(2, 13.0, 0.005),
        Excitation = new NecExcitation(0, 11),
    };

    /// <summary>A fake nec2++: writes the fixture to the -o path and returns success.</summary>
    private sealed class FakeNecTransport(bool writeOutput = true, int exitCode = 0) : IProcessTransport
    {
        public ProcessRequest? LastRequest { get; private set; }

        public Task<ProcessResult> RunAsync(ProcessRequest request, CancellationToken ct = default)
        {
            LastRequest = request;
            if (writeOutput)
            {
                // nec2++ writes to the path following "-o".
                int oIndex = request.Arguments.ToList().IndexOf("-o");
                if (oIndex >= 0 && oIndex + 1 < request.Arguments.Count)
                    File.WriteAllText(request.Arguments[oIndex + 1], NecFixtures.DipoleOutputText());
            }
            return Task.FromResult(new ProcessResult(exitCode, string.Empty, string.Empty));
        }
    }

    [Fact]
    public async Task Runs_and_parses_output()
    {
        var runner = new NecRunner(new FakeNecTransport(), new NecRunnerOptions("nec2++"));
        var result = await runner.RunAsync(Geometry());

        Assert.NotNull(result.Impedance);
        Assert.Equal(19, result.Pattern.Count);
        Assert.Equal(6.20, result.PeakGain!.TotalGainDb, precision: 2);
    }

    [Fact]
    public async Task Invokes_nec_with_input_and_output_arguments()
    {
        var transport = new FakeNecTransport();
        var runner = new NecRunner(transport, new NecRunnerOptions("nec2++"));

        await runner.RunAsync(Geometry());

        var args = transport.LastRequest!.Arguments;
        Assert.Contains("-i", args);
        Assert.Contains("-o", args);
        Assert.Equal("nec2++", transport.LastRequest.ExecutablePath);
    }

    [Fact]
    public async Task Throws_when_nec_exits_nonzero()
    {
        var runner = new NecRunner(new FakeNecTransport(writeOutput: false, exitCode: 1), new NecRunnerOptions("nec2++"));
        var ex = await Assert.ThrowsAsync<NecExecutionException>(() => runner.RunAsync(Geometry()));
        Assert.Equal(1, ex.ExitCode);
    }

    [Fact]
    public async Task Throws_when_output_missing()
    {
        var runner = new NecRunner(new FakeNecTransport(writeOutput: false), new NecRunnerOptions("nec2++"));
        await Assert.ThrowsAsync<NecExecutionException>(() => runner.RunAsync(Geometry()));
    }

    [Fact]
    public async Task Cleans_up_its_temp_directory()
    {
        var transport = new FakeNecTransport();
        var runner = new NecRunner(transport, new NecRunnerOptions("nec2++"));
        await runner.RunAsync(Geometry());
        Assert.False(Directory.Exists(transport.LastRequest!.WorkingDirectory!));
    }
}
