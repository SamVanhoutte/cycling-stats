using Microsoft.EntityFrameworkCore;

namespace CyclingStats.DataAccess.Entities;

[PrimaryKey("RiderId", "RaceId")]
public class RacePoint
{
    public string RiderId { get; set; }
    public Rider Rider { get; set; }
    public string RaceId { get; set; }
    public Race Race { get; set; }
    public int Points { get; set; }
    public int Position { get; set; }
    public int? Pc { get; set; }
    public int? Mc { get; set; }
    public decimal? Picked { get; set; }
    public int Stars { get; set; }
    public int? Gc { get; set; }
}