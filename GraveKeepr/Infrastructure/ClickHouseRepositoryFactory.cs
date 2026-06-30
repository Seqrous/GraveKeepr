using ClickHouse.Driver.ADO;
using GraveKeeper.Domain;

namespace GraveKeeper.Infrastructure;

public class ClickHouseRepositoryFactory(ClickHouseConnection connection)
{
    public IClickHouseRepository Create(SeriesType seriesType) =>
        seriesType switch
        {
            SeriesType.Forecast => new ForecastClickHouseRepository(connection),
            SeriesType.Actual => new ActualClickHouseRepository(connection),
            _ => throw new InvalidOperationException($"Unsupported series type: {seriesType}")
        };
}