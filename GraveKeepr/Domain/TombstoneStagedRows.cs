using System.Collections;

namespace GraveKeeper.Domain;

/// <summary>
/// A wrapper responsible for encapsulating an invariant that for each set of rows with the same natural key,
/// only the one with the latest IngestedAt is retained. This is crucial for the correct functioning of the
/// tombstoning logic, as it ensures that a single tombstone is created in history.
/// </summary>
public sealed class TombstoneStagedRows : IEnumerable<SeriesRow>
{
    private IReadOnlySet<SeriesRow> Rows { get; }
    public int Count => Rows.Count;

    public TombstoneStagedRows(IEnumerable<SeriesRow> rows, IEqualityComparer<SeriesRow> naturalKeyComparer)
    {
        Rows = rows
            .GroupBy(r => r, naturalKeyComparer)
            .Select(g => g.MaxBy(r => r.IngestedAt)!)
            .ToHashSet(naturalKeyComparer);
    }

    public IEnumerator<SeriesRow> GetEnumerator() => Rows.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Rows.GetEnumerator();
}