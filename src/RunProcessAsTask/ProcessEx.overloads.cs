namespace RunProcessAsTask;

/// <summary>
/// These overloads match the ones in Process.Start to make it a simpler transition for callers,
/// see http://msdn.microsoft.com/en-us/library/system.diagnostics.process.start.aspx.
/// </summary>
public static partial class ProcessEx
{
    /// <summary>
    /// Runs asynchronous process.
    /// </summary>
    /// <param name="fileName">An application or document which starts the process.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task<ProcessResults> RunAsync(string fileName)
        => RunAsync(new ProcessStartInfo(fileName));

    /// <summary>
    /// Runs asynchronous process.
    /// </summary>
    /// <param name="fileName">An application or document which starts the process.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task<ProcessResults> RunAsync(string fileName, CancellationToken cancellationToken)
        => RunAsync(new ProcessStartInfo(fileName), cancellationToken);

    /// <summary>
    /// Runs asynchronous process.
    /// </summary>
    /// <param name="fileName">An application or document which starts the process.</param>
    /// <param name="arguments">Command-line arguments to pass to the application when the process starts.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task<ProcessResults> RunAsync(string fileName, string arguments)
        => RunAsync(new ProcessStartInfo(fileName, arguments));

    /// <summary>
    /// Runs asynchronous process.
    /// </summary>
    /// <param name="fileName">An application or document which starts the process.</param>
    /// <param name="arguments">Command-line arguments to pass to the application when the process starts.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task<ProcessResults> RunAsync(string fileName, string arguments, CancellationToken cancellationToken)
        => RunAsync(new ProcessStartInfo(fileName, arguments), cancellationToken);

    /// <summary>
    /// Runs asynchronous process.
    /// </summary>
    /// <param name="processStartInfo">The <see cref="ProcessStartInfo" /> that contains the information that is used to start the process, including the file name and any command-line arguments.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task<ProcessResults> RunAsync(ProcessStartInfo processStartInfo)
        => RunAsync(processStartInfo, CancellationToken.None);

    /// <summary>
    /// Runs asynchronous process.
    /// </summary>
    /// <param name="processStartInfo">The <see cref="ProcessStartInfo" /> that contains the information that is used to start the process, including the file name and any command-line arguments.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task<ProcessResults> RunAsync(ProcessStartInfo processStartInfo, CancellationToken cancellationToken)
        => RunAsync(processStartInfo, [], [], cancellationToken);
}
