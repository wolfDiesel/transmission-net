using System.Diagnostics;

namespace TransmissonNET.Infrastructure.Desktop;

internal static class DesktopProcessRunner
{
    internal sealed record ProcessResult(int ExitCode, string StdOut, string StdErr);

    public static ProcessResult Run(string fileName, params string[] args)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        foreach (var arg in args)
            process.StartInfo.ArgumentList.Add(arg);

        process.Start();
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return new ProcessResult(process.ExitCode, stdout.Trim(), stderr.Trim());
    }

    public static string? TryRun(string fileName, params string[] args)
    {
        try
        {
            var result = Run(fileName, args);
            return result.ExitCode == 0 ? result.StdOut : null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
