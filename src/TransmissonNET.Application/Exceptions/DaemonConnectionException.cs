namespace TransmissonNET.Application.Exceptions;

public sealed class DaemonConnectionException : Exception
{
    public DaemonConnectionException(string message) : base(message)
    {
    }

    public DaemonConnectionException(string message, Exception inner) : base(message, inner)
    {
    }
}
