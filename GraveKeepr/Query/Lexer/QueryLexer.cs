namespace GraveKeeper.Query.Lexer;

internal static class QueryLexer
{
    public static IEnumerable<Token> Tokenize(string input)
    {
        for (var i = 0; i < input.Length;)
        {
            if (char.IsWhiteSpace(input[i]))
            {
                i++;
                continue;
            }

            if (char.IsLetter(input[i]))
            {
                var start = i;
                while (i < input.Length && (char.IsLetter(input[i]) || input[i] == '_'))
                    i++;
                
                var text = input.Substring(start, i - start);
                yield return text switch
                {
                    "forecast_at" => new Token(TokenKind.ForecastAt, text, start),
                    "event_time" => new Token(TokenKind.EventTime, text, start),
                    "AND" => new Token(TokenKind.And, text, start),
                    "OR" => new Token(TokenKind.Or, text, start),
                    "NOT" => new Token(TokenKind.Not, text, start),
                    _ => throw new InvalidOperationException($"Unknown identifier '{text}' at position {start}")
                };
            }
            else if (char.IsDigit(input[i]))
            {
                var start = i;
                while (i < input.Length && (char.IsDigit(input[i]) || input[i] is '-' or ':' or ' '))
                    i++;

                var text = input.Substring(start, i - start).Trim();
                yield return new Token(TokenKind.DateLiteral, text, start);
            }
            else
            {
                var tokenKind = input[i] switch
                {
                    '=' => TokenKind.Equal,
                    '!' when i + 1 < input.Length && input[i + 1] == '=' => TokenKind.NotEqual,
                    '<' when i + 1 < input.Length && input[i + 1] == '=' => TokenKind.LessOrEqual,
                    '>' when i + 1 < input.Length && input[i + 1] == '=' => TokenKind.GreaterOrEqual,
                    '<' => TokenKind.Less,
                    '>' => TokenKind.Greater,
                    '(' => TokenKind.LeftParen,
                    ')' => TokenKind.RightParen,
                    _ => throw new InvalidOperationException($"Unexpected token '{input[i]}' at position {i}")
                };
                
                var token = tokenKind switch
                {
                    TokenKind.NotEqual or TokenKind.LessOrEqual or TokenKind.GreaterOrEqual => new Token(tokenKind, input.Substring(i,  2), i),
                    _ => new Token(tokenKind, input.Substring(i, 1), i)
                };
                i += token.Text.Length;
                yield return token;
            }
        }

        yield return new Token(TokenKind.EndOfInput, string.Empty, input.Length);
    }
}