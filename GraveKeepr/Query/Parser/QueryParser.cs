using System.Globalization;
using GraveKeeper.Query.Ast;
using GraveKeeper.Query.Lexer;

namespace GraveKeeper.Query.Parser;

internal class QueryParser(IEnumerable<Token> tokens)
{
    private int _pivot;
    private readonly List<Token> _tokens = tokens.ToList();
    private const string DateFormat = "yyyy-MM-dd HH:mm:ss";

    public QueryExpression Parse()
    {
        var output = ParseOr();
        if (Peek().Kind != TokenKind.EndOfInput)
            throw new InvalidOperationException($"Expected end of input but found {Peek().Kind} at position {Peek().Position}");

        return output;
    }

    private QueryExpression ParseOr()
    {
        var left = ParseAnd();
        if (Peek().Kind != TokenKind.Or)
            return left;

        Consume();
        var right = ParseOr();
        return new LogicalExpression(LogicalOp.Or,  left, right);
    }

    private QueryExpression ParseAnd()
    {
        var left = ParseParenthesised();
        if (Peek().Kind != TokenKind.And)
            return left;

        Consume();
        var right = ParseAnd();
        return new LogicalExpression(LogicalOp.And, left, right);
    }

    private QueryExpression ParseParenthesised()
    {
        if (Peek().Kind == TokenKind.RightParen)
            throw new InvalidOperationException($"Unexpected ')' at position {Peek().Position}");

        if (Peek().Kind != TokenKind.LeftParen)
            return ParseComparisonExpression();

        Consume();
        var inner = ParseOr();
        if (Peek().Kind != TokenKind.RightParen)
            throw new InvalidOperationException($"Expected  ')' but found {Peek().Kind} at position {Peek().Position}");
        
        Consume();
        return inner;
    }

    private QueryExpression ParseComparisonExpression()
    {
        var dateField = ToDateField(Peek());
        Consume();
        var comparisonOp = ToComparisonOp(Peek());
        Consume();
        var dateTime =  ToDateTime(Peek());
        Consume();

        return new ComparisonExpression(dateField, comparisonOp, dateTime);
    }

    private Token Peek() => 
        _tokens.ElementAtOrDefault(_pivot) ?? throw new InvalidOperationException("Unexpected end of input");

    private void Consume() => _pivot++;

    private static DateField ToDateField(Token token) =>
        token.Kind switch
        {
            TokenKind.ForecastAt => DateField.ForecastAt,
            TokenKind.EventTime => DateField.EventTime,
            _ => throw new InvalidOperationException(
                $"Expected a date field but found {token.Kind} at position {token.Position}")
        };

    private static ComparisonOp ToComparisonOp(Token token) =>
        token.Kind switch
        {
            TokenKind.Equal => ComparisonOp.Equal,
            TokenKind.NotEqual => ComparisonOp.NotEqual,
            TokenKind.Less => ComparisonOp.Less,
            TokenKind.Greater => ComparisonOp.Greater,
            TokenKind.LessOrEqual => ComparisonOp.LessOrEqual,
            TokenKind.GreaterOrEqual => ComparisonOp.GreaterOrEqual,
            _ => throw new InvalidOperationException(
                $"Expected a comparison operator but found {token.Kind} at position {token.Position}")
        };

    private static DateTime ToDateTime(Token token)
    {
        if (token.Kind != TokenKind.DateLiteral)
            throw new InvalidOperationException(
                $"Expected a date literal but found {token.Kind} at position {token.Position}");

        if (!DateTime.TryParseExact(
            token.Text,
            DateFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var result
        ))
            throw new InvalidOperationException($"Invalid date literal '{token.Text}' at position {token.Position}");
        
        return result;
    }
}