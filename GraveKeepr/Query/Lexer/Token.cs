namespace GraveKeeper.Query.Lexer;

internal sealed record Token(TokenKind Kind, string Text, int Position);