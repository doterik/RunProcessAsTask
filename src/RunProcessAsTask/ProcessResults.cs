#pragma warning disable CA1819 // Properties should not return arrays.

namespace RunProcessAsTask;

/// <summary>
/// Contains information about process after it has exited.
/// </summary>
public sealed class ProcessResults(Process process, DateTime processStartTime, string[] standardOutput, string[] standardError) : IDisposable
{
    public Process Process { get; } = process;
    public TimeSpan RunTime { get; } = process.ExitTime - processStartTime;
    public string[] StandardOutput { get; } = standardOutput;
    public string[] StandardError { get; } = standardError;
    public int ExitCode { get; } = process.ExitCode;
    public void Dispose() => Process.Dispose();
}
