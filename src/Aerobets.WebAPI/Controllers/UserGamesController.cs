using Aerobets.WebAPI.ServiceContracts.Requests;
using Aerobets.WebAPI.ServiceContracts.Responses;
using Aerobets.WebAPI.ServiceContracts.Structs;
using Aerozure.Encryption;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;
using CyclingStats.Models.Extensions;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using UserGame = Aerobets.WebAPI.ServiceContracts.Structs.UserGame;

namespace Aerobets.WebAPI.Controllers;

[ApiController]
[Route("users/{UserId}/games")]
public class UserGamesController(IUserService userService, IGameService gameService) : AerobetsController
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
        Description = "Not authorized to remove favorite riders.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(void),
        Description = "Rider removed.")]
    public async Task<IActionResult> RemoveUserGame(string userId, string raceId)
    {
        raceId = raceId.FromApiNotation()!;
        await gameService.RemoveUserGameAsync(userId, raceId);
        return Ok();
    }
}