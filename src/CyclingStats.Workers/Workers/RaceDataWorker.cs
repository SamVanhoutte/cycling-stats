using CyclingStats.DataAccess;
using CyclingStats.DataAccess.Entities;
using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Exceptions;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;
using Microsoft.Extensions.Options;

namespace CyclingStats.Workers.Workers;

public class RaceDataWorker : BaseWorker
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

    protected override async Task ProcessAsync(CancellationToken stoppingToken)
    {
        try
        {
            using (var ctx = StatsDbContext.CreateFromConnectionString(
                       sqlSettings.ConnectionString))
            {
                var races = await ctx.GetAllRacesAsync(RaceStatus.New);
                races = races.Where(r => (r.Updated?.AddHours(1) < DateTime.Now)).ToList();
                if (races.Any())
                {
                    var race = races.First();
                    try
                    {
                        logger.LogInformation("Updating data for race {RaceId}", race.Id);

                        var raceData = await resultCollector.GetRaceDataAsync(race);
                        if (raceData.FirstOrDefault() == null)
                        {
                            Console.WriteLine($"The race {race.PcsRaceId} was not found");
                            if (string.IsNullOrEmpty(race.PcsId) && !string.IsNullOrEmpty(race.Name))
                            {
                                var pcsId = await resultCollector.GetPcsIdAsync(race);
                                if (!string.IsNullOrEmpty(pcsId))
                                {
                                    logger.LogDebug($"PCS status looked up as {pcsId}");
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
                                race.Status = RaceStatus.NotFound;
                            }

                            ctx.Update(race);
                        }
                        else
                        {
                            StageRace stageRace = null;
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
                                race.Status = RaceStatus.WaitingForStartList;
                                ctx.Update(race);
                            }

                            foreach (var raceDetail in raceData)
                            {
                                // Mark these as new to be imported for the next iteration
                                // raceDetail.Id = race.Id;
                                // raceDetail.PcsId = race.PcsId;
                                await ctx.UpsertRaceDataAsync(raceDetail,
                                    string.IsNullOrEmpty(raceDetail.Classification)
                                        ? RaceStatus.New
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
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            logger.LogError(e, "Error while scraping race data: {Exception}", e.Message);
        }
    }
}