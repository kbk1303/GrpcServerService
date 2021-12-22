using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace GrpcServiceTest
{
    public class TransferService : Transfer.TransferBase
    {
        private readonly ILogger<TransferService> _logger;
        public TransferService(ILogger<TransferService> logger)
        {
            _logger = logger;
        }


        public override Task<TransferResponse> TransferData(TransferRequest request, ServerCallContext context)
        {
            Console.WriteLine("request from user {0}", request.UserName);
            return Task.FromResult(new TransferResponse { LoggedIn = true, UserId = request.UserName + "001" });
        }

        public override async Task FetchVideo(
            IAsyncStreamReader<VideoRequest> requestStream,
            IServerStreamWriter<VideoResponse> responseStream,
            ServerCallContext context)
        {
            var alive = ClientToServerMessageAsync(requestStream, context);
            var streaming = ServerStreamAsync(responseStream, context);
            await Task.WhenAll(alive, streaming);
        }



        private async Task ServerStreamAsync(IServerStreamWriter<VideoResponse> responseStream, ServerCallContext context)
        {
            Stream video = await GetStreamFromFile("./videos/video.mp4");
            const int BUFFER_SIZE = 8192;
            byte[] buffer = new byte[BUFFER_SIZE];
            int loop = 0;
            int read;
            
            while((read = video.Read(buffer, 0, BUFFER_SIZE)) > 0) {
                loop++;
                await responseStream.WriteAsync(new VideoResponse
                {
                    Buffer = ByteString.CopyFrom(buffer, 0, read),
                    Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                    Base64String = System.Convert.ToBase64String(buffer, 0, read)
                });
                
            }
            
            
            Console.WriteLine("buffered");
            
        }

        private async Task ClientToServerMessageAsync(IAsyncStreamReader<VideoRequest> requestStream, ServerCallContext context)
        {
            bool alive = true;
            while (await requestStream.MoveNext() && !context.CancellationToken.IsCancellationRequested)
            {
                var message = requestStream.Current;
                _logger.LogInformation("message from client {0}", message.Alive);
                alive = message.Alive;
            }

        }

        public async Task<MemoryStream> GetStreamFromFile(string fullPath)
        {
            Task<byte[]> buffer = File.ReadAllBytesAsync(fullPath);
            Console.WriteLine("length of File in bytes {0}", buffer.Result.Length);
            return await Task.FromResult(new MemoryStream(buffer.Result));

        }


        private async Task<MemoryStream> GetStreamFromUrl()
        {
         HttpClient httpClient = new HttpClient();
            ManualResetEvent evnts = new(false);
            byte[] imageData = null;
            var wc = new System.Net.WebClient();
           
            //wc.DownloadDataAsync(new Uri("./video.mp4"));
            wc.DownloadDataCompleted +=
            delegate (object sender, DownloadDataCompletedEventArgs e)
            {
                imageData = e.Result;
                evnts.Set();
            };

            evnts.Reset();
            wc.DownloadDataAsync(new Uri("file:///video.mp4"));
            evnts.WaitOne(); // wait to download complete
            Console.WriteLine("stream ready {0}", imageData.Length);
            return await Task.FromResult(new MemoryStream(imageData));
            //var imageData = await httpClient.GetByteArrayAsync("https://www.youtube.com/watch?v=wY4nMSUF9e0");


        }

    }
}
