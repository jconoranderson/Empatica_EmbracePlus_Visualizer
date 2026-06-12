using BioSignalVisualizer.Server.Services;
using BioSignalVisualizer.Shared;
using Microsoft.AspNetCore.Mvc;

namespace BioSignalVisualizer.Server.Controllers;

[ApiController]
[Route("api/annotations")]
public sealed class AnnotationsController : ControllerBase
{
    private readonly AnnotationStore _annotationStore;

    public AnnotationsController(AnnotationStore annotationStore)
    {
        _annotationStore = annotationStore;
    }

    [HttpGet]
    public async Task<IEnumerable<AnnotationEntry>> Get(CancellationToken cancellationToken)
        => await _annotationStore.GetAnnotationsAsync(cancellationToken);

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] IEnumerable<AnnotationEntry> annotations, CancellationToken cancellationToken)
    {
        await _annotationStore.SaveAnnotationsAsync(annotations, cancellationToken);
        return NoContent();
    }
}
