using PeerShare.NetSync.Lib.Core;
using PeerShare.NetSync.Lib.Helper;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;

namespace PeerShare.NetSync.Lib.Services;

public class SenderService
{
    private const string ResumeCommand = "RESUME:";
    private const int ChunkSize = 1024 * 8; // 8kb

    /// <summary>
    /// if resume Resume:{resumePosition == BytesTransferred}
    /// if ack just one byte 
    /// </summary>
    private const int AckOrResumeSize = 1024;

    private static readonly ConcurrentDictionary<string, FileTransferProgress> _transfers = new ConcurrentDictionary<string, FileTransferProgress>();

    public static async Task SendMessageAsync(NetworkStream stream, string message)
    {
        using (stream)
        {

            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(buffer, 0, buffer.Length);
            Console.WriteLine($"Message sent: {message}");

        }
    }
    public static async Task SendFileAsync(NetworkStream stream, string filePath)
    {
        using (stream) ;
        using Stream fileStream = File.OpenRead(filePath);
        FileInfo fileInfo = new FileInfo(filePath);


        //1-  send metadata to receiver Peer 
        FileMetaData metaData = new FileMetaData()
        {
            FileName = fileInfo.Name,
            FileSize = fileInfo.Length,
            CheckSum = await FileHelper.CalculateCheckSum(filePath)
        };

        string metaDataString = $"FILE:{metaData.FileName}|{metaData.FileSize}|{metaData.CheckSum}";
        byte[] bufferMetadataBytes = Encoding.UTF8.GetBytes(metaDataString);
        await stream.WriteAsync(bufferMetadataBytes, 0, bufferMetadataBytes.Length);

        //  here summary for what happen In Acknowledgment
        //  in first time sender send request to reciver if file exist and interrupted read last index in file and send back to reciver with:
        //  "Resume:{resumePosition == BytesTransferred}"
        //  if it first time and there is no inturupt in file we did not fount previous commad just one byte with value one 

        // wait for acknowledgment or resume Request 
        byte[] bufferAckOrResumeResponse = new byte[AckOrResumeSize];
        int bytesRead = await stream.ReadAsync(bufferAckOrResumeResponse, 0, bufferAckOrResumeResponse.Length);
        string response = Encoding.UTF8.GetString(bufferAckOrResumeResponse, 0, bytesRead).TrimEnd('\0');

        long startPosition = 0;
        if (response.StartsWith(ResumeCommand))
        {
            // start from end of inturupted point => start read from file from bytestransfered 
            //  get BytesTransferred
            startPosition = long.Parse(response.Substring(ResumeCommand.Length));
            fileStream.Seek(startPosition, SeekOrigin.Begin);
        }

        FileTransferProgress progress = new FileTransferProgress()
        {
            FileName = metaData.FileName,
            TotalBytes = metaData.FileSize,
            BytesTransfered = startPosition
        };
        _transfers[metaData.FileName] = progress;


        // start process send file 
        byte[] buffer = new byte[ChunkSize];
        int bytesReadFromFile;
        while ((bytesReadFromFile = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await stream.WriteAsync(buffer, 0, bytesReadFromFile);
            progress.BytesTransfered += bytesReadFromFile;
            FileHelper.ReportProgress(progress);

            // savestate in sender localmachine 
            FileTransferState.SaveState(new FileTransferState()
            {
                FileName = metaData.FileName,
                TotalBytes = metaData.FileSize,
                BytesTransferred = progress.BytesTransfered,
                CheckSum = metaData.CheckSum
            });
        }

        // clear  
        await stream.FlushAsync();
        Console.WriteLine($"\nFile sent: {filePath}");
        _transfers.TryRemove(metaData.FileName, out _);
        FileTransferState.DeleteState(metaData.FileName);
    }




}
