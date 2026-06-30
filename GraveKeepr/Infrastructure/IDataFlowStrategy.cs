using GraveKeeper.Domain;

namespace GraveKeeper.Infrastructure;

public interface IDataFlowStrategy
{
    IReadOnlyList<SeriesRowViewModel> CreateViewModels(IReadOnlyList<SeriesRow> rows);
    TombstoneStagedRows Stage(IEnumerable<SeriesRow> rows);
}