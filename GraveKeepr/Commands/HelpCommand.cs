using GraveKeeper.UI;

namespace GraveKeeper.Commands;

public class HelpCommand(IUserInteraction userInteraction) : ICommand
{
    private const string HelpText =
        """
        /series <id>
            Load series metadata for the given series ID.
            
        /query <expression>
            Fetch series rows matching a filter expression.
            Fields: forecast_at, event_time
            Ops: = != < > <= >=
            Logical: AND, OR, ( )
            Example: event_time >= 2024-01-01 00:00:00 AND event_time < 2026-01-01 00:00:00

        /tombstone
            Write tombstone rows for all rows currently marked in the table.
            Requires an active series and a loaded query result.
            To mark rows: focus the table, navigate with arrow keys,
            press X to mark/unmark, Shift+Arrow to extend a range,
            or Ctrl+A to select all, then X to toggle the whole set.
            
        /help
            Show this message.
        """;

    public Task ExecuteAsync()
    {
        userInteraction.ShowTextAsync("Help", HelpText);
        return Task.CompletedTask;
    }
}