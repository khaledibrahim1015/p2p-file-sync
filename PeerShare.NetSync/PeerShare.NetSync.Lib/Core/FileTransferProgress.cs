namespace PeerShare.NetSync.Lib.Core;

public class FileTransferProgress
{
    public string FileName { get; set; }
    public long BytesTransfered { get; set; }
    public long TotalBytes { get; set; }

    public double ProgressPercentage => (double)BytesTransfered / TotalBytes * 100;


}
