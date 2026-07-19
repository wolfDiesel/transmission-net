namespace TransmissonNET.Providers.LostFilm;

internal static class LostFilmLog
{
    public static void Info(string message) =>
        Console.Error.WriteLine($"[LostFilm] {message}");

    public static void Error(string message, Exception? ex = null)
    {
        if (ex is null)
            Console.Error.WriteLine($"[LostFilm] ERROR {message}");
        else
            Console.Error.WriteLine($"[LostFilm] ERROR {message}: {ex}");
    }
}
