using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Exceptions;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;
using CyclingStats.Workers.Configuration;
using Microsoft.Extensions.Options;

namespace CyclingStats.Workers.Workers;

public class RacePointsWorker(
    ILogger<RacePointsWorker> logger,
    IDataRetriever resultCollector,
    IRaceService raceService,
    IOptions<ScheduleOptions> scheduleOptions,
    IOptions<SqlOptions> sqlSettings)
    : BaseWorker<BatchConfig>(scheduleOptions, sqlSettings)
{
    protected override string TaskDescription => "Collecting game points.";
    protected override string WorkerName => "RacePoints";
    protected override ILogger Logger => logger;

    protected override async Task<bool> ProcessAsync(CancellationToken stoppingToken, BatchConfig config)
    {
        try
        {
            var races = await raceService.GetRacesAsync(RaceStatus.Finished, pointsRetrieved: false,
                statusToExclude: RaceStatus.NotFound);
            // Only take finished races with PointsRetrieved set to false
            races = races.Where(r => r.IsFinished).ToList();
            if (config.RaceScales != null)
            {
                races = races.Where(r => config.RaceScales.Contains(r.UciScale?.ToLower() ?? string.Empty)).ToList();
            }

            if (config.BatchSize > 0)
            {
                races = races.Take(config.BatchSize).ToList();
            }

            races = races.Where(r => (r.Updated == null || r.Updated?.AddMinutes(config.AgeMinutes) < DateTime.Now))
                .ToList();

            races = races.Where(r => !r.IsTeamTimeTrial).ToList();

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
                                await raceService.UpsertRacePointsAsync(raceDetail);
                                logger.LogInformation(
                                    "Saved {ResultCount} points of {RaceName} to the data store",
                                    raceDetail.Points.Count(), raceDetail.Id);
                            }
                        }
                        catch (NoPointsAvailableException nrex)
                        {
                            raceDetail.PointsRetrieved = true;
                            raceDetail.GameOrganized = false;
                            await raceService.UpsertRaceDetailsAsync(raceDetail, false);
                        }
                        catch (GameNotEnabledException e)
                        {
                            raceDetail.PointsRetrieved = true;
                            raceDetail.GameOrganized = false;
                            await raceService.UpsertRaceDetailsAsync(raceDetail, false);
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
        catch (Exception e)
        {
            Console.WriteLine(e);
            logger.LogError(e, "Error while scraping race points: {Exception}", e.Message);
        }

        return true;
    }
}