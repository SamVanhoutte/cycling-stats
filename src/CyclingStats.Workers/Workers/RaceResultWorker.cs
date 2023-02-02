using CyclingStats.DataAccess;
using CyclingStats.DataAccess.Entities;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models.Configuration;
using Microsoft.Extensions.Options;

namespace CyclingStats.Workers.Workers;

public class RaceResultWorker : BaseWorker
{
    private readonly ILogger<RaceResultWorker> logger;
    private readonly IDataRetriever resultCollector;

    public RaceResultWorker(
        ILogger<RaceResultWorker> logger,
        IDataRetriever resultCollector,
        IOptions<ScheduleOptions> scheduleOptions,
        IOptions<SqlOptions> sqlSettings) :base(scheduleOptions, sqlSettings)
    {
        this.logger = logger;
        this.resultCollector = resultCollector;
    }


    protected override string TaskDescription => "Collecting race results.";
    protected override string WorkerName => "RaceResults";
    protected override ILogger Logger => logger;

    protected override async Task ProcessAsync(CancellationToken stoppingToken)
    {
        try
        {
            using (var ctx = StatsDbContext.CreateFromConnectionString(
                       sqlSettings.ConnectionString))
            {
                var races = await ctx.GetAllRacesAsync(RaceStatus.WaitingForResults);
                foreach (var race in races.Where(r => r.RaceDate < DateTime.Now))
                {
                    try
                    {
                        var raceData = await resultCollector.GetRaceDataAsync(race.Id);
                        raceData.Results = await resultCollector.GetRaceResultsAsync(race.Id, 200);
                        if (raceData.Results.Any())
                        {
                            await ctx.UpsertRaceResultsAsync(raceData);
                            logger.LogInformation(
                                $"Saved {raceData.Results.Count()} results of {raceData.Name} to the data store");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        logger.LogError(e, "Error while scraping race results of {race}: {exception}", race.Id,
                            e.Message);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            logger.LogError(e, "Error while scraping race results: {exception}", e.Message);
        }
    }
}


// using CyclingStatsCollector.Data;
// using CyclingStatsCollector.Entities;
//
// namespace CyclingStatsCollector;
//
// public class WorkflowProcessor
// {
//     public static async Task ImportRaceDataAsync(string cnxString)
//     {
//         using (var ctx = StatsDbContext.CreateFromConnectionString(
//                    cnxString))
//         {
//             var races = await ctx.GetAllRacesAsync(RaceStatus.New);
//             foreach (var race in races)
//             {
//                 Console.WriteLine($"Updating data for race {race.Id}");
//                 var resultCollector = new StatsCollector();
//                 var raceData = await resultCollector.GetRaceDataAsync(race.Id);
//                 await ctx.UpsertRaceDataAsync(raceData, RaceStatus.WaitingForStartList);
//             }
//         }
//
//         Console.WriteLine("Finished");
//     }
//
//     public static async Task ImportRaceResultsAsync(string cnxString)
//     {

//     }
//
//
//     public static async Task ImportTeamsAsync(string cnxString)
//     {
//         var resultCollector = new StatsCollector();
//         foreach (var teamName in StatsCollector.TeamNames)
//         {
//             var riders = await resultCollector.GetTeamRidersAsync(teamName);
//             if (riders.Any())
//             {
//                 using (var ctx = StatsDbContext.CreateFromConnectionString(
//                            cnxString))
//                 {
//                     await ctx.UpsertRidersAsync(riders);
//                 }
//
//                 Console.WriteLine($"Saved {riders.Count()} riders of {teamName} to the data store");
//             }
//         }
//     }
// }