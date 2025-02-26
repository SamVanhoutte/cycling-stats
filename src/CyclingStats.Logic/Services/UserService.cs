using CyclingStats.DataAccess;
using CyclingStats.DataAccess.Entities;
using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rider = CyclingStats.DataAccess.Entities.Rider;

namespace CyclingStats.Logic.Services;

public class UserService(IOptions<SqlOptions> sqlOptions) : IUserService
{
    private SqlOptions sqlSettings = sqlOptions.Value;

    public async Task<List<UserDetails>> GetUserDataAsync()
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        return (await ctx.GetAllUsersAsync()).Select(CreateUserDetails).ToList();
    }

    public async Task<UserDetails?> GetUserAsync(string userId)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        var userEntity = await ctx.Users.FindAsync(userId);
        return userEntity == null
            ? null
            : CreateUserDetails(userEntity);
    }

    public async Task<UserDetails> UpsertUserAsync(UserDetails userDetails)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        var userEntity = await ctx.Users.FindAsync(userDetails.UserId);
        if (userEntity == null)
        {
            ctx.Users.Add(new User()
            {
                Name = userDetails.Name,
                UserId = userDetails.UserId,
                Email = userDetails.Email,
                AuthenticationId = userDetails.AuthenticationId,
                WcsUserName = userDetails.WcsUserName,
                Phone = userDetails.Phone,
                Updated = DateTime.Now
            });
        }
        else
        {
            userEntity.Email = userDetails.Email;
            userEntity.WcsUserName = userDetails.WcsUserName;
            userEntity.Phone = userDetails.Phone;
            userEntity.Updated = DateTime.UtcNow;
            userEntity.Name = userDetails.Name;
        }

        await ctx.SaveChangesAsync();

        return userDetails;
    }

    public async Task<bool> SetWcsPasswordAsync(string userId, string wcsUserName, string wcsPassword)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        var userEntity = await ctx.Users.FindAsync(userId);
        if (userEntity == null)
        {
            return false;
        }

        userEntity.WcsUserName = wcsUserName;
        userEntity.WcsPasswordEncrypted = wcsPassword;
        await ctx.SaveChangesAsync();
        return true;
    }

    public async Task<List<RiderBookmark>> GetFavoriteRidersAsync(string userId)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        var riders = await ctx.Users
            .Include(u => u.UserRiders)
            .ThenInclude(ur => ur.Rider)
            .ThenInclude(r => r.Profiles)
            //.Include(u => u.FavoriteRiders)
            .Where(u => u.UserId == userId)
            .SelectMany(u => u.UserRiders).ToListAsync();
        return riders.Select(CreateBookmark).ToList();
    }

    private RiderBookmark CreateBookmark(UserRider favorite)
    {
        return new RiderBookmark
        {
            Rider = RiderService.CreateRider(favorite.Rider), CreatedOn = favorite.Created, Comment = favorite.Comment
        };
    }

    public async Task UpsertFavoriteRiderAsync(string userId, string riderId, string comment)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        var favEntity = await ctx.UserFavoriteRiders.FindAsync(userId, riderId);
        if (favEntity == null)
        {
            ctx.UserFavoriteRiders.Add(new UserRider()
            {
                UserId = userId,
                RiderId = riderId,
                Comment = comment,
                Created = DateTime.UtcNow
            });
        }
        else
        {
            favEntity.Comment = comment;
            favEntity.Created = DateTime.UtcNow;
        }

        await ctx.SaveChangesAsync();
    }

    public async Task RemoveFavoriteRiderAsync(string userId, string riderId)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        await ctx.UserFavoriteRiders.Where(ufr => ufr.UserId == userId && ufr.RiderId == riderId).ExecuteDeleteAsync();
    }

    public async Task<List<PlayerWatcher>> GetPlayerWatchlistAsync(string userId)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        var players = await ctx.WatchedPlayers
            .Include(u => u.Player)
            .Where(u => u.UserId == userId)
            .ToListAsync();
        return players.Select(CreatePlayer).ToList();
    }

    private PlayerWatcher CreatePlayer(PlayerToWatch watchPlayer)
    {
        var player = new PlayerWatcher()
        {
            WcsUserName = watchPlayer.WcsUserName,
            Comment = watchPlayer.Comment,
            Created = watchPlayer.Created,
            Name = watchPlayer.Name,
            MainOpponent = watchPlayer.MainOpponent,
            PlayerUserId = watchPlayer.PlayerUserId,
        };
        if (watchPlayer.Player != null)
        {
            player.Player = CreateUserDetails(watchPlayer.Player);
        }

        return player;
    }

    public async Task UpsertPlayerToWatchAsync(string userId, PlayerWatcher player)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        if (string.IsNullOrEmpty(player.PlayerUserId))
        {
            // See if the wcs user already exists
            var playerEntity = await ctx.Users.FirstOrDefaultAsync(u => u.WcsUserName == player.WcsUserName);
            player.PlayerUserId = playerEntity?.UserId;
        }
        var watchlistEntity = await ctx.WatchedPlayers.FindAsync(userId, player.WcsUserName);
        if (watchlistEntity == null)
        {
            ctx.WatchedPlayers.Add(new PlayerToWatch()
            {
                UserId = userId,
                Name = player.Name,
                WcsUserName = player.WcsUserName,
                MainOpponent = player.MainOpponent,
                Comment = player.Comment,
                PlayerUserId = player.PlayerUserId,
                Created = DateTime.UtcNow
            });
        }
        else
        {
            watchlistEntity.WcsUserName = player.WcsUserName;
            watchlistEntity.MainOpponent = player.MainOpponent;
            watchlistEntity.Comment = player.Comment;
            watchlistEntity.Name = player.Name;
            watchlistEntity.PlayerUserId = player.PlayerUserId;
            watchlistEntity.Created = DateTime.UtcNow;
        }

        await ctx.SaveChangesAsync();
    }

    public async Task RemovePlayerFromWatchlistAsync(string userId, string wcsUserName)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        await ctx.WatchedPlayers.Where(ptw => ptw.UserId == userId && ptw.WcsUserName == wcsUserName).ExecuteDeleteAsync();
    }


    private UserDetails CreateUserDetails(User entity)
    {
        return new UserDetails()
        {
            UserId = entity.UserId,
            Email = entity.Email,
            AuthenticationId = entity.AuthenticationId,
            WcsUserName = entity.WcsUserName,
            Name = entity.Name,
            Phone = entity.Phone,
            Updated = entity.Updated
        };
    }
}