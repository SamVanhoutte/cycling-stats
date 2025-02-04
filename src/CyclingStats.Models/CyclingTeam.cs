namespace CyclingStats.Models;

public class CyclingTeam
{
    private string id;

    public string Id
    {
        get => id;
        set => id = GetTeamIdFromUrl(value);
    }

    public static string GetTeamIdFromUrl(string url)
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