using CyclingStats.DataAccess;
using CyclingStats.DataAccess.Entities;
using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using Microsoft.Extensions.Options;

namespace CyclingStats.Workers.Workers;

public class RaceStartListWorker : BaseWorker
{
    private readonly ILogger<RaceStartListWorker> logger;
    private readonly IDataRetriever resultCollector;

    public RaceStartListWorker(
        ILogger<RaceStartListWorker> logger,
        IDataRetriever resultCollector,
        IOptions<ScheduleOptions> scheduleOptions,
        IOptions<SqlOptions> sqlSettings) : base(scheduleOptions, sqlSettings)
    {
        this.logger = logger;
        this.resultCollector = resultCollector;
    }

    protected override string TaskDescription => "Checking if races have a start list.";
    protected override string WorkerName => "RaceStartList";
    protected override ILogger Logger => logger;

    protected override async Task ProcessAsync(CancellationToken stoppingToken)
    {
        try
        {
            using (var ctx = StatsDbContext.CreateFromConnectionString(
                       sqlSettings.ConnectionString))
            {
                var races = await ctx.GetAllRacesAsync(RaceStatus.WaitingForStartList);
                foreach (var race in races)
                {
                    try
                    {
                        logger.LogInformation("Updating status for race {RaceId}", race.Id);
                        await ctx.UpdateRaceStatusAsync(race.Id, RaceStatus.WaitingForResults);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        logger.LogError(e, "Error while updating race status of {Race}: {Exception}", race.Id, e.Message);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            logger.LogError(e, "Error while updating race status: {Exception}", e.Message);
        }
    }
}