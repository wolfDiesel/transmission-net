using System.Text.Json;

namespace TransmissonNET.Infrastructure.Rpc;

internal static class RpcJsonReader
{
    public static long GetInt64(JsonElement item, string camelName, string? snakeName = null) =>
        TryGetProperty(item, camelName, snakeName, out var value) ? value.GetInt64() : 0;

    public static int GetInt32(JsonElement item, string camelName, string? snakeName = null) =>
        TryGetProperty(item, camelName, snakeName, out var value) ? value.GetInt32() : 0;

    public static double GetDouble(JsonElement item, string camelName, string? snakeName = null) =>
        TryGetProperty(item, camelName, snakeName, out var value) ? value.GetDouble() : 0;

    public static string GetString(JsonElement item, string camelName, string? snakeName = null) =>
        TryGetProperty(item, camelName, snakeName, out var value) ? value.GetString() ?? string.Empty : string.Empty;

    public static bool GetBoolean(JsonElement item, string camelName, string? snakeName = null) =>
        TryGetProperty(item, camelName, snakeName, out var value) && value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => value.GetInt32() != 0,
            _ => false,
        };

    public static bool TryGetProperty(
        JsonElement item,
        string camelName,
        string? snakeName,
        out JsonElement value)
    {
        if (item.TryGetProperty(camelName, out value))
            return true;

        if (snakeName is not null && item.TryGetProperty(snakeName, out value))
            return true;

        value = default;
        return false;
    }
}
