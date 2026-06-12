using System.Net.Http.Json;
using BioSignalVisualizer.Shared;

namespace BioSignalVisualizer.Client.Services;

public sealed class CatalogClient
{
    private readonly HttpClient _http;

    public CatalogClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<CatalogEntry>> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        var response = await _http.GetFromJsonAsync<List<CatalogEntry>>("api/catalog", cancellationToken);
        return response ?? new List<CatalogEntry>();
    }
}
