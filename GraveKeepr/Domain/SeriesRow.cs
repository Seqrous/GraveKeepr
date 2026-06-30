using System.Data;

namespace GraveKeeper.Domain;

public abstract record SeriesRow(
    uint SeriesKey,
    DateTime EventTime,
    DateTime RecordedAt,
    DateTime IngestedAt,
    double? Value,
    string Quality
)
{
    protected abstract DataTable CreateDataTable();
    protected abstract void AddToDataTable(DataTable dt);

    public static DataTable ToDataTable(IReadOnlyList<SeriesRow> rows)
    {
        if (rows.Count == 0) return new DataTable();
        var dt = rows[0].CreateDataTable();
        foreach (var row in rows) row.AddToDataTable(dt);
        return dt;
    }
}