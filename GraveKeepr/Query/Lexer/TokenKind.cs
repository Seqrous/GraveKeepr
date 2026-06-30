namespace GraveKeeper.Query.Lexer;

public enum TokenKind
{
    DateLiteral,
    ForecastAt,
    EventTime,
    Equal,
    NotEqual,
    Less,
    Greater,
    LessOrEqual,
    GreaterOrEqual,
    And,
    Or,
    Not,
    LeftParen,
    RightParen,
    EndOfInput
}