using PeerShare.NetSync.Lib.Configuration;
using PeerShare.NetSync.Lib.Enums;
using PeerShare.NetSync.Lib.Helper;
using PeerShare.NetSync.Lib.Services;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PeerShare.NetSync.Lib;

public class Peer
{
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private readonly ConcurrentDictionary<PeerConnectionInfo, ConnectionManager> _connections = new ConcurrentDictionary<PeerConnectionInfo, ConnectionManager>();
    private PeerConnectionInfo _peerConnectionInfo;


    public PeerRole Role { get; set; }
    private string PeerIpAddress = string.Empty;
    private int Port = 5000;
    private const int ChunkSize = 8 * 1024; // 8KB
    public Peer(PeerRole role)
    {
        Role = role;
    }

    public async Task StartAsync(string peerIpAddress = null)
    {
        if (Role == PeerRole.Receiver)
            await StartListenerAsync();  //  act as a server 
        else if (Role == PeerRole.Sender && !string.IsNullOrEmpty(peerIpAddress))
        {
            InitializePeerConnectionInfo(peerIpAddress);
            await ConnectToPeerAsync(peerIpAddress);
        }
    }

    private void InitializePeerConnectionInfo(string peerIpAddress)
    {
        PeerIpAddress = peerIpAddress;
        _peerConnectionInfo = new PeerConnectionInfo
        {
            IpAddress = peerIpAddress,
            Port = Port,
            Role = Role
        };
    }

    private async Task StartListenerAsync()
    {
        Port = IPEndpointChecker.IsPortInUse(Port) == false
                         ? Port : IPEndpointChecker.FindAvailablePort(5001, 2);
        using TcpListener tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        Console.WriteLine($"Reciver Peer Listening On Port {Port}");
        while (!_cts.IsCancellationRequested)
        {
            TcpClient client = await tcpListener.AcceptTcpClientAsync();
            _ = HandleIncomingConnectionAsync(client);

        }




    }

    private async Task HandleIncomingConnectionAsync(TcpClient client)
    {
        //  two main ( handle Receive file or handle message )

        //  1 - open stream with client 
        using NetworkStream networkStream = client.GetStream();

        //  2- reading fom stream with chunk
        byte[] buffer = new byte[ChunkSize];

        while (!_cts.IsCancellationRequested)
        {
            //  read from stream into buffer (byte[])
            int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead == 0)
                break;

            //  convert buffer to check if messgae or recive file to start process it 
            string message = Encoding.UTF8.GetString(buffer).TrimEnd('\0');//  to ignore un filled message 

            if (message.StartsWith("FILE:"))
            {
                //  Get All MetaData from message (Protocol impelemented )
                // "FILE:FileName|FileSize|CheckSum
                string metaData = message.Substring(5);
                await ReceiveFileAsync(networkStream, metaData);
            }
            else
                Console.WriteLine($"Message Received from {client.Client.RemoteEndPoint} : {message}");
        }




    }

    private Task ReceiveFileAsync(NetworkStream networkStream, string metaData)
    {
        throw new NotImplementedException();
    }

    private async Task<bool> EnsureConnectedAsync()
    {
        if (!_connections.TryGetValue(_peerConnectionInfo, out var connectionManager) || !connectionManager.IsConnected)
        {
            Console.WriteLine("Connection lost. Attempting to reconnect...");
            await ConnectToPeerAsync(_peerConnectionInfo.IpAddress);
            return _connections.TryGetValue(_peerConnectionInfo, out connectionManager) && connectionManager.IsConnected;
        }
        return true;
    }

    private async Task ConnectToPeerAsync(string peerIpAddress)
    {
        try
        {
            //  Connect to Peer 
            TcpClient tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(peerIpAddress, Port);
            await Console.Out.WriteLineAsync($"Connect To Receiver on IPEndpoint :{tcpClient.Client.RemoteEndPoint}");

            _connections.TryAdd(_peerConnectionInfo, new ConnectionManager(tcpClient));



        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync($"Failed To Connect to Receiver {ex.Message}");
        }

    }


    public async Task SendMessageAsync(string message)
    {

        if (!await EnsureConnectedAsync())
        {
            Console.WriteLine("Failed to connect to receiver. Message not sent.");
            return;
        }

        if (!_connections.TryGetValue(_peerConnectionInfo, out var connectionManager))
        {
            Console.WriteLine("Not connected to a receiver.");
            return;
        }

        await SenderService.SendMessageAsync(connectionManager.stream, message);

    }

    public async Task SendFileAsync(string path)
    {





    }

}
