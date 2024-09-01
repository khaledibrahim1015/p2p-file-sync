using System.Net;
using System.Net.NetworkInformation;

namespace PeerShare.NetSync.Lib.Helper;

public class IPEndpointChecker
{

    public static bool IsPortInUse(int port)
    {
        IPGlobalProperties iPGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

        IPEndPoint[] activeIpsEndointsInTcpProtocols = iPGlobalProperties.GetActiveTcpListeners();
        foreach (IPEndPoint endpoint in activeIpsEndointsInTcpProtocols)
        {
            if (endpoint.Port == port)
                return true; //  port in use 
        }

        IPEndPoint[] activeIpsEndpointsInUdpProtocols = iPGlobalProperties.GetActiveUdpListeners();
        foreach (IPEndPoint endpoint in activeIpsEndpointsInUdpProtocols)
        {
            if (endpoint.Port == port)
                return true; //  port in use 
        }
        return false; //  not in use 
    }

    public static int FindAvailablePort(int preferdPort, int noOfAttemps)
    {
        int maxPort = 65535;  // Maximum port number
        int minPort = 1024;   // Minimum port number (avoid well-known ports)
        Random random = new Random();

        if (!IsPortInUse(preferdPort))
            return preferdPort;

        for (int i = 0; i < noOfAttemps; i++)
        {
            int port = random.Next(minPort, maxPort + 1);
            if (!IsPortInUse(port))
                return port;
        }
        throw new InvalidOperationException($"Attempt {noOfAttemps}: No available ports found in the specified range.");
    }


}
