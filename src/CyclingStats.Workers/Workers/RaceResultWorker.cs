using CyclingStats.DataAccess;
using CyclingStats.DataAccess.Entities;
using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Exceptions;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;
using CyclingStats.Models.Extensions;
using CyclingStats.Workers.Configuration;
using Microsoft.Extensions.Options;

namespace CyclingStats.Workers.Workers;

public class RaceResultWorker : BaseWorker<BatchConfig>
{
    private readonly ILogger<RaceResultWorker> logger;
    private readonly IDataRetriever resultCollector;

    public RaceResultWorker(
        ILogger<RaceResultWorker> logger,
        IDataRetriever resultCollector,
        IOptions<ScheduleOptions> scheduleOptions,
        IOptions<SqlOptions> sqlSettings) : base(scheduleOptions, sqlSettings)
    {
        this.logger = logger;
        this.resultCollector = resultCollector;
    }


    protected override string TaskDescription => "Collecting race results.";
    protected override string WorkerName => "RaceResults";
    protected override ILogger Logger => logger;

    protected override async Task<bool> ProcessAsync(CancellationToken stoppingToken, BatchConfig config)
    {
        try
        {
            ICollection<Race> races;
            await using (var ctx = StatsDbContext.CreateFromConnectionString(
                             sqlSettings.ConnectionString))
            {
                races = await ctx.GetAllRacesAsync(detailsCompleted: true, resultsRetrieved: false);
                // Only take past races and races with a category
                races = races.Where(r => r.RaceDate < DateTime.Now && !string.IsNullOrEmpty(r.UciScale)).ToList();
                races = races.Where(r => r.Updated < DateTime.Now.AddMinutes(-3)).ToList();
                if (config.RaceScales!=null)
                {
                    races = races.Where(r => config.RaceScales.Contains(r.UciScale.ToLower())).ToList();
                }


                if (config.BatchSize > 0)
                {
                    races = races.Take(config.BatchSize).ToList();
                }

                if (races.Any())
                {
                    foreach (var race in races)
                    {
                        try
                        {
                            var raceData = await resultCollector.GetRaceDataAsync(race);
                            foreach (var raceDetail in raceData)
                            {
                                try
                                {
                                    raceDetail.Results = await resultCollector.GetRaceResultsAsync(race, top: config.TopResults);
                                    if (raceDetail.Results.Any())
                                    {
                                        raceDetail.ResultsRetrieved = true;
                                        await ctx.UpsertRaceResultsAsync(raceDetail);
                                        logger.LogInformation(
                                            "Saved {ResultCount} results of {RaceName} to the data store",
                                            raceDetail.Results.Count(), raceDetail.Id);
                                    }
                                }
                                catch (NoResultsAvailableException e)
                                {
                                    await ctx.UpsertRaceDataAsync(raceDetail, RaceStatus.NoResultsAvailable);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            logger.LogError(e, "Error while scraping race results of {Race}: {Exception}", race.Id,
                                e.Message);
                            await ctx.MarkRaceAsErrorAsync(race.Id, e.ToString());
                        }
                    }
                }
                else
                {
                    return false;
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