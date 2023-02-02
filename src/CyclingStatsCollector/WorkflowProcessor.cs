using CyclingStatsCollector.Data;
using CyclingStatsCollector.Entities;

namespace CyclingStatsCollector;

public class WorkflowProcessor
{
    public static async Task ImportRaceDataAsync(string cnxString)
    {
        using (var ctx = StatsDbContext.CreateFromConnectionString(
                   cnxString))
        {
            var races = await ctx.GetAllRacesAsync(RaceStatus.New);
            foreach (var race in races)
            {
                Console.WriteLine($"Updating data for race {race.Id}");
                var resultCollector = new StatsCollector();
                var raceData = await resultCollector.GetRaceDataAsync(race.Id);
                await ctx.UpsertRaceDataAsync(raceData, RaceStatus.WaitingForStartList);
            }
        }

        Console.WriteLine("Finished");
    }

    public static async Task ImportRaceResultsAsync(string cnxString)
    {
        var resultCollector = new StatsCollector();
        using (var ctx = StatsDbContext.CreateFromConnectionString(
                   cnxString))
        {
            var races = await ctx.GetAllRacesAsync(RaceStatus.WaitingForResults);
            foreach (var race in races.Where(r=>r.RaceDate<DateTime.Now))
            {
                var raceData = await resultCollector.GetRaceDataAsync(race.Id);
                raceData.Results = await resultCollector.GetRaceResultsAsync(race.Id, 200);
                if (raceData.Results.Any())
                {
                    await ctx.UpsertRaceResultsAsync(raceData);

                    Console.WriteLine($"Saved {raceData.Results.Count()} results of {raceData.Name} to the data store");
                }
            }
        }
    }


    public static async Task ImportTeamsAsync(string cnxString)
    {
        var resultCollector = new StatsCollector();
        foreach (var teamName in StatsCollector.TeamNames)
        {
            var riders = await resultCollector.GetTeamRidersAsync(teamName);
            if (riders.Any())
            {
                using (var ctx = StatsDbContext.CreateFromConnectionString(
                           cnxString))
                {
                    await ctx.UpsertRidersAsync(riders);
                }

                Console.WriteLine($"Saved {riders.Count()} riders of {teamName} to the data store");
            }
        }
    }
}