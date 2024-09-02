using System.Net.Sockets;
using System.Text;

namespace PeerShare.NetSync.Lib.Services;

public class SenderService
{
    public static async Task SendMessageAsync(NetworkStream stream, string message)
    {
        using (stream)
        {

            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(buffer, 0, buffer.Length);
            Console.WriteLine($"Message sent: {message}");

        }
    }


}
