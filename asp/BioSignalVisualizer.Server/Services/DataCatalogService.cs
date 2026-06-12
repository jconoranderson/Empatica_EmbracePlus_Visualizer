using System.Text.RegularExpressions;
using BioSignalVisualizer.Shared;
using Microsoft.Extensions.Options;

namespace BioSignalVisualizer.Server.Services;

public sealed class DataCatalogService
{
    private readonly VisualizerSettings _settings;
    private readonly Regex _datePattern = new("^\\d{4}-\\d{2}-\\d{2}$", RegexOptions.Compiled);

    public DataCatalogService(IOptions<VisualizerSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<IEnumerable<CatalogEntry>> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        var basePath = Path.GetFullPath(_settings.BaseDataPath);
        if (!Directory.Exists(basePath))
        {
            return Enumerable.Empty<CatalogEntry>();
        }

        var entries = new List<CatalogEntry>();
        foreach (var dateDir in Directory.EnumerateDirectories(basePath).Where(d => _datePattern.IsMatch(Path.GetFileName(d))))
        {
            foreach (var participantDir in Directory.EnumerateDirectories(dateDir))
            {
                var metrics = (await DiscoverMetricsAsync(participantDir)).ToList();
                if (metrics.Count == 0)
                {
                    continue;
                }

                entries.Add(new CatalogEntry(
                    Date: Path.GetFileName(dateDir),
                    Participant: Path.GetFileName(participantDir),
                    Metrics: metrics));
            }
        }

        return entries
            .OrderBy(e => e.Date)
            .ThenBy(e => e.Participant);
    }

    private async Task<IEnumerable<string>> DiscoverMetricsAsync(string participantDir)
    {
        var aggDir = Path.Combine(participantDir, "digital_biomarkers", "aggregated_per_minute");
        if (!Directory.Exists(aggDir))
        {
            return Enumerable.Empty<string>();
        }

        var metrics = new List<string>();
        foreach (var file in Directory.EnumerateFiles(aggDir, "*.csv"))
        {
            if (await HasValidDataAsync(file))
            {
                metrics.Add(Path.GetFileNameWithoutExtension(file));
            }
        }
        
        // A valid testing session should contain at least one core physiological metric with data
        bool hasCoreData = metrics.Exists(m => m.EndsWith("eda", StringComparison.OrdinalIgnoreCase) || 
                                               m.EndsWith("prv", StringComparison.OrdinalIgnoreCase) || 
                                               m.EndsWith("pulse-rate", StringComparison.OrdinalIgnoreCase) || 
                                               m.EndsWith("accelerometers-std", StringComparison.OrdinalIgnoreCase));
                                               
        if (!hasCoreData)
        {
            return Enumerable.Empty<string>();
        }

        return metrics;
    }

    private async Task<bool> HasValidDataAsync(string file)
    {
        try
        {
            using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);
            var headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(headerLine)) return false;

            var headers = headerLine.Split(',');
            var candidateIndices = new List<int>();
            for (int i = 0; i < headers.Length; i++)
            {
                var h = headers[i].ToLowerInvariant();
                if (!h.Contains("timestamp") && !h.Contains("participant") && !h.Contains("missing"))
                {
                    candidateIndices.Add(i);
                }
            }

            if (candidateIndices.Count == 0) return false;

            while (await reader.ReadLineAsync() is { } line)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                foreach (var idx in candidateIndices)
                {
                    if (idx < parts.Length && !string.IsNullOrWhiteSpace(parts[idx]) && double.TryParse(parts[idx], out _))
                    {
                        return true;
                    }
                }
            }
        }
        catch
        {
            // Ignore errors and assume no valid data
        }
        return false;
    }
}
