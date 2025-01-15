using CyclingStats.DataAccess;
using CyclingStats.DataAccess.Entities;
using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using Microsoft.Extensions.Options;

namespace CyclingStats.Workers.Workers;

public class RaceDataWorker : BaseWorker
{
    private readonly ILogger<RaceDataWorker> logger;
    private readonly IDataRetriever resultCollector;

    public RaceDataWorker(
        ILogger<RaceDataWorker> logger,
        IDataRetriever resultCollector,
        IOptions<ScheduleOptions> scheduleOptions,
        IOptions<SqlOptions> sqlSettings) : base(scheduleOptions, sqlSettings)
    {
        this.logger = logger;
        this.resultCollector = resultCollector;
    }

    protected override string TaskDescription => "Collecting race data.";
    protected override string WorkerName => "RaceData";
    protected override ILogger Logger => logger;

    protected override async Task ProcessAsync(CancellationToken stoppingToken)
    {
        try
        {
            using (var ctx = StatsDbContext.CreateFromConnectionString(
                       sqlSettings.ConnectionString))
            {
                var races = await ctx.GetAllRacesAsync(RaceStatus.New);
                foreach (var race in races)
                {
                    try
                    {
                        logger.LogInformation("Updating data for race {RaceId}", race.Id);
                        var raceData = await resultCollector.GetRaceDataAsync(race.Id);
                        await ctx.UpsertRaceDataAsync(raceData, RaceStatus.WaitingForStartList);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        logger.LogError(e, "Error while scraping race data of {Race}: {Exception}", race.Id, e.Message);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            logger.LogError(e, "Error while scraping race data: {Exception}", e.Message);
        }
    }
}