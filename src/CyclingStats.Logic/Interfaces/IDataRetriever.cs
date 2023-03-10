using CyclingStats.Models;

namespace CyclingStats.Logic.Interfaces;

public interface IDataRetriever
{
    Task<List<RacerRaceResult>> GetRaceResultsAsync(string raceId, int top = 50);
    Task<Rider> GetRiderAsync(string riderId);
    Task<IEnumerable<Rider>> GetTeamRidersAsync(string teamName);
    Task<RaceResults> GetRaceDataAsync(string raceId);
}