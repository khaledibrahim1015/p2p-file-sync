using PeerShare.NetSync.Lib;
using PeerShare.NetSync.Lib.Enums;

namespace PeerShare.NetSync.Main;

public class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Enter Role (sender/receiver):");
        string roleInput = Console.ReadLine().ToLower();

        PeerRole role = roleInput == "sender" ? PeerRole.Sender : PeerRole.Receiver;

        var peer = new Peer(role);


        if (role == PeerRole.Sender)
        {
            Console.Write("Enter receiver's IP address: ");
            var receiverIp = Console.ReadLine();
            await peer.StartAsync(receiverIp);
        }
        else
        {
            await peer.StartAsync();
        }
        while (true)
        {
            if (role == PeerRole.Sender)
            {
                Console.WriteLine("\n1. Send Message");
                Console.WriteLine("2. Send File");
                Console.Write("Choose an option: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Enter message: ");
                        var message = Console.ReadLine();
                        await peer.SendMessageAsync(message);
                        break;
                    case "2":
                        Console.Write("Enter file path: ");
                        var filePath = Console.ReadLine();
                        await peer.SendFileAsync(filePath);
                        break;

                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Receiver is waiting for incoming connections and files...");
            }
        }
    }
}
