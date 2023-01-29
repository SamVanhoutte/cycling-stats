namespace CyclingStatsCollector.Models;

public class RaceResults
{
    public string Name { get; set; }
    public string RaceType { get; set; }
    public string Date { get; set; }
    public List<RacerRaceResult> Results { get; set; }
}
public class RacerRaceResult
{
    public Rider Rider { get; set; }
    public int DelaySeconds { get; set; }
    public int Position { get; set; }
}

public class Rider
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Team { get; set; }
}