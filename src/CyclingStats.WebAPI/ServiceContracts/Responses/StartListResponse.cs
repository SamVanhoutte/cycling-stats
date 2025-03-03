using CyclingStats.WebApi.Models;

namespace CyclingStats.WebAPI.ServiceContracts.Responses;

public class StartListResponse(List<RiderRaceInfo> riders, int budget)
{
    public long RiderCount { get; set; } = riders.Count;
    public int StarBudget { get; set; } = budget;
    public RiderRaceInfo[] Riders { get; set; } = riders.ToArray();
}