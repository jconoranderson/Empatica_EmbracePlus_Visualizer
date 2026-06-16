using System.Globalization;
using System.Text.RegularExpressions;
using BioSignalVisualizer.Shared;
using CsvHelper;
using Microsoft.Extensions.Options;

namespace BioSignalVisualizer.Server.Services;

public sealed class ActivityWindowService
{
    private readonly VisualizerSettings _settings;
    private static readonly Dictionary<string, string> _presetColors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["still"] = "#4ade80", // Level 1 - Green
        ["generic"] = "#4ade80", // Level 1 - Green
        ["inactive"] = "#4ade80", // Level 1 - Green
        ["sedentary"] = "#4ade80", // Level 1 - Green
        ["light"] = "#4ade80", // Level 1 - Green
        ["walking"] = "#facc15", // Level 2 - Yellow
        ["moderate"] = "#facc15", // Level 2 - Yellow
        ["running"] = "#f87171", // Level 3 - Red
        ["vigorous"] = "#f87171", // Level 3 - Red
        ["biking"] = "#f87171", // Level 3 - Red
        ["device_not_recording"] = "#d1d5db", // Grey
        ["unknown"] = "#d1d5db", // Grey
        ["sleep"] = "#a066c5" // Purple
    };

    public ActivityWindowService(IOptions<VisualizerSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<IReadOnlyList<ActivityWindow>> GetWindowsAsync(string date, string participant, CancellationToken cancellationToken = default)
    {
        var aggDir = Path.Combine(_settings.BaseDataPath, date, participant, "digital_biomarkers", "aggregated_per_minute");
        if (!Directory.Exists(aggDir))
        {
            return Array.Empty<ActivityWindow>();
        }

        var filePath = Directory
            .EnumerateFiles(aggDir, "*activity-classification*.csv", SearchOption.TopDirectoryOnly)
            .FirstOrDefault();
        if (filePath is null)
        {
            return Array.Empty<ActivityWindow>();
        }

        await using var stream = File.OpenRead(filePath);
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var entries = new List<ActivityEntry>();
        await foreach (var record in csv.GetRecordsAsync<dynamic>(cancellationToken))
        {
            var dict = (IDictionary<string, object>)record;
            var timestamp = TryExtractTimestamp(dict);
            if (timestamp is null)
            {
                continue;
            }

            var label = TryExtractClassification(dict);
            if (string.IsNullOrWhiteSpace(label))
            {
                continue;
            }

            entries.Add(new ActivityEntry(timestamp.Value, label.Trim()));
        }

        if (entries.Count == 0)
        {
            return Array.Empty<ActivityWindow>();
        }

        entries.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
        var windows = new List<ActivityWindow>();
        var start = entries[0].Timestamp;
        var currentLabel = entries[0].Label;

        for (var i = 1; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (!string.Equals(entry.Label, currentLabel, StringComparison.OrdinalIgnoreCase))
            {
                windows.Add(CreateWindow(participant, start, entry.Timestamp, currentLabel));
                start = entry.Timestamp;
                currentLabel = entry.Label;
            }
        }

        // Extend final window by one interval (default 1 minute)
        var lastTimestamp = entries[^1].Timestamp;
        var delta = entries.Count > 1
            ? entries[^1].Timestamp - entries[^2].Timestamp
            : TimeSpan.FromMinutes(1);
        if (delta <= TimeSpan.Zero)
        {
            delta = TimeSpan.FromMinutes(1);
        }

        windows.Add(CreateWindow(participant, start, lastTimestamp + delta, currentLabel));
        return windows;
    }

    private static ActivityWindow CreateWindow(string participant, DateTime start, DateTime end, string label)
        => new(participant, start, end, label, ResolveColor(label));

    private static string ResolveColor(string label)
    {
        if (_presetColors.TryGetValue(label.Trim(), out var color))
        {
            return color;
        }

        var hash = label.Aggregate(0, (current, ch) => current + ch);
        var r = (hash * 37) % 255;
        var g = (hash * 91) % 255;
        var b = (hash * 53) % 255;
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    private static DateTime? TryExtractTimestamp(IDictionary<string, object> dict)
    {
        foreach (var kvp in dict)
        {
            if (!kvp.Key.Contains("timestamp", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (DateTime.TryParse(kvp.Value?.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var timestamp))
            {
                return DateTime.SpecifyKind(timestamp, DateTimeKind.Utc);
            }

            if (long.TryParse(kvp.Value?.ToString(), out var raw))
            {
                if (raw > 1_000_000_000_000)
                {
                    return DateTimeOffset.FromUnixTimeMilliseconds(raw).UtcDateTime;
                }

                if (raw > 1_000_000_000)
                {
                    return DateTimeOffset.FromUnixTimeSeconds(raw).UtcDateTime;
                }
            }
        }

        return null;
    }

    private static string TryExtractClassification(IDictionary<string, object> dict)
    {
        foreach (var kvp in dict)
        {
            if (Regex.IsMatch(kvp.Key, "activity.*class", RegexOptions.IgnoreCase))
            {
                return kvp.Value?.ToString() ?? string.Empty;
            }
        }

        return string.Empty;
    }

    private sealed record ActivityEntry(DateTime Timestamp, string Label);
}
