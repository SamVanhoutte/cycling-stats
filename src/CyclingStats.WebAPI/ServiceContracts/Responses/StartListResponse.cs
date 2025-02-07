using CyclingStats.WebAPI.ServiceContracts.Structs;

namespace CyclingStats.WebAPI.ServiceContracts.Responses;

public class StartListResponse(List<RiderRaceInfo> riders)
{
    public long RiderCount { get; set; } = riders.Count;
    public int StarBudget { get; set; } 
    public RiderRaceInfo[] Riders { get; set; } = riders.ToArray();
}