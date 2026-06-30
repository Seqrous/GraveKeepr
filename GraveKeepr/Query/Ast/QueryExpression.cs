namespace GraveKeeper.Query.Ast;

internal abstract record QueryExpression;

internal sealed record ComparisonExpression(DateField Field, ComparisonOp Op, DateTime Value) : QueryExpression;
internal sealed record LogicalExpression(LogicalOp Op, QueryExpression Left, QueryExpression Right) : QueryExpression;