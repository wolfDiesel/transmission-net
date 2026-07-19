using System.Text;

namespace TransmissonNET.Providers.Kinozal;

internal static class KinozalEncoding
{
    private static readonly Encoding Windows1251 = new Windows1251Encoding();

    public static Encoding Default => Windows1251;

    public static Encoding Resolve(string? charset)
    {
        if (string.IsNullOrWhiteSpace(charset))
            return Windows1251;

        var name = charset.Trim().Trim('"', '\'');
        if (name.Equals("utf-8", StringComparison.OrdinalIgnoreCase)
            || name.Equals("utf8", StringComparison.OrdinalIgnoreCase))
            return Encoding.UTF8;

        return Windows1251;
    }

    public static string Decode(byte[] bytes, string? charset) =>
        Resolve(charset).GetString(bytes);

    public static ByteArrayContent CreateFormUrlEncoded(IEnumerable<KeyValuePair<string, string>> fields)
    {
        var parts = new List<string>();
        foreach (var (key, value) in fields)
            parts.Add($"{UrlEncode(key)}={UrlEncode(value)}");

        var content = new ByteArrayContent(Windows1251.GetBytes(string.Join("&", parts)));
        content.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
        return content;
    }

    private static string UrlEncode(string value)
    {
        var bytes = Windows1251.GetBytes(value);
        var sb = new StringBuilder(bytes.Length * 3);
        foreach (var b in bytes)
        {
            if ((b >= 'a' && b <= 'z')
                || (b >= 'A' && b <= 'Z')
                || (b >= '0' && b <= '9')
                || b is (byte)'-' or (byte)'_' or (byte)'.' or (byte)'*')
            {
                sb.Append((char)b);
            }
            else if (b == (byte)' ')
            {
                sb.Append('+');
            }
            else
            {
                sb.Append('%');
                sb.Append(b.ToString("X2"));
            }
        }

        return sb.ToString();
    }
}

internal sealed class Windows1251Encoding : Encoding
{
    private static readonly char[] ByteToChar =
    [
        '\u0402', '\u0403', '\u201A', '\u0453', '\u201E', '\u2026', '\u2020', '\u2021',
        '\u20AC', '\u2030', '\u0409', '\u2039', '\u040A', '\u040C', '\u040B', '\u040F',
        '\u0452', '\u2018', '\u2019', '\u201C', '\u201D', '\u2022', '\u2013', '\u2014',
        '\u0098', '\u2122', '\u0459', '\u203A', '\u045A', '\u045C', '\u045B', '\u045F',
        '\u00A0', '\u040E', '\u045E', '\u0408', '\u00A4', '\u0490', '\u00A6', '\u00A7',
        '\u0401', '\u00A9', '\u0404', '\u00AB', '\u00AC', '\u00AD', '\u00AE', '\u0407',
        '\u00B0', '\u00B1', '\u0406', '\u0456', '\u0491', '\u00B5', '\u00B6', '\u00B7',
        '\u0451', '\u2116', '\u0454', '\u00BB', '\u0458', '\u0405', '\u0455', '\u0457',
        '\u0410', '\u0411', '\u0412', '\u0413', '\u0414', '\u0415', '\u0416', '\u0417',
        '\u0418', '\u0419', '\u041A', '\u041B', '\u041C', '\u041D', '\u041E', '\u041F',
        '\u0420', '\u0421', '\u0422', '\u0423', '\u0424', '\u0425', '\u0426', '\u0427',
        '\u0428', '\u0429', '\u042A', '\u042B', '\u042C', '\u042D', '\u042E', '\u042F',
        '\u0430', '\u0431', '\u0432', '\u0433', '\u0434', '\u0435', '\u0436', '\u0437',
        '\u0438', '\u0439', '\u043A', '\u043B', '\u043C', '\u043D', '\u043E', '\u043F',
        '\u0440', '\u0441', '\u0442', '\u0443', '\u0444', '\u0445', '\u0446', '\u0447',
        '\u0448', '\u0449', '\u044A', '\u044B', '\u044C', '\u044D', '\u044E', '\u044F',
    ];

    private static readonly Dictionary<char, byte> CharToByte = CreateCharToByte();

    public override string EncodingName => "windows-1251";
    public override string WebName => "windows-1251";
    public override int CodePage => 1251;

    public override int GetByteCount(char[] chars, int index, int count) => count;

    public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
    {
        for (var i = 0; i < charCount; i++)
        {
            var ch = chars[charIndex + i];
            bytes[byteIndex + i] = ch <= 0x7F
                ? (byte)ch
                : CharToByte.TryGetValue(ch, out var b) ? b : (byte)0x3F;
        }

        return charCount;
    }

    public override int GetCharCount(byte[] bytes, int index, int count) => count;

    public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
    {
        for (var i = 0; i < byteCount; i++)
        {
            var b = bytes[byteIndex + i];
            chars[charIndex + i] = b <= 0x7F ? (char)b : ByteToChar[b - 0x80];
        }

        return byteCount;
    }

    public override int GetMaxByteCount(int charCount) => charCount;
    public override int GetMaxCharCount(int byteCount) => byteCount;

    private static Dictionary<char, byte> CreateCharToByte()
    {
        var map = new Dictionary<char, byte>(ByteToChar.Length);
        for (var i = 0; i < ByteToChar.Length; i++)
            map[ByteToChar[i]] = (byte)(0x80 + i);
        return map;
    }
}
