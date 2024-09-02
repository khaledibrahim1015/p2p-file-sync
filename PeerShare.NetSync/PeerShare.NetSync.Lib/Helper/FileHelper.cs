using PeerShare.NetSync.Lib.Core;

namespace PeerShare.NetSync.Lib.Helper;

public static class FileHelper
{

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


}
