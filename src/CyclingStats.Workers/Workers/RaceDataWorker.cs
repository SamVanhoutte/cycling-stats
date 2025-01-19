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
                if (races.Any())
                {
                    var race = races.First();
                    try
                    {
                        logger.LogInformation("Updating data for race {RaceId}", race.Id);

                        var raceData = await resultCollector.GetRaceDataAsync(race.Id, DateTime.Now.Year);
                        if (raceData.FirstOrDefault() == null)
                        {
                            Console.WriteLine($"The race {race.Id} was not found");
                            race.Status = RaceStatus.NotFound;
                            ctx.Update(race);
                        }
                        else
                        {
                            StageRace stageRace = null;
                            if (raceData.Count > 1)
                            {
                                // It's a stage race, so we update accordingly
                                race.IsStageRace = true;
                                stageRace = new StageRace
                                {
                                    StageRaceId = race.Id, StageCount = raceData.Count, Name = race.Name,
                                    StartDate = raceData.Min(rc => rc.Date), EndDate = raceData.Max(rc => rc.Date)
                                };
                                race.StageRace = stageRace;
                                race.Status = RaceStatus.WaitingForStartList;
                                ctx.Update(race);
                            }

                            foreach (var raceDetail in raceData)
                            {
                                // Mark these as new to be imported for the next iteration
                                await ctx.UpsertRaceDataAsync(raceDetail, (raceDetail.StageRace??false) ? RaceStatus.New : RaceStatus.WaitingForStartList);
                            }
                        }
                        logger.LogInformation("Updated data for race {RaceId}", race.Id);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        logger.LogError(e, "Error while scraping race data of {Race}: {Exception}", race.Id, e.Message);
                    }
                }

                await ctx.SaveChangesAsync(stoppingToken);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            logger.LogError(e, "Error while scraping race data: {Exception}", e.Message);
        }
    }
}