using CyclingStats.Models;

namespace CyclingStats.Logic.Interfaces;

public interface IUserService
{
    Task<List<UserDetails>> GetUserDataAsync();
    Task<UserDetails?> GetUserAsync(string userId);
    Task<UserDetails> UpsertUserAsync(UserDetails userDetails);
    Task<bool> SetWcsPasswordAsync(string userId, string wcsUserName, string wcsPassword);
}