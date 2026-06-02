namespace TransmissonNET.Application.Exceptions;

public sealed class SettingsValidationException : Exception
{
    public SettingsValidationException(string message) : base(message)
    {
    }
}
