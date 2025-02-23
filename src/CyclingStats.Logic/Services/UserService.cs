using CyclingStats.DataAccess;
using CyclingStats.DataAccess.Entities;
using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CyclingStats.Logic.Services;



public class UserService(IOptions<SqlOptions> sqlOptions) : IUserService
{
    private SqlOptions sqlSettings = sqlOptions.Value;

    public async Task<List<UserDetails>> GetUserDataAsync()
    {
        using (var ctx = StatsDbContext.CreateFromConnectionString(
                   sqlSettings.ConnectionString))
        {
            return (await ctx.GetAllUsersAsync()).Select(CreateUserDetails).ToList();
        }
    }
    
    public async Task<UserDetails?> GetUserAsync(string userId) {
        using (var ctx = StatsDbContext.CreateFromConnectionString(
                   sqlSettings.ConnectionString))
        {
            var userEntity = await ctx.Users.FindAsync(userId);
            return userEntity == null 
                ? null : 
                CreateUserDetails(userEntity);
        }
    }
    
    public async Task<UserDetails> UpsertUserAsync(UserDetails userDetails)
    {
        using (var ctx = StatsDbContext.CreateFromConnectionString(
                   sqlSettings.ConnectionString))
        {
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
                userEntity.Updated = DateTime.Now;
            }

            await ctx.SaveChangesAsync();
        }

        return userDetails;
    }

    public async Task<bool> SetWcsPasswordAsync(string userId, string wcsUserName, string wcsPassword)
    {
        using (var ctx = StatsDbContext.CreateFromConnectionString(
                   sqlSettings.ConnectionString))
        {
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