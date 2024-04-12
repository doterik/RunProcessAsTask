#pragma warning disable CA5394 // Do not use insecure randomness.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed.
#pragma warning disable MA0134 // Observe result of async calls.
#pragma warning disable SA1309 // Field names should not begin with underscore.

using System.Diagnostics;
using Xunit;

namespace RunProcessAsTask.Tests;

public class ProcessExTests
{
    private static ProcessStartInfo DummyStartProcessArgs(
        int expectedExitCode,
        int millisecondsToSleep,
        int expectedStandardOutputLineCount,
        int expectedStandardErrorLineCount)
        => new(
            @"c:\repos\RunProcessAsTask\src\DummyConsoleApp\~bin\Debug\net9.0\DummyConsoleApp.exe",
            string.Join(' ', expectedExitCode, millisecondsToSleep, expectedStandardOutputLineCount, expectedStandardErrorLineCount))
        { WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory };

    private static string[] InitializeArray(string txt, int count)
        => Enumerable.Range(1, count).Select(i => $"Standard {txt} line #{i}").ToArray();

    [Fact]
    public async Task WhenProcessRunsNormally_ReturnsExpectedResults()
    {
        // Arrange
        const int ExpectedExitCode = 123;
        const int MillisecondsToSleep = 5_000; // Set a minimum run time so we can validate it as part of the output.
        const int ExpectedStandardOutputLineCount = 5;
        const int ExpectedStandardErrorLineCount = 3;
        var expectedStandardOutput = InitializeArray("output", ExpectedStandardOutputLineCount);
        var expectedStandardError = InitializeArray("error", ExpectedStandardErrorLineCount);

        // Act
        var processStartInfo = DummyStartProcessArgs(ExpectedExitCode, MillisecondsToSleep, ExpectedStandardOutputLineCount, ExpectedStandardErrorLineCount);
        var task = ProcessEx.RunAsync(processStartInfo);
        using var results = await task;

        // Assert
        Assert.NotNull(task);
        Assert.NotNull(results);
        Assert.Equal(TaskStatus.RanToCompletion, task.Status);
        Assert.NotNull(results.Process);
        Assert.True(results.Process.HasExited);
        Assert.NotNull(results.StandardOutput);
        Assert.NotNull(results.StandardError);
        Assert.Equal(expectedStandardOutput, results.StandardOutput);
        Assert.Equal(expectedStandardError, results.StandardError);
        Assert.Equal(ExpectedExitCode, results.ExitCode);
        Assert.Equal(ExpectedExitCode, results.Process.ExitCode);
        Assert.True(results.RunTime.TotalMilliseconds >= MillisecondsToSleep);
    }

    [Fact]
    public void RunLotsOfOutputForPeriod()
    {
        // When this problem manifested with the older code, it would normally
        // trigger in this test within 5 to 10 seconds, so if it can run for
        // a full minute and not cause the output-truncation issue, we are probably fine.
        var oneMinute = TimeSpan.FromMinutes(1);
        for (var stopwatch = Stopwatch.StartNew(); stopwatch.Elapsed < oneMinute;)
            _ = Parallel.ForEach(Enumerable.Range(1, 100), _ => WhenProcessReturnsLotsOfOutput_AllOutputCapturedCorrectly());
    }

    private readonly Random _random = new();

    [Fact]
    public async Task WhenProcessReturnsLotsOfOutput_AllOutputCapturedCorrectly()
    {
        // Arrange
        const int ExpectedExitCode = 123;
        const int MillisecondsToSleep = 0; // We want the process to exit right after printing the lines, so no wait time.
        var expectedStandardOutputLineCount = _random.Next(1_000, 100_000);
        var expectedStandardErrorLineCount = _random.Next(1_000, 100_000);
        var expectedStandardOutput = InitializeArray("output", expectedStandardOutputLineCount);
        var expectedStandardError = InitializeArray("error", expectedStandardErrorLineCount);

        // Act
        var processStartInfo = DummyStartProcessArgs(ExpectedExitCode, MillisecondsToSleep, expectedStandardOutputLineCount, expectedStandardErrorLineCount);
        processStartInfo.CreateNoWindow = true;
        var task = ProcessEx.RunAsync(processStartInfo);
        using var results = await task;

        // Assert
        Assert.NotNull(task);
        Assert.NotNull(results);
        Assert.Equal(TaskStatus.RanToCompletion, task.Status);
        Assert.NotNull(results.Process);
        Assert.True(results.Process.HasExited);
        Assert.NotNull(results.StandardOutput);
        Assert.NotNull(results.StandardError);
        Assert.Equal(ExpectedExitCode, results.ExitCode);
        Assert.Equal(ExpectedExitCode, results.Process.ExitCode);
        Assert.True(results.RunTime.TotalMilliseconds >= MillisecondsToSleep);
        Assert.Equal(expectedStandardOutputLineCount, results.StandardOutput.Length);
        Assert.Equal(expectedStandardErrorLineCount, results.StandardError.Length);
        Assert.Equal(expectedStandardOutput, results.StandardOutput);
        Assert.Equal(expectedStandardError, results.StandardError);
    }

    [Fact]
    public void WhenProcessTimesOut_TaskIsCanceled()
    {
        // Arrange
        const int ExpectedExitCode = 123;
        const int MillisecondsToSleep = 5_000;
        const int ExpectedStandardOutputLineCount = 5;
        const int ExpectedStandardErrorLineCount = 3;
        const int MillisecondsForTimeout = 3_000;

        // Act
        var processStartInfo = DummyStartProcessArgs(ExpectedExitCode, MillisecondsToSleep, ExpectedStandardOutputLineCount, ExpectedStandardErrorLineCount);
        using var cts = new CancellationTokenSource(MillisecondsForTimeout);
        var cancellationToken = cts.Token;
        var task = ProcessEx.RunAsync(processStartInfo, cancellationToken);

        // Assert
        Assert.NotNull(task);
        var aggregateException = Assert.Throws<AggregateException>(() => task.Result);
        var innerException = Assert.Single(aggregateException.InnerExceptions);
        var canceledException = Assert.IsType<TaskCanceledException>(innerException);
        Assert.NotNull(canceledException);
        Assert.True(cancellationToken.IsCancellationRequested);
        Assert.Equal(TaskStatus.Canceled, task.Status);
    }
}
