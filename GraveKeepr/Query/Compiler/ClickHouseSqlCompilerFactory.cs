using GraveKeeper.Domain;

namespace GraveKeeper.Query.Compiler;

internal class ClickHouseSqlCompilerFactory
{
    public static ClickHouseSqlCompiler Create(SeriesType seriesType) => seriesType switch
    {
        SeriesType.Forecast => new ClickHouseSqlCompiler(new HashSet<DateField> { DateField.ForecastAt, DateField.EventTime }),
        SeriesType.Actual => new ClickHouseSqlCompiler(new HashSet<DateField> { DateField.EventTime }),
        _ => throw new InvalidOperationException($"Unsupported series type: {seriesType}")
    };
}