using Aerobets.WebAPI.ServiceContracts.Structs;
using CyclingStats.WebApi.Models;

namespace Aerobets.WebAPI.ServiceContracts.Responses;

public class UserGameStartlistResponse(List<UserRiderRaceInfo> riders, int budget)
{
    public long RiderCount { get; set; } = riders.Count;
    public int StarBudget { get; set; } = budget;
    public UserRiderRaceInfo[] Riders { get; set; } = riders.ToArray();    
}