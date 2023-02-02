using CyclingStatsCollector;
using CyclingStatsCollector.Data;

try
{
    var cnxString = args.Last();
    switch(args[0].ToLower())
    {
        case "teams":
            await WorkflowProcessor.ImportTeamsAsync(cnxString);
            break;
        case "race-data":
            await WorkflowProcessor.ImportRaceDataAsync(cnxString);
            break;
        case "race-results":
            await WorkflowProcessor.ImportRaceResultsAsync(cnxString);
            break;
    }

    Console.WriteLine("Finished");
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
    Console.WriteLine();
    Console.WriteLine(e.Message);
}


