using PeerShare.NetSync.Lib.Enums;

namespace PeerShare.NetSync.Lib.Configuration;

public class PeerConnectionInfo : IEquatable<PeerConnectionInfo>
{
    public string IpAddress { get; set; }
    public int Port { get; set; }
    public PeerRole Role { get; set; }

    public bool Equals(PeerConnectionInfo? other)
    {
        return IpAddress == other.IpAddress && Port == other.Port && Role == other.Role;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(IpAddress, Port, Role);
    }

}
