using Aerobets.WebAPI.ServiceContracts.Requests;
using Aerobets.WebAPI.ServiceContracts.Responses;
using Aerobets.WebAPI.ServiceContracts.Structs;
using Aerozure.Encryption;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;
using CyclingStats.Models.Extensions;
using CyclingStats.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using TeamRider = CyclingStats.Models.TeamRider;
using UserGame = Aerobets.WebAPI.ServiceContracts.Structs.UserGame;
using UserGameTeam = Aerobets.WebAPI.ServiceContracts.Structs.UserGameTeam;

namespace Aerobets.WebAPI.Controllers;

[ApiController]
[Route("users/{userId}/games")]
public class UserGamesController(IUserService userService, IGameService gameService, IRaceService raceService) : AerobetsController
{
    /// <summary>
    /// Get the games of a user
    /// </summary>
    /// <param name="userId">The id of the user</param>
    [HttpGet()]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ProblemDetails),
        Description = "Not authorized to retrieve games for the user.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(UserGamesResponse),
        Description = "Game list.")]
    public async Task<IActionResult> GetUserGames(string userId)
    {
        var user = await userService.GetUserAsync(userId);
        if (user == null) return UserNotFound(userId);
        var games = await gameService.GetUserGamesAsync(userId);
        return Ok(new UserGamesResponse(games.Select(UserGame.FromDomain).ToArray()));
    }
    
    /// <summary>
    /// Add a game to a user
    /// </summary>
    /// <param name="userId">The id of the user</param>
    /// <param name="raceId">The id of the game</param>
    /// <param name="request">The details for the game</param>
    [HttpPost("{raceId}")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ProblemDetails),
        Description = "Not authorized to add a game.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(void),
        Description = "Game favorited.")]
    public async Task<IActionResult> AddGame(string userId, string raceId, CreateUserGameRequest request)
    {
        raceId = raceId.FromApiNotation()!;
        await gameService.UpsertUserGameAsync(userId, raceId, new UserGameDetails
        {
            Comment = request.Comment
        });
        return Ok();
    }
    
    /// <summary>
    /// Delete a game for a user
    /// </summary>
    /// <param name="userId">The id of the user</param>
    /// <param name="raceId">The id of the game</param>
    [HttpDelete("{raceId}")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ProblemDetails),
        Description = "Not authorized to remove game.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(void),
        Description = "Rider removed.")]
    public async Task<IActionResult> RemoveUserGame(string userId, string raceId)
    {
        raceId = raceId.FromApiNotation()!;
        await gameService.RemoveUserGameAsync(userId, raceId);
        return Ok();
    }
    
    /// <summary>
    /// Get the teams of a user for a game
    /// </summary>
    /// <param name="userId">The id of the user</param>
    /// <param name="gameId">The id of the game</param>
    [HttpGet("{gameId}/teams")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ProblemDetails),
        Description = "Not authorized to retrieve teams for the user.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(UserGameTeamsResponse),
        Description = "Game teams list.")]
    public async Task<IActionResult> GetUserGameTeams(string userId, string gameId)
    {
        gameId = gameId.FromApiNotation();
        var user = await userService.GetUserAsync(userId);
        if (user == null) return UserNotFound(userId);
        var games = await gameService.GetUserGameTeamsAsync(userId, gameId);
        return Ok(new UserGameTeamsResponse(games.Select(UserGameTeam.FromDomain).ToArray()));
    }
    
    /// <summary>
    /// Save a team for a game to a user
    /// </summary>
    /// <param name="userId">The id of the user</param>
    /// <param name="raceId">The id of the game</param>
    /// <param name="request">The team for the game</param>
    [HttpPost("{raceId}/teams")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ProblemDetails),
        Description = "Not authorized to add a game team.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(void),
        Description = "Game team added.")]
    public async Task<IActionResult> AddGameTeam(string userId, string raceId, CreateUserGameTeamRequest request)
    {
        raceId = raceId.FromApiNotation()!;
        await gameService.UpsertUserGameTeamAsync(userId, raceId, null, new CyclingStats.Models.UserGameTeam 
        {
            Comment = request.Comment,
            Name = request.Name,
            LastSaved = DateTime.UtcNow,
            Riders = request.Riders.Select(r => new TeamRider
            {
                RiderId = r.RiderId, Stars = r.Stars, RiderType = r.RiderType, Youth = r.Youth
            }).ToList()
        });
        return Ok();
    }
    
    /// <summary>
    /// Update a team for a game to a user
    /// </summary>
    /// <param name="userId">The id of the user</param>
    /// <param name="raceId">The id of the game</param>
    /// <param name="teamId">The id of the team</param>
    /// <param name="request">The team for the game</param>
    [HttpPut("{raceId}/teams/{teamId}")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ProblemDetails),
        Description = "Not authorized to add a game team.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(void),
        Description = "Game team added.")]
    public async Task<IActionResult> UpdateGameTeam(string userId, string raceId, string teamId, UpdateUserGameTeamRequest request)
    {
        raceId = raceId.FromApiNotation()!;
        if (!Guid.TryParse(teamId, out var tId))
        {
            return BadRequest("The team id should be a valid Guid");
        }
        await gameService.UpsertUserGameTeamAsync(userId, raceId, tId, new CyclingStats.Models.UserGameTeam 
        {
            Comment = request.Comment,
            Name = request.Name,
            LastSaved = DateTime.UtcNow,
            Riders = request.Riders.Select(r => new TeamRider
            {
                RiderId = r.RiderId, Stars = r.Stars, RiderType = r.RiderType, Youth = r.Youth
            }).ToList()
        });
        return Ok();
    }
    
    /// <summary>
    /// Delete a team for a game for a user
    /// </summary>
    /// <param name="userId">The id of the user</param>
    /// <param name="raceId">The id of the game</param>
    /// <param name="teamId">The id of the team</param>
    [HttpDelete("{raceId}/teams/{teamId}")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ProblemDetails),
        Description = "Not authorized to remove game team.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(void),
        Description = "Rider removed.")]
    public async Task<IActionResult> RemoveUserGameTeam(string userId, string raceId, string teamId)
    {
        raceId = raceId.FromApiNotation()!;
        if (!Guid.TryParse(teamId, out var tId))
        {
            return BadRequest("The team id should be a valid Guid");
        }
        await gameService.DeleteUserGameTeamAsync(userId, tId);
        return Ok();
    }
    
    
    
    /// <summary>
    /// Return relevant information for the user to prepare a game
    /// </summary>
    /// <param name="userId">The id of the user</param>
    /// <param name="gameId">The id of the user</param>
    [HttpGet("{gameId}/startlist")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ProblemDetails),
        Description = "Not authorized to retrieve start list for the game.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(UserGameStartlistResponse),
        Description = "Start list.")]
    public async Task<IActionResult> GetGameStartList(string userId, string gameId)
    {
        gameId = gameId.FromApiNotation();
        var user = await userService.GetUserAsync(userId);
        if (user == null) return UserNotFound(userId);
        var favoriteRiders = (await userService.GetFavoriteRidersAsync(userId));
        var startGrid = await raceService.GetRaceStartGridAsync(gameId);
        if (startGrid == null)
        {
            return EntityNotFound("Game", gameId);
        }

        var race = await raceService.GetRaceAsync(gameId);
        var lastYearsResult = (await raceService.GetRaceAsync(gameId.ChangeYear(DateTime.UtcNow.Year - 1)!, true)).Results;
        
        var pastRaces = await raceService.GetRaceDataAsync();
        pastRaces = pastRaces.Where(r => 
            r.ResultsRetrieved 
            && r.Date.Value.Year==DateTime.UtcNow.Year
            && r.ParcoursType == race.ParcoursType 
            ).ToList();
        pastRaces = (await raceService.FilloutResultsAsync(pastRaces)).ToList();
        
        var games = await gameService.GetUserGamesAsync(userId);
        var userStartGrid = new List<UserRiderRaceInfo> { };
        foreach (var rider in startGrid.Riders)
        {
            var lastYearResult = lastYearsResult?.FirstOrDefault(result => result.Rider?.Id.Equals(rider.Rider.Id) ?? false)?.Position ?? -1;
            var pastRacesScore = pastRaces
                .Where(pr => pr.Results?.Any(result => result.Rider?.Id.Equals(rider.Rider.Id) ?? false)??false)
                .ToDictionary(pr => pr.Id, pr => pr.Results?.FirstOrDefault(result => result.Rider?.Id.Equals(rider.Rider.Id)??false)?.Position ?? -1);
            var recommendationScores = GetRecommendationScores(race, rider, lastYearResult, pastRacesScore);
            var bookmark = favoriteRiders.SingleOrDefault(r => r.Rider.Id.Equals(rider.Rider.Id));
            var userRider = UserRiderRaceInfo.FromDomain(rider, bookmark, lastYearResult, pastRacesScore, recommendationScores);
            userStartGrid.Add(userRider);
        }
        return Ok(new UserGameStartlistResponse(userStartGrid, startGrid.StarBudget));
    }


    private struct RecommendationWeights
    {
        public const int Speciality = 3;
        public const int RecentResults = 5;
        public const int LastYearResult = 2;
    }
    private RecommendationScores GetRecommendationScores(RaceDetails? race, StartingRider rider, int lastYearResult,
        Dictionary<string, int> pastRacesScore)
    {
        var scores = new RecommendationScores();
        var pointsMatrix = new List<double>();
        // Take the score of the rider profile , according to the race profile
        // Normalize the score to a 0-1 scale
        if (rider?.Rider != null)
        {
            var specialityPoints = race.ParcoursType switch
            {
                1 => rider?.Rider?.Sprinter ?? 0,
                2 => rider?.Rider?.OneDay ?? 0,
                3 => rider?.Rider?.Puncheur ?? 0,
                4 => rider?.Rider?.Climber ?? 0,
                5 => rider?.Rider?.Climber ?? 0,
                _ => (race.IsStageRace ? rider?.Rider.GC : rider?.Rider?.OneDay) ?? 0
            } / 1500d;
            scores.SpecialityPoints = specialityPoints;
            pointsMatrix.AddRange(Enumerable.Repeat(specialityPoints, RecommendationWeights.Speciality));
        }

        // Take the median score of the past races
        // The smaller the value, the higher the score
        if (pastRacesScore.Any())
        {
            var pastRacePoints = 1d - Math.Min( Median(pastRacesScore.Values.ToArray()), 150) / 150d;
            pointsMatrix.AddRange(Enumerable.Repeat(pastRacePoints, RecommendationWeights.RecentResults));
            scores.RecentRacePoints = pastRacePoints;
        }
        
        // Take the median score of the past races
        // The smaller the value, the higher the score
        if (lastYearResult > 0)
        {
            var lastYearPoints = 1d - Math.Min(lastYearResult, 150) / 150d;
            pointsMatrix.AddRange(Enumerable.Repeat(lastYearPoints, RecommendationWeights.LastYearResult));
            scores.LastYearPoints = lastYearPoints;
        }

        scores.Overall = pointsMatrix.Any() ? pointsMatrix.Average() : -1d;
        return scores;
    }
    
    private int Median(int[] xs) {
        var ys = xs.OrderBy(x => x).ToList();
        var mid = (ys.Count - 1) / 2;
        return (ys[mid] + ys[(int)(mid + 0.5)]) / 2;
    }
}