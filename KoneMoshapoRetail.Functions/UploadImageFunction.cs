using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using KoneMoshapoRetail.Services;

namespace KoneMoshapoRetail.Functions
{
    public class UploadImageFunction
    {
        private readonly IBlobStorageService _blobService;
        private readonly ILogger<UploadImageFunction> _logger;

        public UploadImageFunction(IBlobStorageService blobService, ILogger<UploadImageFunction> logger)
        {
            _blobService = blobService;
            _logger = logger;
        }

        [FunctionName("UploadImage")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("Uploading image to Blob Storage...");

            var file = req.Form.Files["image"];
            if (file == null || file.Length == 0)
            {
                return new BadRequestObjectResult("Please upload an image file using form-data with key 'image'.");
            }

            var imageUrl = await _blobService.UploadImageAsync(file);

            return new OkObjectResult($"Image uploaded successfully. URL: {imageUrl}");
        }
    }
}