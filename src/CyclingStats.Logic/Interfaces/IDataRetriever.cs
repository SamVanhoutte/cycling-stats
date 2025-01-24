using CyclingStats.DataAccess.Entities;
using CyclingStats.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Rider = CyclingStats.Models.Rider;

namespace CyclingStats.Logic.Interfaces;

public interface IDataRetriever
{
    Task<List<RacerRaceResult>> GetRaceResultsAsync(Race race, int top = 50);
    Task<List<RacerRacePoint>> GetRacePointsAsync(Race race, int top = 50);
    Task<Rider?> GetRiderProfileAsync(Models.Rider rider);
    Task<IEnumerable<Rider>> GetTeamRidersAsync(string teamName);
    Task<IEnumerable<CyclingTeam>> GetTeamsAsync();
    Task<List<RaceDetails>> GetRaceDataAsync(Race race);
    Task<IDictionary<int, RaceDetails>> GetPastRaceResultsAsync(Race race, int years, string? stageId = null);
    Task<List<RaceDetails>> GetRaceCalendarAsync(int year);
    Task<string?> GetPcsRaceIdAsync(Race race);
    Task<string?> GetPcsRiderIdAsync(Rider rider);
}