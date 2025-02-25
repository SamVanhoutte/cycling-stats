using Microsoft.EntityFrameworkCore;

namespace CyclingStats.DataAccess.Entities;

[PrimaryKey("RaceId", "RiderId")]
public class StartGridEntry
{
    public string? RiderId { get; set; }
    public string RaceId { get; set; }
    public int Stars { get; set; }
    public int RiderType { get; set; }
    public bool? Youth { get; set; }
    public DateTime Created { get; set; }
    public virtual Rider? Rider { get; set; }
    public virtual Game Game { get; set; }
}