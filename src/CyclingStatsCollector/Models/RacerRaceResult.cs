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
    private string id;

    public string Id
    {
        get => id;
        init => id = GetRiderIdFromUrl(value);
    }

    public static string GetRiderIdFromUrl(string url)
    {
        if (url.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
        {
            var paths = url.Split("/");
            if (url.EndsWith(DateTime.Today.Year.ToString()))
            {
                url = paths.SkipLast(1).LastOrDefault() ?? "";
            }
            else
            {
                url = paths.LastOrDefault() ?? "";
            }
        }

        return url;
    }

    public string Name { get; set; }
    public string Team { get; set; }
    public int Sprinter { get; set; }
    public int Puncheur { get; set; }
    public int OneDay { get; set; }
    public int Climber { get; set; }
    public int AllRounder { get; set; }
    public int TimeTrialist { get; set; }
}