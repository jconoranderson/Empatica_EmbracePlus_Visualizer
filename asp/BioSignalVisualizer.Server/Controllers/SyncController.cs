using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using BioSignalVisualizer.Shared;
using Microsoft.Extensions.Options;

namespace BioSignalVisualizer.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly ILogger<SyncController> _logger;
    private readonly string _baseDataPath;

    public SyncController(ILogger<SyncController> logger, IOptions<VisualizerSettings> settings)
    {
        _logger = logger;
        _baseDataPath = settings.Value.BaseDataPath;
    }

    [HttpPost]
    public async Task<IActionResult> SyncData()
    {
        try
        {
            var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var accessKey = config["VisualizerSettings:AwsAccessKey"];
            var secretKey = config["VisualizerSettings:AwsSecretKey"];

            var processInfo = new ProcessStartInfo
            {
                FileName = "aws",
                Arguments = $"s3 sync s3://empatica-us-east-1-prod-data/v2/2414/1/1/participant_data/ \"{_baseDataPath}\" --delete --no-progress --exclude \"*/raw_data/*\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (!string.IsNullOrWhiteSpace(accessKey) && !string.IsNullOrWhiteSpace(secretKey))
            {
                processInfo.EnvironmentVariables["AWS_ACCESS_KEY_ID"] = accessKey;
                processInfo.EnvironmentVariables["AWS_SECRET_ACCESS_KEY"] = secretKey;
            }
            else
            {
                processInfo.Arguments += " --profile empatica";
            }

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                return StatusCode(500, "Could not start AWS process.");
            }

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await Task.WhenAll(process.WaitForExitAsync(), outputTask, errorTask);
            
            var output = await outputTask;
            var error = await errorTask;

            if (process.ExitCode != 0)
            {
                _logger.LogError($"AWS Sync Error: {error}");
                return StatusCode(500, $"Sync failed: {error}");
            }

            return Ok(new { Output = output });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync data");
            return StatusCode(500, ex.Message);
        }
    }
}
