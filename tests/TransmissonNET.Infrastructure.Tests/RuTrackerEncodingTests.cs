using System.Text;
using TransmissonNET.Providers.RuTracker;
using Xunit;

namespace TransmissonNET.Infrastructure.Tests;

public sealed class RuTrackerEncodingTests
{
    [Fact]
    public void Decode_Windows1251_DoesNotThrow()
    {
        var text = "Привет windows-1251";
        var bytes = new Windows1251Encoding().GetBytes(text);

        var decoded = RuTrackerEncoding.Decode(bytes, "windows-1251");

        Assert.Equal(text, decoded);
    }

    [Fact]
    public void CreateFormUrlEncoded_EncodesCyrillicAsPercentBytes()
    {
        using var content = RuTrackerEncoding.CreateFormUrlEncoded(
        [
            new KeyValuePair<string, string>("login", "Вход"),
        ]);

        var bytes = content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
        var ascii = Encoding.ASCII.GetString(bytes);

        Assert.StartsWith("login=", ascii);
        Assert.Contains('%', ascii);
    }
}
