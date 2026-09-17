using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using KoneMoshapoRetail.Models;
using KoneMoshapoRetail.Services;

namespace KoneMoshapoRetail.Functions
{
    public class LogFileFunction
    {
        private readonly IFileStorageService _fileService;
        private readonly ILogger<LogFileFunction> _logger;

        public LogFileFunction(IFileStorageService fileService, ILogger<LogFileFunction> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        [FunctionName("WriteLogFile")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("Writing log file to Azure Files...");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var logEntry = JsonConvert.DeserializeObject<LogEntry>(requestBody);

            if (logEntry == null)
            {
                return new BadRequestObjectResult("Please pass a valid log entry.");
            }

            var fileName = $"log-{DateTime.UtcNow:yyyyMMddHHmmss}.txt";
            var content = logEntry.ToFormattedString();

            var result = await _fileService.UploadLogFileAsync(fileName, content);

            return result
                ? (ActionResult)new OkObjectResult($"Log file {fileName} created successfully.")
                : new BadRequestObjectResult("Failed to create log file.");
        }
    }
}