using GraveKeeper.Domain;
using GraveKeeper.Query.Compiler;
using GraveKeeper.Query.Lexer;
using GraveKeeper.Query.Parser;

namespace GraveKeeper.Query.Engine;

public class ClickHouseQueryEngine
{
    private readonly ClickHouseSqlCompiler _compiler;

    public static ClickHouseQueryEngine Create(SeriesType seriesType) =>
        new(ClickHouseSqlCompilerFactory.Create(seriesType));
    
    private ClickHouseQueryEngine(ClickHouseSqlCompiler compiler) =>  _compiler = compiler;

    public CompiledQuery Compile(string query)
    {
        if (string.IsNullOrEmpty(query))
            return new CompiledQuery("");

        var tokens = QueryLexer.Tokenize(query);
        var parser = new QueryParser(tokens);
        var ast = parser.Parse();
        return new CompiledQuery(_compiler.Compile(ast));
    }
}