using System.Net.Http.Json;
using BioSignalVisualizer.Shared;

namespace BioSignalVisualizer.Client.Services;

public sealed class MetricsClient
{
    private readonly HttpClient _http;

    public MetricsClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<MetricSeries?> GetSeriesAsync(string date, string participant, string metric, CancellationToken cancellationToken = default)
    {
        var url = $"api/metrics?date={Uri.EscapeDataString(date)}&participant={Uri.EscapeDataString(participant)}&metric={Uri.EscapeDataString(metric)}";
        return await _http.GetFromJsonAsync<MetricSeries>(url, cancellationToken);
    }
}
