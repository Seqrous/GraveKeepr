using GraveKeeper.Domain;

namespace GraveKeeper.Infrastructure;

public sealed class DataFlowStrategyFactory
{
    public IDataFlowStrategy Create(DataFlowType flowType) =>
        flowType switch
        {
            DataFlowType.Append => new AppendStrategy(),
            DataFlowType.Upsert => new UpsertStrategy(),
            _ => throw new InvalidOperationException($"Unsupported data flow type: {flowType}")
        };
    
    private sealed class AppendStrategy : IDataFlowStrategy
    {
        private static readonly IEqualityComparer<SeriesRow> Comparer =
            SeriesRowComparers.AppendRevisionKeyComparer.Instance;
        
        public IReadOnlyList<SeriesRowViewModel> CreateViewModels(IReadOnlyList<SeriesRow> rows) =>
            CreateViewModelsCore(rows, Comparer);

        public TombstoneStagedRows Stage(IEnumerable<SeriesRow> rows) => new(rows, Comparer);
    }
    
    private sealed class UpsertStrategy : IDataFlowStrategy
    {
        private static readonly IEqualityComparer<SeriesRow> Comparer =
            SeriesRowComparers.UpsertRevisionKeyComparer.Instance;

        public IReadOnlyList<SeriesRowViewModel> CreateViewModels(IReadOnlyList<SeriesRow> rows) =>
            CreateViewModelsCore(rows, Comparer);

        public TombstoneStagedRows Stage(IEnumerable<SeriesRow> rows) => new(rows, Comparer);
    }

    private static IReadOnlyList<SeriesRowViewModel> CreateViewModelsCore(
        IReadOnlyList<SeriesRow> rows,
        IEqualityComparer<SeriesRow> comparer)
    {
        var groups = rows.GroupBy(r => r, comparer).ToList();
        List<SeriesRowViewModel> result = [];
        foreach (var group in groups)
        {
            var ordered = group.OrderByDescending(r => r.IngestedAt).ToList();
            var latest = ordered[0];

            if (latest.Value is null)
                result.Add(new SeriesRowViewModel(ordered.Skip(1).First(), RowKind.Invalidated));
            else
                result.AddRange(ordered.Select(r => new SeriesRowViewModel(r, RowKind.Normal)));
        }

        return result;
    }
}