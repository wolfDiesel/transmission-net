using System.Diagnostics;

namespace TransmissonNET.App.Desktop;

internal static class DesktopWindowActivator
{
    public static void TryActivate()
    {
        if (!OperatingSystem.IsLinux())
            return;

        _ = Run("wmctrl", "-x", "TransmissionNET");
        _ = Run("wmctrl", "-a", "TransmissionNET");
        _ = Run("wmctrl", "-R", "TransmissionNET");
    }

    private static int Run(string fileName, params string[] args)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            foreach (var arg in args)
                process.StartInfo.ArgumentList.Add(arg);

            if (!process.Start())
                return 1;

            process.WaitForExit(500);
            return process.ExitCode;
        }
        catch
        {
            return 1;
        }
    }
}
