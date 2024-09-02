using System.Net.Sockets;

namespace PeerShare.NetSync.Lib.Services;

public class ConnectionManager
{
    private readonly CancellationTokenSource _cts;
    private readonly TcpClient _client;

    public ConnectionManager(TcpClient client)
    {
        _client = client;
        _cts = new CancellationTokenSource();
    }
    public NetworkStream stream => _client.GetStream();
    public bool IsConnected => _client.Connected;

    public void Close()
    {
        _cts.Cancel();
        _client.Close();
    }

}
