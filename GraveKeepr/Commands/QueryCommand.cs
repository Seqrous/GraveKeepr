using GraveKeeper.Infrastructure;
using GraveKeeper.Query.Engine;
using GraveKeeper.UI;

namespace GraveKeeper.Commands;

public class QueryCommand(
    string args,
    ClickHouseRepositoryFactory clickHouseRepositoryFactory,
    IUserInteraction userInteraction,
    AppState appState) : ICommand
{
    private const ulong RowCountSoftThreshold = 100_000;
    private const ulong RowCountHardThreshold = 100_000_000;
    
    public async Task ExecuteAsync()
    {
        var series = appState.ActiveSeries;
        if (series is null)
            throw new InvalidOperationException("No active series. Please select a series before running a query.");

        var repository = clickHouseRepositoryFactory.Create(series.SeriesType);
        var engine = ClickHouseQueryEngine.Create(series.SeriesType);
        var compiledQuery = engine.Compile(args);

        var count = await repository.CountData(series.SeriesKey, compiledQuery);
        if (count == 0)
        {
            userInteraction.InformAsync("Info", "No data found");
            return;
        }

        if (count is >= RowCountSoftThreshold and < RowCountHardThreshold)
        {
            var proceed = await userInteraction.ConfirmAsync($"Query will return {count:N0} rows. Load anyway?");
            if (!proceed) return;
        }
        else if (count >= RowCountHardThreshold)
        {
            userInteraction.ErrorAsync("Warning", $"Query will return {count:N0} rows, please refine your query.");
            return;
        }

        var rows = await repository.GetData(series.SeriesKey, compiledQuery);
        appState.SetLoadedRows(rows, compiledQuery);
    }
}