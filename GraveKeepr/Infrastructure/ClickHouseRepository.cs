using System.Data.Common;
using System.Text;
using ClickHouse.Driver.ADO;
using ClickHouse.Driver.ADO.Parameters;
using GraveKeeper.Domain;
using GraveKeeper.Query;

namespace GraveKeeper.Infrastructure;

public abstract class ClickHouseRepository<TRow>(ClickHouseConnection connection) : IClickHouseRepository 
    where TRow : SeriesRow
{
    public async Task<ulong> CountData(uint seriesKey, CompiledQuery query)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText =$"""
            SELECT COUNT()
            FROM {TableName}
            WHERE series_id = @SeriesKey
            {BuildWherePredicate(query)}
        """;
        
        cmd.Parameters.AddRange(new ClickHouseDbParameter[]
        {
            new() { ParameterName = "SeriesKey", Value = seriesKey }
        });
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToUInt64(result);
    }

    public async Task<IReadOnlyList<SeriesRow>> GetData(uint seriesKey, CompiledQuery query)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText =$"""
            SELECT *
            FROM {TableName}
            WHERE series_id = @SeriesKey
            {BuildWherePredicate(query)}
            ORDER BY ({OrderByClause}) DESC
        """;
        
        cmd.Parameters.AddRange(new ClickHouseDbParameter[]
        {
            new() { ParameterName = "SeriesKey", Value = seriesKey }
        });
        await using var reader = await cmd.ExecuteReaderAsync();
        List<TRow> rows = [];
        while (await reader.ReadAsync())
            rows.Add(ReadRow(reader));

        return rows;
    }

    public async Task TombstoneRows(TombstoneStagedRows rows)
    {
        if (rows.Count == 0) return;

        var i = 0;
        var whereBuilder = new StringBuilder();
        foreach (var row in rows)
        {
            whereBuilder.Append('(');
            AppendRowPredicate(whereBuilder, (TRow)row);
            whereBuilder.Append(')');
            if (i != rows.Count - 1) whereBuilder.Append(" OR ");
            i++;
        }
        
        var sql = BuildTombstoneInsert(whereBuilder.ToString());
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync();
    }
    
    protected abstract string TableName { get; }
    protected abstract string OrderByClause { get; }
    protected abstract TRow ReadRow(DbDataReader reader);
    protected abstract void AppendRowPredicate(StringBuilder sb, TRow row);
    protected abstract string BuildTombstoneInsert(string whereClause);
    
    private static string BuildWherePredicate(CompiledQuery query) =>
        query.Sql.Length > 0 ? $"AND {query.Sql}" : string.Empty;
}

public sealed class ForecastClickHouseRepository(ClickHouseConnection connection)
    : ClickHouseRepository<ForecastSeriesRow>(connection)
{
    protected override string TableName => "demo.series_forecast";
    protected override string OrderByClause => "forecast_at, event_time, recorded_at, ingested_at";
    protected override ForecastSeriesRow ReadRow(DbDataReader reader) => ForecastSeriesRow.FromReader(reader);

    protected override void AppendRowPredicate(StringBuilder sb, ForecastSeriesRow row)
    {
        sb.Append($"series_id = {row.SeriesKey} AND ");
        sb.Append($"forecast_at = toDateTime('{row.ForecastAt:yyyy-MM-dd HH:mm:ss}') AND ");
        sb.Append($"event_time = toDateTime('{row.EventTime:yyyy-MM-dd HH:mm:ss}') AND ");
        sb.Append($"recorded_at = toDateTime('{row.RecordedAt:yyyy-MM-dd HH:mm:ss}') AND ");
        sb.Append($"ingested_at = toDateTime('{row.IngestedAt:yyyy-MM-dd HH:mm:ss}', 7)");
    }

    protected override string BuildTombstoneInsert(string whereClause) => $"""
        INSERT INTO {TableName} (
            series_id,
            forecast_at,
            event_time,
            recorded_at,
            ingested_at,
            value,
            quality
        )
        SELECT
            series_id,
            forecast_at,
            event_time,
            recorded_at,
            now(),
            NULL,
            quality
        FROM {TableName}
        WHERE {whereClause}
    """;
}

public sealed class ActualClickHouseRepository(ClickHouseConnection connection)
    : ClickHouseRepository<ActualSeriesRow>(connection)
{
    protected override string TableName => "demo.series_actual";
    protected override string OrderByClause => "event_time, recorded_at, ingested_at";
    protected override ActualSeriesRow ReadRow(DbDataReader reader) => ActualSeriesRow.FromReader(reader);

    protected override void AppendRowPredicate(StringBuilder sb, ActualSeriesRow row)
    {
        sb.Append($"series_id = {row.SeriesKey} AND ");
        sb.Append($"event_time = toDateTime('{row.EventTime:yyyy-MM-dd HH:mm:ss}') AND ");
        sb.Append($"recorded_at = toDateTime('{row.RecordedAt:yyyy-MM-dd HH:mm:ss}') AND ");
        sb.Append($"ingested_at = toDateTime('{row.IngestedAt:yyyy-MM-dd HH:mm:ss}', 7)");
    }

    protected override string BuildTombstoneInsert(string whereClause) => $"""
        INSERT INTO {TableName} (
            series_id,
            event_time,
            recorded_at,
            ingested_at,
            value,
            quality
        )
        SELECT
            series_id,
            event_time,
            recorded_at,
            now(),
            NULL,
            quality
        FROM {TableName}
        WHERE {whereClause}
    """;
}