using CyclingStats.DataAccess;
using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Exceptions;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;
using CyclingStats.Models.Extensions;
using CyclingStats.Workers.Configuration;
using Microsoft.Extensions.Options;

namespace CyclingStats.Workers.Workers;

public class RacePointsWorker : BaseWorker<BatchConfig>
{
    private readonly ILogger<RacePointsWorker> logger;
    private readonly IDataRetriever resultCollector;

    public RacePointsWorker(
        ILogger<RacePointsWorker> logger,
        IDataRetriever resultCollector,
        IOptions<ScheduleOptions> scheduleOptions,
        IOptions<SqlOptions> sqlSettings) : base(scheduleOptions, sqlSettings)
    {
        this.logger = logger;
        this.resultCollector = resultCollector;
    }


    protected override string TaskDescription => "Collecting game points.";
    protected override string WorkerName => "RacePoints";
    protected override ILogger Logger => logger;

    protected override async Task<bool> ProcessAsync(CancellationToken stoppingToken,BatchConfig config)
    {
        try
        {
            using (var ctx = StatsDbContext.CreateFromConnectionString(
                       sqlSettings.ConnectionString))
            {
                var races = await ctx.GetAllRacesAsync(RaceStatus.Finished, pointsRetrieved: false,
                    detailsCompleted: true, statusToExclude: RaceStatus.NotFound);
                // Only take finished races with PointsRetrieved set to false
                races = races.Where(r => r.RaceDate < DateTime.Now).ToList();
                if (config.RaceScales!=null)
                {
                    races = races.Where(r => config.RaceScales.Contains(r.UciScale.ToLower())).ToList();
                }

                if (config.BatchSize > 0)
                {
                    races = races.Take(config.BatchSize).ToList();
                }
                races = races.Where(r => (r.Updated == null || r.Updated?.AddHours(config.HoursAge) < DateTime.Now)).ToList();

                if (!races.Any())
                {
                    return false;
                }
                foreach (var race in races)
                {
                    try
                    {
                        var raceData = await resultCollector.GetRaceDataAsync(race);
                        foreach (var raceDetail in raceData)
                        {
                            try
                            {
                                raceDetail.Points = await resultCollector.GetRacePointsAsync(race, top: 200);
                                if (raceDetail.Points.Any())
                                {
                                    raceDetail.PointsRetrieved = true;
                                    await ctx.UpsertRacePointsAsync(raceDetail);
                                    logger.LogInformation(
                                        "Saved {ResultCount} points of {RaceName} to the data store",
                                        raceDetail.Points.Count(), raceDetail.Id);
                                }
                            }
                            catch (GameNotEnabledException e)
                            {
                                raceDetail.PointsRetrieved = true;
                                raceDetail.GameOrganized = false;
                                await ctx.UpsertRaceDataAsync(raceDetail, raceDetail.Status);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        logger.LogError(e, "Error while scraping race points of {Race}: {Exception}", race.Id,
                            e.Message);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            logger.LogError(e, "Error while scraping race points: {Exception}", e.Message);
        }

        return true;
    }
}