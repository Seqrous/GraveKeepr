using System.Net;
using ClickHouse.Driver.ADO;
using GraveKeeper;
using GraveKeeper.Commands;
using GraveKeeper.Infrastructure;
using GraveKeeper.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

#region DI

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();
    
var connString = configuration.GetConnectionString("ClickHouse") ?? throw new Exception("Missing connection string");

var services = new ServiceCollection();
services.AddClickHouseDataSource(
    connectionString: connString,
    new HttpClient(
        new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            MaxConnectionsPerServer = 1
        }
    )
);

var serviceProvider = services.BuildServiceProvider();

#endregion

var dataFlowStrategyFactory = new DataFlowStrategyFactory();
AppState appState = new(dataFlowStrategyFactory);

var metadataFilePath = Path.Combine(AppContext.BaseDirectory, "metadata.json");
var seriesMetadataRepository = new MetadataClient(metadataFilePath);

var connection = serviceProvider.GetRequiredService<ClickHouseConnection>();
var repositoryFactory = new ClickHouseRepositoryFactory(connection);

var app = Application.Create();
app.Init();
app.Mouse.IsMouseDisabled = true;

var userInteraction = new DefaultUserInteraction(app);
var commandParser = new CommandParser(seriesMetadataRepository, repositoryFactory, userInteraction, appState);

var window = new Window();
window.Padding!.Thickness = new Thickness(1, 0, 1, 0);
window.Title = "GraveKeepr";

var statusBar = new AppStatusBar { X = 0, Y = 0, Width = Dim.Fill() };
var commandBar = new CommandBar { X = 0, Y = Pos.Bottom(statusBar) + 1, Width = Dim.Fill() };
var tableView = new DataOverviewTable
{
    X = 0,
    Y = Pos.Bottom(commandBar) + 1,
    Width = Dim.Fill(),
    Height = Dim.Fill()
};

window.Add(statusBar, commandBar, tableView);

appState.SeriesChanged += (_, metadata) =>
    app.Invoke(() => statusBar.Update($"Series: {metadata.SeriesId} | Type: {metadata.SeriesType} | Flow: {metadata.DataFlowType}"));

commandBar.CommandEntered += async (_, metadata) =>
{
    try
    {
        var command = commandParser.Parse(metadata);
        await command.ExecuteAsync();
    }
    catch (InvalidOperationException ex)
    {
        userInteraction.ErrorAsync("Error", ex.Message);
    }
};
    
appState.RowsLoaded += (_, rows) =>
{
    var series = appState.ActiveSeries;
    var strategy = appState.ActiveStrategy;
    if (series is null || strategy is null)
    {
        userInteraction.ErrorAsync("Error", "No active series. Please select a series before running a query");
        return;
    }
    
    app.Invoke(() =>
    {
        statusBar.Update($"Series: {series.SeriesId} | Type: {series.SeriesType} | Flow: {series.DataFlowType}");
        tableView.LoadData(strategy.CreateViewModels(rows));
    });
};

tableView.TombstoneMarkedRowsChanged = appState.SetTombstoneMarkedRows;

app.Run(window);