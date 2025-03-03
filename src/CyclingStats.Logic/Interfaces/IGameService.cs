using CyclingStats.Models;

namespace CyclingStats.Logic.Interfaces;

public interface IGameService
{
    Task<List<UserGameDetails>> GetUserGamesAsync(string userId);
    Task UpsertUserGameAsync(string userId, string raceId, UserGameDetails userGameDetails);
    Task RemoveUserGameAsync(string userId, string raceId);
    Task<List< UserGameTeam>> GetUserGameTeamsAsync(string userId, string gameId);
    Task UpsertUserGameTeamAsync(string userId, string gameId, Guid? teamId, UserGameTeam team);
    Task DeleteUserGameTeamAsync(string userId, Guid teamId);
}