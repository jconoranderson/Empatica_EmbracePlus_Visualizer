using System.Net.Http.Json;

namespace BioSignalVisualizer.Client.Services;

public sealed class SyncClient
{
    private readonly HttpClient _http;

    public SyncClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> SyncDataAsync()
    {
        var response = await _http.PostAsync("api/sync", null);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return content;
    }
}
