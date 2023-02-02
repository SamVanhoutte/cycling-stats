namespace CyclingStatsCollector.Models;

public class RaceResults
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string RaceType { get; set; }
    public DateTime Date { get; set; }
    public decimal Distance { get; set; }
    public List<RacerRaceResult> Results { get; set; }
}