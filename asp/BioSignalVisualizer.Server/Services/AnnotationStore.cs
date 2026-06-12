using System.Text.Json;
using BioSignalVisualizer.Shared;
using Microsoft.Extensions.Options;

namespace BioSignalVisualizer.Server.Services;

public sealed class AnnotationStore
{
    private readonly string _storePath;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public AnnotationStore(IOptions<VisualizerSettings> settings)
    {
        var basePath = settings.Value.BaseDataPath;
        if (string.IsNullOrWhiteSpace(basePath))
        {
            basePath = AppContext.BaseDirectory;
        }

        _storePath = Path.Combine(basePath, "annotations_store.json");
    }

    public async Task<IReadOnlyList<AnnotationEntry>> GetAnnotationsAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_storePath))
        {
            return Array.Empty<AnnotationEntry>();
        }

        await using var stream = File.OpenRead(_storePath);
        var entries = await JsonSerializer.DeserializeAsync<List<AnnotationEntry>>(stream, _serializerOptions, cancellationToken);
        return entries ?? new List<AnnotationEntry>();
    }

    public async Task SaveAnnotationsAsync(IEnumerable<AnnotationEntry> annotations, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(_storePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_storePath);
        await JsonSerializer.SerializeAsync(stream, annotations, _serializerOptions, cancellationToken);
    }
}
