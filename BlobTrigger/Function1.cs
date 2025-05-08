using System.IO;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace BlobTrigger
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function(nameof(Function1))]
        public async Task Run([BlobTrigger("images/{name}", Connection = "AzureWebJobsStorage")] Stream stream, string name,
            FunctionContext context)
        {
            using var blobStreamReader = new StreamReader(stream);
            var content = await blobStreamReader.ReadToEndAsync();
            //_logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Data: {content}");

            var metedata = context.BindingContext.BindingData;
            if(metedata.TryGetValue("Uri",out var blobUrl))
            {                                                                                                          
                _logger.LogInformation($"Blob {blobUrl}");
            }

            if (metedata.TryGetValue("Properties", out var properties))
            {
                var obj = JsonConvert.DeserializeObject<BlobItemInfo>(properties.ToString());
                _logger.LogInformation($"{obj.LastModified}");
            }
        }
        [Function("OutputFunction")]
        [BlobOutput("images-original/{name}", Connection = "AzureWebJobsStorage")]
        public byte[] RunOut([BlobTrigger("images/{name}", Connection = "AzureWebJobsStorage")] Stream inputStream, string name)
        {
            _logger.LogInformation("Tested for output function");

            if (name.EndsWith(".png"))
            {
                using (var image = Image.Load(inputStream))
                {
                    image.Mutate(c => c.Resize(500, 500));

                    var encoder = new JpegEncoder
                    {
                        Quality = 50 
                    };

                    using (var ms = new MemoryStream())
                    {
                        image.SaveAsJpeg(ms, encoder); 
                        _logger.LogInformation("PNG processed and converted to JPG");

                        return ms.ToArray(); 
                    }
                }
            }

            return null;
        }
    }
}
