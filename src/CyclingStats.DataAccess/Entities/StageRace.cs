using Microsoft.EntityFrameworkCore;

namespace CyclingStats.DataAccess.Entities;

[PrimaryKey("StageRaceId")]
public class StageRace
{
    public string StageRaceId { get; set; }
    public string? Name { get; set; }
    public int StageCount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}