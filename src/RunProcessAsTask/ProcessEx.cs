#pragma warning disable CA2000 // Dispose objects before losing scope.
// #pragma warning disable MA0040 // Forward the CancellationToken parameter to methods that take one.
#pragma warning disable MA0051 // Method is too long.
#pragma warning disable RS0030 // Do not use banned APIs.
#pragma warning disable SA1601 // Partial elements should be documented.

namespace RunProcessAsTask;

public static partial class ProcessEx
{
    /// <summary>
    /// Runs asynchronous process.
    /// </summary>
    /// <param name="processStartInfo">The <see cref="ProcessStartInfo" /> that contains the information that is used to start the process, including the file name and any command-line arguments.</param>
    /// <param name="standardOutput">List that lines written to standard output by the process will be added to.</param>
    /// <param name="standardError">List that lines written to standard error by the process will be added to.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task<ProcessResults> RunAsync(ProcessStartInfo processStartInfo, IList<string> standardOutput, IList<string> standardError, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(processStartInfo);
        ArgumentNullException.ThrowIfNull(standardOutput);
        ArgumentNullException.ThrowIfNull(standardError);

        // Force some settings in the start info so we can capture the output.
        processStartInfo.UseShellExecute = false;
        processStartInfo.RedirectStandardOutput = true;
        processStartInfo.RedirectStandardError = true;

        var process = new Process // Not 'using' !!
        {
            StartInfo = processStartInfo,
            EnableRaisingEvents = true
        };

        var standardOutputResults = new TaskCompletionSource<string[]>();
        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data is null)
                standardOutputResults.SetResult([.. standardOutput]);
            else
                standardOutput.Add(e.Data);
        };

        var standardErrorResults = new TaskCompletionSource<string[]>();
        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data is null)
                standardErrorResults.SetResult([.. standardError]);
            else
                standardError.Add(e.Data);
        };

        var tcs = new TaskCompletionSource<ProcessResults>();
        var processStartTime = new TaskCompletionSource<DateTime>();

        process.Exited += async (sender, e) =>
            // Since the Exited event can happen asynchronously to the output and error events,
            // we await the task results for stdout/stderr to ensure they both closed. We must await
            // the stdout/stderr tasks instead of just accessing the Result property due to behavior on MacOS.
            // For more details, see the PR at https://github.com/jamesmanning/RunProcessAsTask/pull/16/
            _ = tcs.TrySetResult(
                new ProcessResults(
                    process,
                    await processStartTime.Task.ConfigureAwait(false),
                    await standardOutputResults.Task.ConfigureAwait(false),
                    await standardErrorResults.Task.ConfigureAwait(false)));

        await using (cancellationToken.Register(
            () =>
            {
                _ = tcs.TrySetCanceled(cancellationToken); // cancellationToken !?
                try
                {
                    if (!process.HasExited) process.Kill();
                }
                catch (InvalidOperationException) { /*~*/ }
            })
            .ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!process.Start())
            {
                _ = tcs.TrySetException(new InvalidOperationException("Failed to start process."));
            }
            else
            {
                var startTime = DateTime.Now;
                try
                {
                    startTime = process.StartTime;
                }
                catch
                {
                    // Best effort to try and get a more accurate start time, but if we fail to access StartTime
                    // (for instance, process has already existed), we still have a valid value to use.
                }

                processStartTime.SetResult(startTime);

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }

            return await tcs.Task.ConfigureAwait(false);
        }
    }
}
