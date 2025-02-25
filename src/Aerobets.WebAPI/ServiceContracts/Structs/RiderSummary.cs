using CyclingStats.Models;

namespace Aerobets.WebAPI.ServiceContracts.Structs;

public class RiderSummary
{
    public string RiderId { get; set; }
    public string Name { get; set; }
    public string Team { get; set; }
    public string Type { get; set; }
    public int CurrentRanking { get; set; }

    public static RiderSummary FromDomain(RiderBookmark riderBookmark)
    {
        var rider = riderBookmark.Rider;
        return new RiderSummary
        {
            Name = rider.Name!, RiderId = rider.Id, Team = rider.Team!, 
            CurrentRanking = rider.UciRanking, Type = rider.RiderType.ToString()
        };
    }
}