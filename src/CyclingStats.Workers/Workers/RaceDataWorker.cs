using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;
using CyclingStats.Workers.Configuration;
using Microsoft.Extensions.Options;

namespace CyclingStats.Workers.Workers;

public class RaceDataWorker(
    ILogger<RaceDataWorker> logger,
    IDataRetriever resultCollector,
    IRaceService raceService,
    IOptions<ScheduleOptions> scheduleOptions,
    IOptions<SqlOptions> sqlSettings)
    : BaseWorker<BatchConfig>(scheduleOptions, sqlSettings)
{
    protected override string TaskDescription => "Collecting race data.";
    protected override string WorkerName => "RaceData";
    protected override ILogger Logger => logger;

    protected override async Task<bool> ProcessAsync(CancellationToken stoppingToken, BatchConfig config)
    {
        try
        {
            // Get incompleted races
            var races = await raceService.GetRacesAsync(
                statusToExclude: RaceStatus.NotFound);
            if(!races.Any(r=>r.MarkForProcess))
            {
                // Filter out races that are further than 1 month in the future
                races = races.Where(r => r.Date == null || r.Date < DateTime.UtcNow.AddMonths(1)).ToList();
                races = races.Where(r => r.Status != RaceStatus.Error && r.Status != RaceStatus.Canceled).ToList();
                races = races.Where(r => (r.Updated == null || r.Updated?.AddMinutes(config.AgeMinutes) < DateTime.UtcNow)).ToList();
                races = races.Where(r => r.DetailsCompleted==false || r.Duration == null).ToList();
            }
            else
            {
                races = races.Where(r => r.MarkForProcess).ToList();
            }
            if (races.Any())
            {
                Console.WriteLine($"(Data) Found {races.Count} races to update");
                races = races.OrderBy(r => r.Updated).ToList();
                var race = races.First();
                try
                {
                    logger.LogInformation("(Data) Updating data for race {RaceId} - {Date}", race.Id, race.Date);

                    var raceData = await resultCollector.GetRaceDataAsync(race);
                    
                    // The race id does not seem to be found on PCS
                    if (raceData.FirstOrDefault() == null)
                    {
                        if (string.IsNullOrEmpty(race.PcsId) && !string.IsNullOrEmpty(race.Name))
                        {
                            var pcsId = await resultCollector.GetPcsRaceIdAsync(race);
                            if (!string.IsNullOrEmpty(pcsId))
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"(Data) PCS status looked up as {pcsId}");
                                Console.ResetColor();
                                logger.LogInformation($"PCS status looked up as {pcsId}");
                                race.Status = RaceStatus.New;
                                race.PcsId = pcsId;
                            }
                            else
                            {
                                race.Status = RaceStatus.NotFound;
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"(Data) The race {race.PcsRaceId} was not found");
                            Console.ResetColor();
                            logger.LogWarning($"The race {race.PcsRaceId} was not found");
                            race.Status = RaceStatus.NotFound;
                        }

                        await raceService.UpsertRaceDetailsAsync(race, false);
                    }
                    else
                    {
                        if (raceData.Count > 1)
                        {
                            race.Status = race.IsFinished ? RaceStatus.Finished : RaceStatus.Planned;
                            race.DetailsCompleted = true;
                            race.MarkForProcess = false;
                            await raceService.UpsertRaceDetailsAsync(race, false);
                        }

                        foreach (var raceDetail in raceData)
                        {
                            // Mark these as new to be imported for the next iteration
                            // raceDetail.Id = race.Id;
                            // raceDetail.PcsId = race.PcsId;
                            raceDetail.DetailsCompleted = true;
                            raceDetail.MarkForProcess = false;
                            await raceService.UpsertRaceDetailsAsync(raceDetail, string.IsNullOrEmpty(raceDetail.UciScale), race.IsFinished ? RaceStatus.Finished : RaceStatus.Planned);
                        }
                    }

                    logger.LogInformation("Updated data for race {RaceId}", race.Id);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"(Data) {e}");
                    logger.LogError(e, "Error while scraping race data of {Race}: {Exception}", race.Id, e.Message);
                }
            }
            else
            {
                Console.WriteLine("(Data) No new races found to update");
                return false;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            logger.LogError(e, "Error while scraping race data: {Exception}", e.Message);
        }

        return true;
    }
}