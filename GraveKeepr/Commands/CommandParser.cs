using GraveKeeper.Infrastructure;
using GraveKeeper.UI;

namespace GraveKeeper.Commands;

public class CommandParser(
    ISeriesMetadataRepository seriesMetadataRepository,
    ClickHouseRepositoryFactory clickHouseRepositoryFactory,
    IUserInteraction userInteraction,
    AppState appState)
{
    public ICommand Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) throw new InvalidOperationException("Command cannot be empty");
        if (!input.StartsWith('/')) throw new InvalidOperationException("Command must start with '/'");

        var i = 1;
        var name = string.Empty;
        while (i < input.Length && !char.IsWhiteSpace(input[i]))
            name += input[i++];

        var args = input[i..].Trim();
        return name.ToLowerInvariant() switch
        {
            "series" => new SeriesCommand(args, seriesMetadataRepository, userInteraction, appState),
            "query" => new QueryCommand(args, clickHouseRepositoryFactory, userInteraction, appState),
            "tombstone" => new TombstoneCommand(clickHouseRepositoryFactory, userInteraction, appState),
            "help" => new HelpCommand(userInteraction),
            _ => throw new InvalidOperationException($"Unknown command '{name}'. Type /help for available commands.")
        };
    }
}