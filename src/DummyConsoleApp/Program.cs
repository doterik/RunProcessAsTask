namespace DummyConsoleApp;

public static class Program
{
    private static void Main(string[] args)
    {
#pragma warning disable MA0011 // IFormatProvider is missing.
        var exitCodeToReturn = int.Parse(args[0]);
        var millisecondsToSleep = int.Parse(args[1]);
        var linesOfStandardOutput = int.Parse(args[2]);
        var linesOfStandardError = int.Parse(args[3]);
#pragma warning restore MA0011 // IFormatProvider is missing.

        Thread.Sleep(millisecondsToSleep);

        for (var i = 0; i < linesOfStandardOutput; i++) Console.WriteLine($"Standard output line #{i + 1}");
        for (var i = 0; i < linesOfStandardError; i++) Console.Error.WriteLine($"Standard error line #{i + 1}");

        Environment.Exit(exitCodeToReturn);
    }
}
