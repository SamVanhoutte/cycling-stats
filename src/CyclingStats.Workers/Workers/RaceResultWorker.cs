using CyclingStats.DataAccess;
using CyclingStats.DataAccess.Entities;
using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using Microsoft.Extensions.Options;

namespace CyclingStats.Workers.Workers;

public class RaceResultWorker : BaseWorker
{
    private readonly ILogger<RaceResultWorker> logger;
    private readonly IDataRetriever resultCollector;

    public RaceResultWorker(
        ILogger<RaceResultWorker> logger,
        IDataRetriever resultCollector,
        IOptions<ScheduleOptions> scheduleOptions,
        IOptions<SqlOptions> sqlSettings) :base(scheduleOptions, sqlSettings)
    {
        this.logger = logger;
        this.resultCollector = resultCollector;
    }


    protected override string TaskDescription => "Collecting race results.";
    protected override string WorkerName => "RaceResults";
    protected override ILogger Logger => logger;

    protected override async Task ProcessAsync(CancellationToken stoppingToken)
    {
        try
        {
            using (var ctx = StatsDbContext.CreateFromConnectionString(
                       sqlSettings.ConnectionString))
            {
                var races = await ctx.GetAllRacesAsync(RaceStatus.WaitingForResults);
                foreach (var race in races.Where(r => r.RaceDate < DateTime.Now))
                {
                    try
                    {
                        var raceData = await resultCollector.GetRaceDataAsync(race.Id);
                        raceData.Results = await resultCollector.GetRaceResultsAsync(race.Id, 200);
                        if (raceData.Results.Any())
                        {
                            await ctx.UpsertRaceResultsAsync(raceData);
                            logger.LogInformation(
                                "Saved {ResultCount} results of {RaceName} to the data store", raceData.Results.Count(), raceData.Name);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        logger.LogError(e, "Error while scraping race results of {Race}: {Exception}", race.Id,
                            e.Message);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            logger.LogError(e, "Error while scraping race results: {Exception}", e.Message);
        }
    }
}