using CyclingStats.Models;
using CyclingStats.Models.Extensions;

namespace CyclingStats.WebApi.Models;

public class Race
{
    public string Id { get; set; }
    public string? Name { get; set; }
    public DateTime? Date { get; set; }
    public string? RaceType { get; set; }
    public decimal? Distance { get; set; }
    public int? Elevation { get; set; }
    public int? Duration { get; set; }
    public RaceStatus Status { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? PointsScale { get; set; }
    public string? UciScale { get; set; }
    public int? ParcoursType { get; set; }
    public int? ProfileScore { get; set; }
    public int? RaceRanking { get; set; }
    public int? StartlistQuality { get; set; }
    public string? DecidingMethod { get; set; }
    public string? Classification { get; set; }
    public int? Year { get; set; }
    public string? Category { get; set; }
    public bool? StageRace { get; set; }
    public string MainRaceId => StageRaceId ?? Id;
    public bool DetailsAvailable => Name != null && Date != DateTime.MinValue && Distance != 0;
    public string? PcsId { get; set; }
    public string? PcsUrl { get; set; }
    public string? WcsUrl { get; set; }
    public bool PointsRetrieved { get; set; }
    public bool? GameOrganized { get; set; }
    public bool ResultsRetrieved { get; set; }
    public bool StartListRetrieved { get; set; }
    public bool DetailsCompleted { get; set; }
    public string? StageRaceId { get; set; }

    public Race[]? Stages { get; set; }
    
    public static Race FromDomain(RaceDetails raceDetails)
    {
        return new Race
        {
            Id = raceDetails.Id.ToApiNotation()!,
            StageRaceId = raceDetails.StageRaceId.ToApiNotation(),
            Year = raceDetails.Id.GetYearValueFromRaceId(),
            Name = raceDetails.Name,
            Date = raceDetails.Date,
            RaceType = raceDetails.RaceType,
            Distance = raceDetails.Distance,
            Status = raceDetails.Status,
            ProfileImageUrl = raceDetails.ProfileImageUrl,
            PointsScale = raceDetails.PointsScale,
            UciScale = raceDetails.UciScale,
            ParcoursType = raceDetails.ParcoursType,
            ProfileScore = raceDetails.ProfileScore,
            RaceRanking = raceDetails.RaceRanking,
            Elevation = raceDetails.Elevation,
            Duration = raceDetails.Duration,
            DecidingMethod = raceDetails.DecidingMethod,
            Classification = raceDetails.Classification,
            Category = raceDetails.Category,
            PcsId = raceDetails.PcsId,
            PcsUrl = !string.IsNullOrEmpty( raceDetails.PcsId)?
                $"https://www.procyclingstats.com/race/{raceDetails.PcsId}":null,
            WcsUrl = $"https://www.worldcyclingstats.com/en/race/{raceDetails.Id}",
            PointsRetrieved = raceDetails.PointsRetrieved,
            GameOrganized = raceDetails.GameOrganized, DetailsCompleted = raceDetails.DetailsCompleted,
            ResultsRetrieved = raceDetails.ResultsRetrieved,
            StartlistQuality = raceDetails.StartlistQuality, StartListRetrieved = raceDetails.StartListRetrieved,
            StageRace = raceDetails.StageRace
        };
    }
    
    public static Race FromDomain(RaceDetails mainRace, RaceDetails[] stages)
    {
        var race = Race.FromDomain(mainRace);
        race.Stages = stages.Select(FromDomain).ToArray();
        race.Distance = stages.Sum(s => s.Distance);
        race.Duration = stages.Sum(s => s.Duration);
        race.Elevation = stages.Sum(s => s.Elevation);
        return race;
    }
}