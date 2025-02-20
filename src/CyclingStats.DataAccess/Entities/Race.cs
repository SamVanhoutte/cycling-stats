using System.ComponentModel.DataAnnotations.Schema;
using CyclingStats.Models;
using Microsoft.EntityFrameworkCore;

namespace CyclingStats.DataAccess.Entities;

[PrimaryKey("Id")]
public class Race
{
    public string Id { get; set; }
    public string? PcsId { get; set; }
    [Column("StageRace")]
    public bool IsStageRace { get; set; }
    public string? Name { get; set; }
    public DateTime? RaceDate { get; set; }
    public string? RaceType { get; set; }
    public decimal? Distance { get; set; }
    [Column(TypeName = "nvarchar(20)")]
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
    public string? Error { get; set; }
    public bool PointsRetrieved { get; set; }
    public bool ResultsRetrieved { get; set; }
    public bool StartListRetrieved { get; set; }
    public bool DetailsCompleted { get; set; }
    public bool? MarkForProcess { get; set; }
    public ICollection<RaceResult>? Results { get; set; }
    public string? StageRaceId { get; set; }
    public string? StageId { get; set; }
    public DateTime? Updated { get; set; }
    public string? PcsUrl { get; set; }
    public string? WcsUrl { get; set; }
    public bool? GameOrganized { get; set; }
    public int? Duration { get; set; }
}