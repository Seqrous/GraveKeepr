using System.Data;
using System.Data.Common;

namespace GraveKeeper.Domain;

public record ForecastSeriesRow(
    uint SeriesKey,
    DateTime ForecastAt,
    DateTime EventTime,
    DateTime RecordedAt,
    DateTime IngestedAt,
    double? Value,
    string Quality
) : SeriesRow(SeriesKey, EventTime, RecordedAt, IngestedAt, Value, Quality)
{
    private static class Columns
    {
        public const string SeriesId = "series_id";
        public const string ForecastAt = "forecast_at";
        public const string EventTime = "event_time";
        public const string RecordedAt = "recorded_at";
        public const string IngestedAt = "ingested_at";
        public const string Value = "value";
        public const string Quality = "quality";
    }

    public static ForecastSeriesRow FromReader(DbDataReader reader) => new(
        Convert.ToUInt32(reader[Columns.SeriesId]),
        Convert.ToDateTime(reader[Columns.ForecastAt]),
        Convert.ToDateTime(reader[Columns.EventTime]),
        Convert.ToDateTime(reader[Columns.RecordedAt]),
        Convert.ToDateTime(reader[Columns.IngestedAt]),
        reader[Columns.Value] is DBNull ? null : Convert.ToDouble(reader[Columns.Value]),
        reader[Columns.Quality].ToString() ?? string.Empty
    );

    protected override DataTable CreateDataTable()
    {
        var dt = new DataTable();
        dt.Columns.Add(Columns.SeriesId, typeof(uint));
        dt.Columns.Add(Columns.ForecastAt, typeof(DateTime));
        dt.Columns.Add(Columns.EventTime, typeof(DateTime));
        dt.Columns.Add(Columns.RecordedAt, typeof(DateTime));
        dt.Columns.Add(Columns.IngestedAt, typeof(DateTime));
        dt.Columns.Add(Columns.Value, typeof(double));
        dt.Columns.Add(Columns.Quality, typeof(string));

        return dt;
    }

    protected override void AddToDataTable(DataTable dt) =>
        dt.Rows.Add(
            SeriesKey,
            ForecastAt,
            EventTime,
            RecordedAt,
            IngestedAt,
            (object?)Value ?? DBNull.Value,
            Quality
        );
}