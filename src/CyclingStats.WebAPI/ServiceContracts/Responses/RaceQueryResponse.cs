namespace CyclingStats.WebAPI.ServiceContracts.Responses;

public class RaceQueryResponse(Dictionary<string, string> results)
{
    public long Count { get; set; } = results.Count;
    public Dictionary<string, string> Results { get; set; } = results;
}