using Microsoft.EntityFrameworkCore;

namespace CyclingStatsCollector.Entities;

[PrimaryKey("RiderID", "RaceID")]
public class Result
{
    public string RiderID { get; set; }
    public Rider Rider { get; set; }
    public string RaceID { get; set; }
    public Race Race { get; set; }
    public int Position { get; set; }
    public int Gap { get; set; }
}