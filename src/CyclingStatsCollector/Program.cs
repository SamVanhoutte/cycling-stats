using CyclingStatsCollector;

var url = "https://www.worldcyclingstats.com/en/race/tour-down-under/2023/stage-1";
url = "https://www.worldcyclingstats.com/nl/koers/vuelta-a-san-juan-internacional/2023/etappe-1";
Console.WriteLine($"We will retrieve results from {url}");

var resultCollector = new RaceResultCollector();
var race = await resultCollector.GetRaceDataAsync(url);
race.Results = await resultCollector.GetRaceResultsAsync(url, 10);

Console.WriteLine(race.Name);
Console.WriteLine(race.Date);
Console.WriteLine(race.RaceType);
foreach (var result in race.Results)
{
    Console.WriteLine($"{result.Rider.Name} : {result.DelaySeconds} seconds");
}