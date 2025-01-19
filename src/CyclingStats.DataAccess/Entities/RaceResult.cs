using Microsoft.EntityFrameworkCore;

namespace CyclingStats.DataAccess.Entities;

[PrimaryKey("RiderId", "RaceId")]
public class RaceResult
{
    public string RiderId { get; set; }
    public Rider Rider { get; set; }
    public string RaceId { get; set; }
    public Race Race { get; set; }
    public int Position { get; set; }
    public int Gap { get; set; }
}