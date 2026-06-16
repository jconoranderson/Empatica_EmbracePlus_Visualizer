using System.Dynamic;
using System.Globalization;
using BioSignalVisualizer.Shared;
using CsvHelper;
using Microsoft.Extensions.Options;

namespace BioSignalVisualizer.Server.Services;

public sealed class MetricLoader
{
    private readonly VisualizerSettings _settings;

    public MetricLoader(IOptions<VisualizerSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<MetricSeries?> LoadMetricAsync(string date, string participant, string metric, CancellationToken cancellationToken = default)
    {
        var csvPath = Path.Combine(_settings.BaseDataPath, date, participant, "digital_biomarkers", "aggregated_per_minute", metric + ".csv");
        if (!File.Exists(csvPath))
        {
            return null;
        }

        await using var stream = File.OpenRead(csvPath);
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var points = new List<DataPoint>();
        await foreach (var record in csv.GetRecordsAsync<dynamic>(cancellationToken))
        {
            var dict = (IDictionary<string, object>)record;
            if (!TryExtractTimestamp(dict, out var timestamp) || timestamp is null)
            {
                continue;
            }

            double? value = TryExtractValue(dict);
            if (value is null)
            {
                continue;
            }

            points.Add(new DataPoint(timestamp.Value, value.Value));
        }

        return new MetricSeries(participant, metric, points.OrderBy(p => p.Timestamp));
    }

    private static bool TryExtractTimestamp(IDictionary<string, object> dict, out DateTime? timestamp)
    {
        timestamp = null;
        foreach (var kvp in dict)
        {
            if (!kvp.Key.Contains("timestamp", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (DateTime.TryParse(kvp.Value?.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
            {
                timestamp = parsed.ToUniversalTime();
                return true;
            }

            if (long.TryParse(kvp.Value?.ToString(), out var raw))
            {
                if (raw > 1_000_000_000_000)
                {
                    timestamp = DateTimeOffset.FromUnixTimeMilliseconds(raw).UtcDateTime;
                    return true;
                }
                if (raw > 1_000_000_000)
                {
                    timestamp = DateTimeOffset.FromUnixTimeSeconds(raw).UtcDateTime;
                    return true;
                }
            }
        }
        return false;
    }

    private static double? TryExtractValue(IDictionary<string, object> dict)
    {
        foreach (var kvp in dict)
        {
            if (kvp.Key.Contains("timestamp", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            if (kvp.Key.Contains("participant", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            if (kvp.Key.Contains("missing", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            if (double.TryParse(kvp.Value?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }
        }
        return null;
    }
}
