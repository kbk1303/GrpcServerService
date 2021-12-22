using GrpcServiceTest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebTest.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private TransferService trService = new TransferService(null);

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async void OnGet()
        {
            MemoryStream s = await trService.GetStreamFromFile("./videos/video.mp4");
            //return await Task.FromResult(new MemoryStream(buffer.Result));
            string strBuffer = "data:video/mp4;base64," + System.Convert.ToBase64String(ReadFully(s));
            _logger.LogDebug("substring {0}", strBuffer.Substring(0, 100));
            Debug.WriteLine("substring {0}", strBuffer.Substring(0, 100));
            ViewData["Buffer"] = strBuffer;
            ViewData["DebugData"] = strBuffer.Substring(0, 200) + "\n" + strBuffer.Substring(201, 500);
            MemoryStream img = await trService.GetStreamFromFile("./images/image.jpg");
            string ImgBuffer = "data:image/jpg;base64," + System.Convert.ToBase64String(ReadFully(img));
            ViewData["Image"] = ImgBuffer;
        }
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
