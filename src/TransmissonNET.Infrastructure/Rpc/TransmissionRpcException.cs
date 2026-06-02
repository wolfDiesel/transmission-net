namespace TransmissonNET.Infrastructure.Rpc;

public sealed class TransmissionRpcException : Exception
{
    public TransmissionRpcException(string message) : base(message)
    {
    }

    public TransmissionRpcException(string message, Exception inner) : base(message, inner)
    {
    }
}
