using CyclingStats.WebAPI.ServiceContracts.Structs;

namespace CyclingStats.WebAPI.ServiceContracts.Responses;

public class RaceListResponse(List<Race> raceDetails)
{
    public long Count { get; set; } = raceDetails.Count;
    public Race[] Races { get; set; } = raceDetails.ToArray();
}