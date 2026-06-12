using System.Net.Http.Json;
using BioSignalVisualizer.Shared;

namespace BioSignalVisualizer.Client.Services;

public sealed class PdfClient
{
    private readonly HttpClient _http;

    public PdfClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<byte[]> DownloadSummaryAsync(PdfExportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync("api/export", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}
