using CyclingStats.DataAccess;
using CyclingStats.DataAccess.Entities;
using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CyclingStats.Logic.Services;

public class GameService(IOptions<SqlOptions> sqlOptions, IUserService userService) : IGameService
{
    private SqlOptions sqlSettings = sqlOptions.Value;

    public async Task<List<UserGameDetails>> GetUserGamesAsync(string userId)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        var games = await ctx.Users
            .Include(u => u.UserGames)
            .ThenInclude(ur => ur.Game)
            .Where(u => u.UserId == userId)
            .SelectMany(u => u.UserGames).ToListAsync();
        return games.Select(CreateGameDetails).ToList();
    }

    private UserGameDetails CreateGameDetails(UserGame game)
    {
        return new UserGameDetails
        {
            Comment = game.Comment, Created = game.Created,
            Status = (GameStatus?)game.Game?.Status ?? GameStatus.Unknown,
        };
    }

    public async Task UpsertUserGameAsync(string userId, string raceId, UserGameDetails userGameDetails)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        var gameEntity = await ctx.UserGames.FindAsync(userId, raceId);
        if (gameEntity == null)
        {
            ctx.UserGames.Add(new UserGame()
            {
                UserId = userId,
                GameId = raceId,
                Comment = userGameDetails.Comment ?? "",
                Created = DateTime.UtcNow
            });
        }
        else
        {
            gameEntity.Comment = userGameDetails.Comment ?? "";
            gameEntity.Created = DateTime.UtcNow;
        }

        await ctx.SaveChangesAsync();
    }

    public async Task RemoveUserGameAsync(string userId, string raceId)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        await ctx.UserGames.Where(ufr => ufr.UserId == userId && ufr.GameId == raceId).ExecuteDeleteAsync();
    }

    public async Task<List<UserGameTeam>> GetUserGameTeamsAsync(string userId, string gameId)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        var gameEntities = await ctx.UserGameTeams.Where(team => team.GameId == gameId && team.UserId == userId)
            .ToListAsync();

        return gameEntities.Select(CreateGameTeam).ToList();
    }



    public async Task UpsertUserGameTeamAsync(string userId, string gameId, Guid? teamId, UserGameTeam team)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);

        teamId ??= Guid.NewGuid();
        var teamEntity = await ctx.UserGameTeams.FindAsync(teamId);
        if (teamEntity == null)
        {
            ctx.UserGameTeams.Add(new UserTeam()
            {
                UserId = userId,
                TeamId = teamId.Value,
                GameId = gameId,
                Comment = team.Comment,
                Name = team.Name,
                TeamListJson = System.Text.Json.JsonSerializer.Serialize(team.Riders),
                Created = DateTime.UtcNow
            });
        }
        else
        {
            teamEntity.Comment = team.Comment ?? "";
            teamEntity.Name = team.Name;
            teamEntity.TeamListJson = System.Text.Json.JsonSerializer.Serialize(team.Riders);
            teamEntity.Created = DateTime.UtcNow;
        }
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteUserGameTeamAsync(string userId, Guid teamId)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        await ctx.UserGameTeams.Where(ufr => ufr.TeamId == teamId && ufr.UserId == userId).ExecuteDeleteAsync();
    }


    private RiderBookmark CreateBookmark(UserRider favorite)
    {
        return new RiderBookmark
        {
            Rider = RiderService.CreateRider(favorite.Rider), CreatedOn = favorite.Created, Comment = favorite.Comment
        };
    }
    
    private UserGameTeam CreateGameTeam(UserTeam entity)
    {
        return new UserGameTeam
        {
            TeamId = entity.TeamId.ToString(),
            Name = entity.Name,
            Comment = entity.Comment,
            LastSaved = entity.Created,
            Riders = System.Text.Json.JsonSerializer.Deserialize<List<TeamRider>>(entity.TeamListJson)
        };
    }
}