namespace CyclingStats.Models;

public class RaceDetails
{
    public string Id { get; set; }
    public string? StageId { get; set; }
    public string? Name { get; set; }
    public DateTime? Date { get; set; }
    public string? RaceType { get; set; }
    public decimal? Distance { get; set; }
    public RaceStatus Status { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? PointsScale { get; set; }
    public string? UciScale { get; set; }
    public int? ParcoursType { get; set; }
    public int? ProfileScore { get; set; }
    public int? RaceRanking { get; set; }
    public int? Elevation { get; set; }
    public int? StartlistQuality { get; set; }
    public string? DecidingMethod { get; set; }
    public string? Classification { get; set; }
    public string? Category { get; set; }
    public List<RacerRaceResult>? Results { get; set; }
    public List<RacerRacePoint>? Points { get; set; }
    public bool? StageRace => RaceType?.Contains("Multi-day race");
    public bool DetailsAvailable => Name != null && Date != DateTime.MinValue && Distance != 0;
    public string? PcsId { get; set; }
    public string? PcsUrl { get; set; }
    public string? WcsUrl { get; set; }

    public static string GetRaceIdFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return "";
        var paths = url.Split("/");
        paths = paths
            .SkipWhile(p => !p.Equals("race", StringComparison.InvariantCultureIgnoreCase))
            .Skip(1)
            .ToArray();
        
        if (!int.TryParse(paths.Last(), out _) && !paths.Last().StartsWith("stage", StringComparison.InvariantCultureIgnoreCase))
        {
            paths = paths.Append(DateTime.UtcNow.Year.ToString()).ToArray();
        }
        return string.Join('/', paths);
        // if (url.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
        // {
        //     var paths = url.Split("/");
        //     // Check if the url ends with a year
        //     var suffix = url[^4..];
        //     if (int.TryParse(suffix, out _))
        //     {
        //         url = paths.SkipLast(1).LastOrDefault() ?? "";
        //     }
        //     else
        //     {
        //         url = paths.LastOrDefault() ?? "";
        //     }
        // }
        // // if(url.)
        // var subPaths = url.Split("/");
        // return subPaths.LastOrDefault() ?? "";
    }
}