namespace TransmissonNET.Domain;

public sealed record DaemonConnection(
    string Host,
    int Port,
    string RpcPath,
    string Username,
    string Password)
{
    public string RpcUrl => $"http://{Host}:{Port}{RpcPath}";
}
