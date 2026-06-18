using System.Net.Http.Json;
using System.Text.Json;
using BioSignalVisualizer.Shared;

namespace BioSignalVisualizer.Client.Services;

public sealed class MetricsClient
{
    private readonly HttpClient _http;

    public MetricsClient(HttpClient http)
    {
        _http = http;
    }

    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    public async Task<MetricSeries?> GetSeriesAsync(string date, string participant, string metric, CancellationToken cancellationToken = default)
    {
        var url = $"api/metrics?date={Uri.EscapeDataString(date)}&participant={Uri.EscapeDataString(participant)}&metric={Uri.EscapeDataString(metric)}";
        return await _http.GetFromJsonAsync<MetricSeries>(url, _options, cancellationToken);
    }
}
