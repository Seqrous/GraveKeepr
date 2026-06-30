using GraveKeeper.Query.Ast;

namespace GraveKeeper.Query.Compiler;

internal class ClickHouseSqlCompiler(IReadOnlySet<DateField> allowedFields)
{
    public string Compile(QueryExpression expression) => expression switch
    {
        ComparisonExpression cmp => $"{FieldToSql(cmp.Field)} {OpToSql(cmp.Op)} {DateLiteralToSql(cmp.Value)}",
        LogicalExpression log => $"({Compile(log.Left)}) {LogicalOpToSql(log.Op)} ({Compile(log.Right)})",
        _ => throw new InvalidOperationException($"Unknown expression type '{expression}'")
    };

    private static string LogicalOpToSql(LogicalOp op) => op switch
    {
        LogicalOp.And => "AND",
        LogicalOp.Or => "OR",
        _ => throw new InvalidOperationException($"Unknown logical operator '{op}'")
    };

    private static string OpToSql(ComparisonOp op) => op switch
    {
        ComparisonOp.Equal => "=",
        ComparisonOp.NotEqual => "!=",
        ComparisonOp.Less => "<",
        ComparisonOp.Greater => ">",
        ComparisonOp.LessOrEqual => "<=",
        ComparisonOp.GreaterOrEqual => ">=",
        _ => throw new InvalidOperationException($"Unknown comparison operator '{op}'")
    };

    private string FieldToSql(DateField field)
    {
        if (!allowedFields.Contains(field))
            throw new InvalidOperationException($"Field '{field}' is not available for this series type.");

        return field switch
        {
            DateField.ForecastAt => "forecast_at",
            DateField.EventTime => "event_time",
            _ => throw new InvalidOperationException($"Unknown date field '{field}'")
        };
    }

    private static string DateLiteralToSql(DateTime date) => $"toDateTime('{date:yyyy-MM-dd HH:mm:ss}')";
}