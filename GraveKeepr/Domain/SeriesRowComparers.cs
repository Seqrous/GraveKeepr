namespace GraveKeeper.Domain;

internal static class SeriesRowComparers
{
    private static bool DatesEqual(DateTime? x, DateTime? y) => x == y && x?.Kind == y?.Kind;
    private static int DateHashCode(DateTime? d) => HashCode.Combine(d, d?.Kind);
    
    private static DateTime? GetForecastAt(SeriesRow r) => r is ForecastSeriesRow s ? s.ForecastAt : null;

    internal sealed class AppendRevisionKeyComparer : IEqualityComparer<SeriesRow>
    {
        public static readonly AppendRevisionKeyComparer Instance = new();

        public bool Equals(SeriesRow? x, SeriesRow? y) =>
            x is not null && y is not null &&
            x.SeriesKey == y.SeriesKey &&
            DatesEqual(GetForecastAt(x), GetForecastAt(y)) &&
            DatesEqual(x.EventTime, y.EventTime) &&
            DatesEqual(x.RecordedAt, y.RecordedAt);

        public int GetHashCode(SeriesRow r) => HashCode.Combine(
            r.SeriesKey,
            DateHashCode(GetForecastAt(r)),
            DateHashCode(r.EventTime),
            DateHashCode(r.RecordedAt)
        );
    }
    
    internal sealed class UpsertRevisionKeyComparer : IEqualityComparer<SeriesRow>
    {
        public static readonly UpsertRevisionKeyComparer Instance = new();

        public bool Equals(SeriesRow? x, SeriesRow? y) =>
            x is not null && y is not null &&
            x.SeriesKey == y.SeriesKey &&
            DatesEqual(GetForecastAt(x), GetForecastAt(y)) &&
            DatesEqual(x.EventTime, y.EventTime);
        
        public int GetHashCode(SeriesRow r) => HashCode.Combine(
            r.SeriesKey,
            DateHashCode(GetForecastAt(r)),
            DateHashCode(r.EventTime)
        );
    }
}