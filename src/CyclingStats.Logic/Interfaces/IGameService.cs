using CyclingStats.Models;

namespace CyclingStats.Logic.Interfaces;

public interface IGameService
{
    Task<List<UserGameDetails>> GetUserGamesAsync(string userId);
    Task UpsertUserGameAsync(string userId, string raceId, UserGameDetails userGameDetails);
    Task RemoveUserGameAsync(string userId, string raceId);
}