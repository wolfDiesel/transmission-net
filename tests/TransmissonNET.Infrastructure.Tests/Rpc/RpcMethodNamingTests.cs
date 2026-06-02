using TransmissonNET.Infrastructure.Rpc;

namespace TransmissonNET.Infrastructure.Tests.Rpc;

public class RpcMethodNamingTests
{
    [Fact]
    public void UsesSnakeCase_BeforeRpcVersionKnown()
    {
        var naming = new RpcMethodNaming();

        Assert.Equal("session_get", naming.SessionGet);
        Assert.Equal("torrent_get", naming.TorrentGet);
    }
}
