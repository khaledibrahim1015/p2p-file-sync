using PeerShare.NetSync.Lib.Core;
using PeerShare.NetSync.Lib.Helper;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;

namespace PeerShare.NetSync.Lib.Services;

public class ReceiverService
{
    private const string DefaultPath = "C:\\Users\\ZALL-TECH\\Desktop\\P2PDistributedFileTransfer\\data\\ReceiverData";
    private const string ResumeCommand = "RESUME:";
    private const int ChunkSize = 1024 * 8; // 8kb


    private static readonly ConcurrentDictionary<string, FileTransferProgress> _transfers = new ConcurrentDictionary<string, FileTransferProgress>();

    public static async Task ReceiveFileAsync(NetworkStream stream, string metaDataString)
    {
        //  here recive request and start validation 
        //  validation check if filetransferstate exist before or not 
        //  if state exist send resume if not send Ack

        try
        {
            FileMetaData metaData = FileHelper.ParseFileMetaData(metaDataString);
            FileTransferState existingState = FileTransferState.LoadState(metaData.FileName);

            // if exist then send resume command else it is first time to connect 

            // send Resume Command 
            long resumePosition = 0;
            if (existingState != null && metaData.CheckSum == existingState.CheckSum)
            {
                resumePosition = existingState.BytesTransferred;
                string resumeCommand = $"{ResumeCommand}{existingState.BytesTransferred}";
                byte[] bufferResumeBytes = Encoding.UTF8.GetBytes(resumeCommand);
                await stream.WriteAsync(bufferResumeBytes, 0, bufferResumeBytes.Length);
            }
            else
            {
                // Send acknowledgment
                await stream.WriteAsync(new byte[] { 1 }, 0, 1);
            }



            FileTransferProgress progress = new FileTransferProgress()
            {
                FileName = metaData.FileName,
                TotalBytes = metaData.FileSize,
                BytesTransfered = resumePosition
            };
            _transfers[metaData.FileName] = progress;


            //  start receive file 
            string filePath = Path.Combine(DefaultPath, metaData.FileName);
            using Stream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            fileStream.Seek(resumePosition, SeekOrigin.Begin);

            byte[] buffer = new byte[ChunkSize];
            int bytesRead;
            while (progress.BytesTransfered < metaData.FileSize)
            {
                bytesRead = await stream.ReadAsync(buffer, 0, Math.Min(buffer.Length, (int)(metaData.FileSize - progress.BytesTransfered)));
                if (bytesRead == 0) break; // Connection closed prematurely

                await fileStream.WriteAsync(buffer, 0, bytesRead);
                progress.BytesTransfered += bytesRead;
                FileHelper.ReportProgress(progress);

                FileTransferState state = new FileTransferState
                {
                    FileName = metaData.FileName,
                    TotalBytes = metaData.FileSize,
                    BytesTransferred = progress.BytesTransfered,
                    CheckSum = metaData.CheckSum
                };


                FileTransferState.SaveState(state);


            }

            await fileStream.FlushAsync();

            if (progress.BytesTransfered == metaData.FileSize)
            {
                Console.WriteLine($"\nFile received: {metaData.FileName}");
                _transfers.TryRemove(metaData.FileName, out _);
                FileTransferState.DeleteState(metaData.FileName);

                // Verify checksum
                string receivedChecksum = await FileHelper.CalculateCheckSum(filePath);
                if (receivedChecksum != metaData.CheckSum)
                {
                    Console.WriteLine("Warning: Received file checksum does not match. The file may be corrupted.");
                }
            }
            else
            {
                Console.WriteLine($"Warning: Received {progress.BytesTransfered} bytes, expected {metaData.FileSize} bytes. Transfer interrupted.");
            }
        }
        catch (Exception ex)
        {

            Console.WriteLine($"Error receiving file: {ex.Message}");
        }



    }


}
