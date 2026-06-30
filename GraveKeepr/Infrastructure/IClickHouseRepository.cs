using GraveKeeper.Domain;
using GraveKeeper.Query;

namespace GraveKeeper.Infrastructure;

public interface IClickHouseRepository
{
    Task<ulong> CountData(uint seriesKey, CompiledQuery query);
    Task<IReadOnlyList<SeriesRow>> GetData(uint seriesKey, CompiledQuery query);
    Task TombstoneRows(TombstoneStagedRows rows);
}