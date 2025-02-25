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
        await using var ctx = StatsDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        return (await ctx.GetAllUsersAsync()).Select(CreateUserDetails).ToList();
    }

    public async Task<UserDetails?> GetUserAsync(string userId)
    {
        await using var ctx = StatsDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        var userEntity = await ctx.Users.FindAsync(userId);
        return userEntity == null
            ? null
            : CreateUserDetails(userEntity);
    }

    public async Task<UserDetails> UpsertUserAsync(UserDetails userDetails)
    {
        await using var ctx = StatsDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        var userEntity = await ctx.Users.FindAsync(userDetails.UserId);
        if (userEntity == null)
        {
            ctx.Users.Add(new User()
            {
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
        }

        await ctx.SaveChangesAsync();

        return userDetails;
    }

    public async Task<bool> SetWcsPasswordAsync(string userId, string wcsUserName, string wcsPassword)
    {
        await using var ctx = StatsDbContext.CreateFromConnectionString(
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
        await using var ctx = StatsDbContext.CreateFromConnectionString(
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
            Rider = RiderService.CreateRider( favorite.Rider), CreatedOn = favorite.Created, Comment = favorite.Comment
        };
    }

    public async Task UpsertFavoriteRiderAsync(string userId, string riderId, string comment)
    {
        await using var ctx = StatsDbContext.CreateFromConnectionString(
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
        await using var ctx = StatsDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        await ctx.UserFavoriteRiders.Where(ufr => ufr.UserId==userId && ufr.RiderId==riderId).ExecuteDeleteAsync();
    }


    private UserDetails CreateUserDetails(User entity)
    {
        return new UserDetails()
        {
            UserId = entity.UserId,
            Email = entity.Email,
            AuthenticationId = entity.AuthenticationId,
            WcsUserName = entity.WcsUserName,
            Phone = entity.Phone,
            Updated = entity.Updated
        };
    }
}