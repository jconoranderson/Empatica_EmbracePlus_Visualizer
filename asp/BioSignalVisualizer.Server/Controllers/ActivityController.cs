using BioSignalVisualizer.Server.Services;
using BioSignalVisualizer.Shared;
using Microsoft.AspNetCore.Mvc;

namespace BioSignalVisualizer.Server.Controllers;

[ApiController]
[Route("api/activity")]
public sealed class ActivityController : ControllerBase
{
    private readonly ActivityWindowService _activityWindowService;

    public ActivityController(ActivityWindowService activityWindowService)
    {
        _activityWindowService = activityWindowService;
    }

    [HttpGet]
    public async Task<IEnumerable<ActivityWindow>> Get(
        [FromQuery] string date,
        [FromQuery] string participant,
        CancellationToken cancellationToken)
        => await _activityWindowService.GetWindowsAsync(date, participant, cancellationToken);
}
