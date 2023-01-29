using CyclingStatsCollector;

var url = "https://www.worldcyclingstats.com/en/race/tour-down-under/2023/stage-1";
url = "https://www.worldcyclingstats.com/nl/koers/vuelta-a-san-juan-internacional/2023/etappe-1";
var riderId = "sam-bennett";

Console.WriteLine($"We will retrieve results from {url}");

var resultCollector = new StatsCollector();
var team = "bora-hansgrohe";
var riders = await resultCollector.GetTeamRidersAsync(team);
foreach (var riderv in riders)
{
    Console.WriteLine($"{riderv.Name} - {riderv.Sprinter} - {riderv.Puncheur}");
}
return;
var race = await resultCollector.GetRaceDataAsync(url);
race.Results = await resultCollector.GetRaceResultsAsync(url, 4);
var rider = await resultCollector.GetRiderAsync(riderId);
Console.WriteLine(rider.Name);
Console.WriteLine(rider.Team);
Console.WriteLine(rider.Sprinter);
Console.WriteLine(rider.Puncheur);
return;
Console.WriteLine(race.Name);
Console.WriteLine(race.Date);
Console.WriteLine(race.RaceType);
foreach (var result in race.Results)
{
    Console.WriteLine($"{result.Rider.Id} : {result.DelaySeconds} seconds");
}