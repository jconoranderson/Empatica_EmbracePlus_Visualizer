using BioSignalVisualizer.Server.Services;
using BioSignalVisualizer.Shared;
using Microsoft.AspNetCore.Mvc;

namespace BioSignalVisualizer.Server.Controllers;

[ApiController]
[Route("api/catalog")]
public sealed class CatalogController : ControllerBase
{
    private readonly DataCatalogService _catalogService;

    public CatalogController(DataCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    public async Task<IEnumerable<CatalogEntry>> Get(CancellationToken cancellationToken)
        => await _catalogService.GetCatalogAsync(cancellationToken);
}
