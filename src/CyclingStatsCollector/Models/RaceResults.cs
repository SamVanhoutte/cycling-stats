namespace CyclingStatsCollector.Models;

public class RaceResults
{
    public string Name { get; set; }
    public string RaceType { get; set; }
    public string Date { get; set; }
    public List<RacerRaceResult> Results { get; set; }
}