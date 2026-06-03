using System.Text;

namespace TransmissonNET.Infrastructure.Desktop;

internal static class DesktopFileEncoding
{
    public static Encoding Instance { get; } = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
}
