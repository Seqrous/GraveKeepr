using GraveKeeper.Infrastructure;
using GraveKeeper.UI;

namespace GraveKeeper.Commands;

public class SeriesCommand(
    string args,
    ISeriesMetadataRepository repository,
    IUserInteraction userInteraction,
    AppState appState) : ICommand
{
    public async Task ExecuteAsync()
    {
        if (!int.TryParse(args, out var seriesId))
        {
            userInteraction.ErrorAsync("Error", $"Invalid series ID: {seriesId}");
            return;
        }

        var metadata = await repository.GetSeriesMetadataAsync(seriesId);
        appState.SetActiveSeries(metadata);
    }
}