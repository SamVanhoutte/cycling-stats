using CyclingStats.DataAccess;
using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;
using CyclingStats.Workers.Configuration;
using Microsoft.Extensions.Options;

namespace CyclingStats.Workers.Workers;

public class RaceStartListWorker : BaseWorker<BatchConfig>
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

    protected override async Task<bool> ProcessAsync(CancellationToken stoppingToken, BatchConfig config)
    {
        try
        {
            using (var ctx = CyclingDbContext.CreateFromConnectionString(
                       sqlSettings.ConnectionString))
            {
                var races = await ctx.GetAllRacesAsync(RaceStatus.Planned);
                foreach (var race in races)
                {
                    try
                    {
                        logger.LogInformation("Updating status for race {RaceId}", race.Id);
                        await ctx.UpdateRaceStatusAsync(race.Id, RaceStatus.Planned);
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

        return true;
    }
    
}