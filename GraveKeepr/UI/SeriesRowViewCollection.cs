using GraveKeeper.Domain;
using Terminal.Gui.Drawing;
using Terminal.Gui.Views;
using Attribute = Terminal.Gui.Drawing.Attribute;

namespace GraveKeeper.UI;

public class SeriesRowViewCollection(IReadOnlyList<SeriesRowViewModel> rows)
{
    public DataTableSource ToTableSource() => new(SeriesRow.ToDataTable(rows.Select(r => r.Row).ToList()));

    public Scheme? GetColorScheme(int rowIndex, Attribute normalBackground)
    {
        var row = rows[rowIndex];
        if (row.IsMarkedForTombstone)
            return new Scheme(new Attribute(ColorName16.Red, normalBackground.Background));
        if (row.Kind == RowKind.Invalidated)
            return new Scheme(new Attribute(ColorName16.DarkGray, normalBackground.Background));
        return null;
    }

    public void ToggleMark(IEnumerable<int> rowIndices)
    {
        var indices = rowIndices.ToList();
        var anyUnmarked = indices.Any(i => rows[i].Kind == RowKind.Normal && !rows[i].IsMarkedForTombstone);
        foreach (var i in indices.Where(i => anyUnmarked != rows[i].IsMarkedForTombstone))
            rows[i].ToggleTombstoneMark();
    }

    public IReadOnlySet<SeriesRow> MarkedRows => rows
        .Where(r => r is { IsMarkedForTombstone: true })
        .Select(r => r.Row)
        .ToHashSet();
}