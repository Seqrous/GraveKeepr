using GraveKeeper.Domain;
using GraveKeeper.Infrastructure;
using GraveKeeper.Query;

namespace GraveKeeper;

public class AppState(DataFlowStrategyFactory dataFlowStrategyFactory)
{
    private DataFlowStrategyFactory DataFlowStrategyFactory { get; } = dataFlowStrategyFactory;
    public SeriesMetadata? ActiveSeries { get; private set; }
    public IDataFlowStrategy? ActiveStrategy { get; private set; }
    public IReadOnlySet<SeriesRow>? TombstoneMarkedRows { get; private set; }
    public CompiledQuery? LastSearchQuery { get; private set; }

    public event EventHandler<SeriesMetadata>? SeriesChanged;
    public event EventHandler<IReadOnlyList<SeriesRow>>? RowsLoaded;

    public void SetActiveSeries(SeriesMetadata metadata)
    {
        ActiveSeries = metadata;
        ActiveStrategy = DataFlowStrategyFactory.Create(metadata.DataFlowType);
        SeriesChanged?.Invoke(this, metadata);
    }

    public void SetLoadedRows(IReadOnlyList<SeriesRow> rows, CompiledQuery compiledQuery)
    {
        LastSearchQuery = compiledQuery;
        RowsLoaded?.Invoke(this, rows);
    }

    public void SetTombstoneMarkedRows(IReadOnlySet<SeriesRow> rows) => TombstoneMarkedRows = rows;
}
