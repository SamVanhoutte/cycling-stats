using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CyclingStatsCollector.Entities;

[PrimaryKey("Id")]
public class Race
{
    public string Id { get; set; }
    public string? Name { get; set; }
    public DateTime? RaceDate { get; set; }
    public string? RaceType { get; set; }
    public decimal? Distance { get; set; }
    [Column(TypeName = "nvarchar(20)")]
    public RaceStatus Status { get; set; }
    
    public ICollection<Result>? Results { get; set; }
}