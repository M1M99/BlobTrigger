using System.IO;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

public static class CompressPngFunction
{
    [FunctionName("CompressPngImage")]
    public static void Run(
        [Microsoft.Azure.Functions.Worker.BlobTrigger("input-images/{name}.png", Connection = "AzureWebJobsStorage")] Stream inputBlob,
        [Blob("compressed-images/{name}.png", FileAccess.Write, Connection = "AzureWebJobsStorage")] Stream outputBlob,
        string name,
        ILogger log)
    {
        log.LogInformation($"Compressing PNG image: {name}.png");

        using var image = Image.Load(inputBlob);

        var encoder = new PngEncoder
        {
            CompressionLevel = PngCompressionLevel.BestCompression,
            ColorType = PngColorType.Rgb 
        };

        image.SaveAsPng(outputBlob, encoder);

        log.LogInformation($"Compressed PNG saved: {name}.png");
    }
}



