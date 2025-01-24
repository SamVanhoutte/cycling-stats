using CyclingStats.DataAccess;
using CyclingStats.DataAccess.Entities;
using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;
using CyclingStats.Models.Extensions;
using CyclingStats.Workers.Configuration;
using Microsoft.Extensions.Options;

namespace CyclingStats.Workers.Workers;

public class RaceDataWorker : BaseWorker<BatchConfig>
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

    protected override async Task<bool> ProcessAsync(CancellationToken stoppingToken, BatchConfig config)
    {
        try
        {
            using (var ctx = StatsDbContext.CreateFromConnectionString(
                       sqlSettings.ConnectionString))
            {
                // Get incompleted races
                var races = await ctx.GetAllRacesAsync(detailsCompleted:false, statusToExclude: RaceStatus.NotFound);
                // Filter out races that are further than 1 month in the future
                races = races.Where(r=>r.RaceDate == null || r.RaceDate < DateTime.Now.AddMonths(1)).ToList();
                races = races.Where(r=>r.Status!= RaceStatus.Error &&r.Status!= RaceStatus.Canceled ).ToList();
                races = races.Where(r => (r.Updated == null || r.Updated?.AddHours(config.HoursAge) < DateTime.Now)).ToList();
                if (races.Any())
                {
                    var race = races.First();
                    try
                    {
                        logger.LogInformation("Updating data for race {RaceId}", race.Id);

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
                                    Console.WriteLine($"PCS status looked up as {pcsId}");
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
                                Console.WriteLine($"The race {race.PcsRaceId} was not found");
                                Console.ResetColor();
                                logger.LogWarning($"The race {race.PcsRaceId} was not found");
                                race.Status = RaceStatus.NotFound;
                            }

                            ctx.Update(race);
                        }
                        else
                        {
                            if (raceData.Count > 1)
                            {
                                // It's a stage race, so we update accordingly
                                race.IsStageRace = true;
                                // stageRace = new StageRace
                                // {
                                //     StageRaceId = race.Id, StageCount = raceData.Count, Name = race.Name,
                                //     StartDate = raceData.Min(rc => rc.Date), EndDate = raceData.Max(rc => rc.Date)
                                // };
                                // race.StageRace = stageRace;
                                race.Status = string.IsNullOrEmpty( race.ProfileImageUrl) ? RaceStatus.WaitingForDetails:  RaceStatus.WaitingForStartList;
                                race.DetailsCompleted = true;
                                ctx.Update(race);
                            }

                            foreach (var raceDetail in raceData)
                            {
                                // Mark these as new to be imported for the next iteration
                                // raceDetail.Id = race.Id;
                                // raceDetail.PcsId = race.PcsId;
                                raceDetail.DetailsCompleted = !string.IsNullOrEmpty(raceDetail.ProfileImageUrl);
                                await ctx.UpsertRaceDataAsync(raceDetail,
                                    string.IsNullOrEmpty(raceDetail.Classification)
                                        ? RaceStatus.WaitingForDetails
                                        : RaceStatus.WaitingForStartList);
                            }
                        }

                        await ctx.SaveChangesAsync(stoppingToken);

                        logger.LogInformation("Updated data for race {RaceId}", race.Id);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        logger.LogError(e, "Error while scraping race data of {Race}: {Exception}", race.Id, e.Message);
                    }
                }
                else
                {
                    Console.WriteLine("No new races found to update");
                    return false;
                }
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