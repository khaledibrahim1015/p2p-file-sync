using PeerShare.NetSync.Lib.Core;
using System.Security.Cryptography;

namespace PeerShare.NetSync.Lib.Helper;

public static class FileHelper
{
    private const int ChecksumChunkSize = 8192; // 8kb

    public static FileMetaData ParseFileMetaData(string metaData)
    {
        string[] parts = metaData.Split(new char[] { '|' });
        return new FileMetaData()
        {
            FileName = parts[0],
            FileSize = long.Parse(parts[1]),
            CheckSum = parts[2]
        };
    }

    public static async Task<string> CalculateCheckSum(string filePath)
    {
        using HashAlgorithm md5Hash = MD5.Create();
        using Stream fileStream = File.OpenRead(filePath);

        byte[] buffer = new byte[ChecksumChunkSize];
        int bytesRead;
        do
        {
            bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead > 0)
                md5Hash.TransformBlock(buffer, 0, bytesRead, null, 0);
        } while (bytesRead != 0);

        md5Hash.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        return BitConverter.ToString(md5Hash.Hash).Replace("-", "").ToLowerInvariant();
    }
    public static void ReportProgress(FileTransferProgress progress)
    {
        Console.WriteLine($"Transferring {progress.FileName}: {progress.ProgressPercentage:F2}% complete");
    }
}
