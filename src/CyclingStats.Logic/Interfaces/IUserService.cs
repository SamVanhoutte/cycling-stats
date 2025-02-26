using CyclingStats.Models;

namespace CyclingStats.Logic.Interfaces;

public interface IUserService
{
    Task<List<UserDetails>> GetUserDataAsync();
    Task<UserDetails?> GetUserAsync(string userId);
    Task<UserDetails> UpsertUserAsync(UserDetails userDetails);
    Task<bool> SetWcsPasswordAsync(string userId, string wcsUserName, string wcsPassword);
    Task<List<RiderBookmark>> GetFavoriteRidersAsync(string userId);
    Task UpsertFavoriteRiderAsync(string userId, string riderId, string comment);
    Task RemoveFavoriteRiderAsync(string userId, string riderId);
    Task<List<PlayerWatcher>> GetPlayerWatchlistAsync(string userId);
    Task UpsertPlayerToWatchAsync(string userId, PlayerWatcher player);
    Task RemovePlayerFromWatchlistAsync(string userId, string wcsUserName);
}