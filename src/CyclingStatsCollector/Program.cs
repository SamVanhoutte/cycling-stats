using CyclingStatsCollector;
using CyclingStatsCollector.Data;

try
{
    var cnxString = args[0];
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
            break;
        }
    }

    Console.WriteLine("Finished");
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
    Console.WriteLine();
    Console.WriteLine(e.Message);
}