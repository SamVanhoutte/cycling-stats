using CyclingStats.Models.Extensions;

namespace CyclingStats.Models;

public class RaceDetails
{
    public string Id { get; set; }
    public string? StageId { get; set; }
    public string? StageRaceId { get; set; }
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
    public int? Duration { get; set; }
    public int? Elevation { get; set; }
    public int? StartlistQuality { get; set; }
    public string? DecidingMethod { get; set; }
    public string? Classification { get; set; }
    public string? Category { get; set; }
    public List<RacerRaceResult>? Results { get; set; }
    public List<RacerRacePoint>? Points { get; set; }
    public bool StageRace =>  IsStageRace || (RaceType?.Contains("Multi-day race") ?? false);
    public bool IsStageRace { get; set; }
    public bool DetailsAvailable => Name != null && Date != DateTime.MinValue && Distance != 0;
    public string? PcsId { get; set; }
    public string? PcsUrl { get; set; }
    public string? WcsUrl { get; set; }
    public bool PointsRetrieved { get; set; }
    public bool? GameOrganized { get; set; }
    public bool ResultsRetrieved { get; set; }
    public bool StartListRetrieved { get; set; }
    public bool DetailsCompleted { get; set; }
    public bool MarkForProcess { get; set; }
    public bool IsFinished => !(DecidingMethod?.StartsWith('?')??false) && Date < DateTime.UtcNow;
    public bool IsTeamTimeTrial => PointsScale?.Contains("TTT", StringComparison.CurrentCultureIgnoreCase) ?? false;
    public DateTime? Updated { get; set; }
    public string? PcsRaceId
    {
        get
        {
            if (string.IsNullOrEmpty(PcsId)) return Id;
            var idYear = Id.GetYearValueFromRaceId();
            var pcsYear = PcsId.GetYearValueFromRaceId();
            if (idYear != null)
            {
                return pcsYear==null
                    ? PcsId + "/" + idYear 
                    : PcsId.Replace(pcsYear.ToString(), idYear.ToString());
            }
            return PcsId;
        }
        
        
    }



    public static string GetRaceIdFromUrl(string url, int? year = null)
    {
        if (string.IsNullOrEmpty(url)) return "";
        var paths = url.Split("/");
        paths = paths
            .SkipWhile(p => !p.Equals("race", StringComparison.InvariantCultureIgnoreCase))
            .Skip(1)
            .ToArray();
        
        if (!int.TryParse(paths.Last(), out _) && !paths.Last().StartsWith("stage", StringComparison.InvariantCultureIgnoreCase))
        {
            paths = paths.Append((year ?? DateTime.UtcNow.Year).ToString()).ToArray();
        }
        return string.Join('/', paths);
    }
    
    
}