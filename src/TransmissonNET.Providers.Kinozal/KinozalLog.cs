namespace TransmissonNET.Providers.Kinozal;

internal static class KinozalLog
{
    public static void Info(string message) =>
        Console.Error.WriteLine($"[Kinozal] {message}");

    public static void Error(string message, Exception? ex = null)
    {
        Console.Error.WriteLine($"[Kinozal] ERROR: {message}");
        if (ex is not null)
            Console.Error.WriteLine(ex);
    }
}
