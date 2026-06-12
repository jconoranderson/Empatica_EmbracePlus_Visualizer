using System.Net.Http.Json;
using BioSignalVisualizer.Shared;

namespace BioSignalVisualizer.Client.Services;

public sealed class AnnotationsClient
{
    private readonly HttpClient _http;

    public AnnotationsClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<AnnotationEntry>> GetAsync(CancellationToken cancellationToken = default)
    {
        var entries = await _http.GetFromJsonAsync<List<AnnotationEntry>>("api/annotations", cancellationToken);
        return entries ?? new List<AnnotationEntry>();
    }

    public async Task SaveAsync(IEnumerable<AnnotationEntry> annotations, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync("api/annotations", annotations, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
