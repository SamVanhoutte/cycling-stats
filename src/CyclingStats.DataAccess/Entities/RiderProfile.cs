using CyclingStats.Models;
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

    public RiderType RiderType
    {
        get
        {
            var type = RiderType.Domestique;
            var highestValue = 0;
            if (GC > highestValue)
            {
                type = RiderType.GC;
                highestValue = GC;
            }
            if (Sprinter > highestValue)
            {
                type = RiderType.Sprinter;
                highestValue = Sprinter;
            }
            if (Puncheur > highestValue)
            {
                type = RiderType.Puncheur;
                highestValue = Puncheur;
            }
            if (OneDay > highestValue)
            {
                type = RiderType.OneDaySpecialist;
                highestValue = OneDay;
            }
            if (Climber > highestValue)
            {
                type = RiderType.Climber;
                highestValue = Climber;
            }
            if (TimeTrialist > highestValue)
            {
                type = RiderType.TimeTrialist;
                highestValue = TimeTrialist;
            }
            return type;
        }
    }

    public virtual Rider Rider { get; set; }
}