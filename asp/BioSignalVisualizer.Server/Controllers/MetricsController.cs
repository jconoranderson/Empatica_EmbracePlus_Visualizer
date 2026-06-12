using BioSignalVisualizer.Server.Services;
using BioSignalVisualizer.Shared;
using Microsoft.AspNetCore.Mvc;

namespace BioSignalVisualizer.Server.Controllers;

[ApiController]
[Route("api/metrics")]
public sealed class MetricsController : ControllerBase
{
    private readonly MetricLoader _metricLoader;

    public MetricsController(MetricLoader metricLoader)
    {
        _metricLoader = metricLoader;
    }

    [HttpGet]
    public async Task<ActionResult<MetricSeries>> Get(
        [FromQuery] string date,
        [FromQuery] string participant,
        [FromQuery] string metric,
        CancellationToken cancellationToken)
    {
        var result = await _metricLoader.LoadMetricAsync(date, participant, metric, cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        return result;
    }
}
