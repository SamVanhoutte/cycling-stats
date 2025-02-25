using CyclingStats.DataAccess.Entities;
using CyclingStats.Models;
using Rider = CyclingStats.Models.Rider;

namespace CyclingStats.Logic.Interfaces;

public interface IDataRetriever
{
    Task<(int, List<RacerRaceResult>)> GetRaceResultsAsync(RaceDetails race, int top = 50);
    Task<List<RacerRacePoint>> GetRacePointsAsync(RaceDetails race, int top = 50);
    Task<Rider?> GetRiderProfileAsync(Models.Rider rider);
    Task<IEnumerable<Rider>> GetTeamRidersAsync(string teamName);
    Task<IEnumerable<CyclingTeam>> GetTeamsAsync();
    Task<List<RaceDetails>> GetRaceDataAsync(RaceDetails race);
    Task<IDictionary<int, RaceDetails>> GetPastRaceResultsAsync(Race race, int years, string? stageId = null);
    Task<List<RaceDetails>> GetRaceCalendarAsync(int year);
    Task<string?> GetPcsRaceIdAsync(RaceDetails race);
    Task<Dictionary<string, string>> QueryPcsRaceIdsAsync(string query, int year);
    Task<string?> GetPcsRiderIdAsync(Rider rider);
    Task<StartGrid?> GetStartListAsync(string raceId);
}