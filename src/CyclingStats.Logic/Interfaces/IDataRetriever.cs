using CyclingStats.DataAccess.Entities;
using CyclingStats.Models;
using Rider = CyclingStats.Models.Rider;

namespace CyclingStats.Logic.Interfaces;

public interface IDataRetriever
{
    Task<List<RacerRaceResult>> GetRaceResultsAsync(string raceId, int year, string? stageId = null, int top = 50);
    Task<List<RacerRacePoint>> GetRacePointsAsync(string raceId, int year, string? stageId = null, int top = 50);
    Task<Rider> GetRiderAsync(string riderId);
    Task<IEnumerable<Rider>> GetTeamRidersAsync(string teamName);
    Task<IEnumerable<CyclingTeam>> GetTeamsAsync();
    Task<List<RaceDetails>> GetRaceDataAsync(Race race, string? stageId = null);
    Task<IDictionary<int, RaceDetails>> GetPastRaceResultsAsync(Race race, int years, string? stageId = null);
    Task<List<RaceDetails>> GetRaceCalendarAsync(int year);
    Task<string?> GetPcsIdAsync(Race race);
}