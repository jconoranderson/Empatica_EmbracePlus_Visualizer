using System.Net.Http.Json;
using BioSignalVisualizer.Shared;

namespace BioSignalVisualizer.Client.Services;

public sealed class ActivityClient
{
    private readonly HttpClient _http;

    public ActivityClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<ActivityWindow>> GetWindowsAsync(string date, string participant, CancellationToken cancellationToken = default)
    {
        var url = $"api/activity?date={Uri.EscapeDataString(date)}&participant={Uri.EscapeDataString(participant)}";
        var windows = await _http.GetFromJsonAsync<List<ActivityWindow>>(url, cancellationToken);
        return windows ?? new List<ActivityWindow>();
    }
}
