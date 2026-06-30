namespace GraveKeeper.Query;

public readonly record struct CompiledQuery
{
    public string Sql { get; }

    public CompiledQuery(string sql)
    {
        ArgumentNullException.ThrowIfNull(sql);
        Sql = sql;
    }
}