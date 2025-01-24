using CyclingStats.DataAccess;
using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;
using CyclingStats.Workers.Configuration;
using Microsoft.Extensions.Options;

namespace CyclingStats.Workers.Workers;

public class RaceIdMatchWorker : BaseWorker<BatchConfig>
{
    private readonly ILogger<RaceIdMatchWorker> logger;
    private readonly IDataRetriever resultCollector;

    public RaceIdMatchWorker(
        ILogger<RaceIdMatchWorker> logger,
        IDataRetriever resultCollector,
        IOptions<ScheduleOptions> scheduleOptions,
        IOptions<SqlOptions> sqlSettings) : base(scheduleOptions, sqlSettings)
    {
        this.logger = logger;
        this.resultCollector = resultCollector;
    }
    
    protected override string TaskDescription => "Tries to get the PCS id for races that are not found";
    protected override string WorkerName => nameof(RaceIdMatchWorker);
    protected override ILogger Logger => logger;
    protected override async Task<bool> ProcessAsync(CancellationToken stoppingToken, BatchConfig config)
    {
        try
        {
            using (var ctx = StatsDbContext.CreateFromConnectionString(
                       sqlSettings.ConnectionString))
            {
                var races = await ctx.GetAllRacesAsync(RaceStatus.NotFound);
                foreach (var race in races)
                {
                    logger.LogInformation("Getting id for race {RaceId}", race.Id);

                    try
                    {
                        var pcsId = await resultCollector.GetPcsRaceIdAsync(race);
                        if (string.IsNullOrEmpty(pcsId))
                        {
                            race.Status = RaceStatus.Error;
                            race.Error = "PCS Id Not Found";
                            Console.WriteLine($"No PCS id found for race {race.Id}");
                        }
                        else
                        {
                            Console.WriteLine($"PCS id found for race {race.Id}: {pcsId}");;
                            race.PcsId = pcsId;
                            race.Status = RaceStatus.New;
                        }
                        ctx.Races.Update(race);
                        await ctx.SaveChangesAsync(stoppingToken);
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

        return true;
    }
}