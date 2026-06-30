using GraveKeeper.Infrastructure;
using GraveKeeper.UI;

namespace GraveKeeper.Commands;

public class TombstoneCommand(
    ClickHouseRepositoryFactory clickHouseRepositoryFactory,
    IUserInteraction userInteraction,
    AppState appState) : ICommand
{
    public async Task ExecuteAsync()
    {
        var series = appState.ActiveSeries;
        var strategy = appState.ActiveStrategy;
        if (series is null || strategy is null)
        {
            userInteraction.InformAsync("Warning", "No active series. User /series command to select one.");
            return;
        }

        var lastSearchQuery = appState.LastSearchQuery;
        if (lastSearchQuery is null)
        {
            userInteraction.InformAsync("Warning", "No active query. User /query command to load data before tombstoning.");
            return;
        }

        var markedRows = appState.TombstoneMarkedRows;
        if (markedRows is null || markedRows.Count == 0)
        {
            userInteraction.InformAsync("Info", "No rows marked for tombstoning.");
            return;
        }

        var confirm = await userInteraction.ConfirmAsync($"Tombstone {markedRows.Count} row(s)?");
        if (!confirm) return;

        var repository = clickHouseRepositoryFactory.Create(series.SeriesType);
        await repository.TombstoneRows(strategy.Stage(markedRows));

        var updatedRows = await repository.GetData(series.SeriesKey, lastSearchQuery.Value);
        appState.SetLoadedRows(updatedRows, lastSearchQuery.Value);
    }
}