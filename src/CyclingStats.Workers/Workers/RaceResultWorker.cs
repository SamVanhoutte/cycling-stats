using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Exceptions;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;
using CyclingStats.Workers.Configuration;
using Microsoft.Extensions.Options;

namespace CyclingStats.Workers.Workers;

public class RaceResultWorker(
    ILogger<RaceResultWorker> logger,
    IRaceService raceService,
    IDataRetriever resultCollector,
    IOptions<ScheduleOptions> scheduleOptions,
    IOptions<SqlOptions> sqlSettings)
    : BaseWorker<BatchConfig>(scheduleOptions, sqlSettings)
{
    protected override string TaskDescription => "Collecting race results.";
    protected override string WorkerName => "RaceResults";
    protected override ILogger Logger => logger;

    protected override async Task<bool> ProcessAsync(CancellationToken stoppingToken, BatchConfig config)
    {
        try
        {
            ICollection<RaceDetails> races;
            races = await raceService.GetRacesAsync(detailsCompleted: true, resultsRetrieved: false);
            // Only take past races and races with a category
            races = races.Where(r => r.IsFinished).ToList();
            races = races.Where(r => r.Updated < DateTime.Now.AddMinutes(-config.AgeMinutes)).ToList();
            if (config.RaceScales != null)
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
                                var output = await resultCollector.GetRaceResultsAsync(race, top: config.TopResults);
                                if (raceDetail != null)
                                {
                                    raceDetail.Duration = output.Item1;
                                    raceDetail.Results = output.Item2;
                                    if (raceDetail.Results.Any())
                                    {
                                        raceDetail.ResultsRetrieved = true;
                                        await raceService.UpsertRaceResultsAsync(raceDetail);
                                        logger.LogInformation(
                                            "Saved {ResultCount} results of {RaceName} to the data store",
                                            raceDetail.Results.Count(), raceDetail.Id);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("RaceDetail not found");
                                    await raceService.UpsertRaceDetailsAsync(race, RaceStatus.NotFound);
                                }
                            }
                            catch (NoResultsAvailableException e)
                            {
                                await raceService.UpsertRaceDetailsAsync(raceDetail, RaceStatus.Finished);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        logger.LogError(e, "Error while scraping race results of {Race}: {Exception}", race.Id,
                            e.Message);
                        await raceService.MarkRaceAsErrorAsync(race.Id, e.ToString());
                    }
                }
            }
            else
            {
                return false;
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