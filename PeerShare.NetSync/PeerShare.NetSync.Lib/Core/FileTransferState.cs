using System.Text.Json;

namespace PeerShare.NetSync.Lib.Core;

public class FileTransferState
{
    private const string StateDirectory = "C:\\Users\\ZALL-TECH\\Desktop\\P2PDistributedFileTransfer\\data\\temp";

    public string FileName { get; set; }
    public long TotalBytes { get; set; }
    public long BytesTransferred { get; set; }
    public string CheckSum { get; set; }

    public static void SaveState(FileTransferState state)
    {
        if (!Directory.Exists(StateDirectory))
        {
            Directory.CreateDirectory(StateDirectory);
        }

        string filePath = Path.Combine(StateDirectory, $"{state.FileName}.json");
        string json = JsonSerializer.Serialize(state);
        File.WriteAllText(filePath, json);
    }
    public static void DeleteState(string fileName)
    {
        string filePath = Path.Combine(StateDirectory, $"{fileName}.json");
        if (File.Exists(filePath))
            File.Delete(filePath);
    }







}
