using System.Text.Json;
using System.Text.Json.Serialization;
using GraveKeeper.Domain;
using JetBrains.Annotations;

namespace GraveKeeper.Infrastructure;

public class MetadataClient(string metadataFilePath) : ISeriesMetadataRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly Dictionary<int, SeriesMetadata> _index = LoadIndex(metadataFilePath);

    public Task<SeriesMetadata> GetSeriesMetadataAsync(int seriesId, CancellationToken ct = default)
    {
        if (!_index.TryGetValue(seriesId, out var metadata))
            throw new KeyNotFoundException($"No metadata found for series ID {seriesId}");
        
        return Task.FromResult(metadata);
    }

    private static Dictionary<int, SeriesMetadata> LoadIndex(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Metadata file not found: {path}", path);
        
        using var stream = File.OpenRead(path);
        var entries = JsonSerializer.Deserialize<List<MetadataEntry>>(stream, JsonOptions) ??
            throw new InvalidOperationException($"Failed to deserializer metadata file: {path}");

        return entries.ToDictionary(
            e => e.SeriesId,
            e => new SeriesMetadata(e.SeriesId, e.SeriesKey, e.Type, e.DataFlow)
        );
    }

    [UsedImplicitly]
    private sealed record MetadataEntry(int SeriesId, uint SeriesKey, SeriesType Type, DataFlowType DataFlow);
}