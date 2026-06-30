using GraveKeeper.Domain;

namespace GraveKeeper.Infrastructure;

public interface ISeriesMetadataRepository
{
    Task<SeriesMetadata> GetSeriesMetadataAsync(int seriesId, CancellationToken ct = default);
}