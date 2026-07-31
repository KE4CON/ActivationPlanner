using System.Diagnostics;

namespace ActivationPlanner.ProcessEngine;

/// <summary>
/// The production <see cref="IProcessTransport"/>: launches a real process via
/// <see cref="Process"/>, capturing stdout/stderr fully and asynchronously. Cancellation
/// kills the process tree. No blocking waits (no <c>Thread.Sleep</c>, no <c>WaitForExit()</c>
/// on the UI thread) — everything is awaited.
/// </summary>
public sealed class ProcessTransport : IProcessTransport
{
    /// <inheritdoc />
    public async Task<ProcessResult> RunAsync(ProcessRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrEmpty(request.ExecutablePath);

        var startInfo = new ProcessStartInfo
        {
            FileName = request.ExecutablePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = request.WorkingDirectory ?? string.Empty,
        };
        foreach (string arg in request.Arguments)
            startInfo.ArgumentList.Add(arg);

        using var process = new Process { StartInfo = startInfo };

        try
        {
            if (!process.Start())
                throw new ProcessTransportException(
                    $"Failed to start process '{request.ExecutablePath}'.");
        }
        catch (Exception ex) when (ex is not ProcessTransportException)
        {
            throw new ProcessTransportException(
                $"Failed to start process '{request.ExecutablePath}': {ex.Message}", ex);
        }

        // Read both streams concurrently to avoid deadlocking on a full pipe buffer.
        Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
        Task<string> stderrTask = process.StandardError.ReadToEndAsync(ct);

        try
        {
            await process.WaitForExitAsync(ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            TryKill(process);
            throw;
        }

        string stdout = await stdoutTask.ConfigureAwait(false);
        string stderr = await stderrTask.ConfigureAwait(false);

        return new ProcessResult(process.ExitCode, stdout, stderr);
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch
        {
            // Best-effort kill on cancellation; the original cancellation is what propagates.
        }
    }
}

/// <summary>Thrown when an external process cannot be launched or fails at the transport level.</summary>
public sealed class ProcessTransportException : Exception
{
    public ProcessTransportException(string message) : base(message) { }
    public ProcessTransportException(string message, Exception inner) : base(message, inner) { }
}
