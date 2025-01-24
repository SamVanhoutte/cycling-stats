using Microsoft.EntityFrameworkCore;

namespace CyclingStats.DataAccess.Entities;

[PrimaryKey("RiderId", "Month")]
public class RiderProfile
{
    public string RiderId { get; set; }
    public string Month { get; set; }
    public int GC { get; set; }
    public int Sprinter { get; set; }
    public int Puncheur { get; set; }
    public int OneDay { get; set; }
    public int Climber { get; set; }
    public int TimeTrialist { get; set; }
    public int UciRanking { get; set; }
    public int PcsRanking { get; set; }

    public virtual Rider Rider { get; set; }
}