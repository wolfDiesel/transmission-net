using System.Text;

namespace TransmissonNET.Application.Torrents;

internal sealed class BencodeReader
{
    private readonly byte[] _data;
    private int _position;

    public BencodeReader(byte[] data) => _data = data;

    public IReadOnlyDictionary<string, object> ReadDictionary()
    {
        Expect('d');
        var dict = new Dictionary<string, object>(StringComparer.Ordinal);

        while (_position < _data.Length && _data[_position] != (byte)'e')
        {
            var key = ReadString();
            var value = ReadValue();
            dict[key] = value;
        }

        Expect('e');
        return dict;
    }

    public IList<object> ReadList()
    {
        Expect('l');
        var list = new List<object>();

        while (_position < _data.Length && _data[_position] != (byte)'e')
            list.Add(ReadValue());

        Expect('e');
        return list;
    }

    public long ReadInteger()
    {
        Expect('i');
        var start = _position;
        while (_position < _data.Length && _data[_position] != (byte)'e')
            _position++;

        var text = Encoding.UTF8.GetString(_data, start, _position - start);
        Expect('e');
        return long.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
    }

    public string ReadString()
    {
        var colon = Array.IndexOf(_data, (byte)':', _position);
        if (colon < 0)
            throw new InvalidDataException("Invalid bencode string length.");

        var lengthText = Encoding.UTF8.GetString(_data, _position, colon - _position);
        if (!int.TryParse(lengthText, out var length) || length < 0)
            throw new InvalidDataException("Invalid bencode string length.");

        _position = colon + 1;
        if (_position + length > _data.Length)
            throw new InvalidDataException("Unexpected end of bencode string.");

        var value = Encoding.UTF8.GetString(_data, _position, length);
        _position += length;
        return value;
    }

    private object ReadValue() =>
        _data[_position] switch
        {
            (byte)'d' => ReadDictionary(),
            (byte)'l' => ReadList(),
            (byte)'i' => ReadInteger(),
            >= (byte)'0' and <= (byte)'9' => ReadString(),
            _ => throw new InvalidDataException($"Unexpected bencode token at {_position}."),
        };

    private void Expect(char token)
    {
        if (_position >= _data.Length || _data[_position] != (byte)token)
            throw new InvalidDataException($"Expected '{token}' at position {_position}.");

        _position++;
    }
}
