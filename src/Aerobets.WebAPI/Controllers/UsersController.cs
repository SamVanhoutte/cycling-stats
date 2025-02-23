using Aerobets.WebAPI.ServiceContracts.Requests;
using Aerobets.WebAPI.ServiceContracts.Responses;
using Aerobets.WebAPI.ServiceContracts.Structs;
using Aerozure.Encryption;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
namespace Aerobets.WebAPI.Controllers;

[ApiController]
[Route("users")]
public class UsersController(IUserService userService, IEncryptionService encryptor) : AerobetsController
{
    /// <summary>
    /// Get the users
    /// </summary>
    [HttpGet()]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ProblemDetails),
        Description = "Not authorized to retrieve users.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(UserListResponse),
        Description = "User list.")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await userService.GetUserDataAsync();
        return Ok(new UserListResponse(users.Select(Aerobets.WebAPI.ServiceContracts.Structs.User.FromDomain).ToArray()));
    }

    /// <summary>
    /// Get the user details
    /// </summary>
    [HttpGet("{userId}")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ProblemDetails),
        Description = "Not authorized to retrieve user.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(User),
        Description = "User details.")]
    public async Task<IActionResult> GetUser(string userId)
    {
        var user = await userService.GetUserAsync(userId);
        return user==null 
            ? UserNotFound(userId) 
            : Ok(Aerobets.WebAPI.ServiceContracts.Structs.User.FromDomain(user));
    }
    
    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost()]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ProblemDetails),
        Description = "Not authorized to create a user.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(User),
        Description = "User created.")]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        if (string.IsNullOrEmpty(request.UserId))
        {
            return BadRequest("UserId is required");
        }
        var user = new UserDetails
        {
            UserId = request.UserId,
            Email = request.Email,
            Phone = request.PhoneNumber,
            WcsUserName = request.WcsUserName,
            Language = request.Language,
            AuthenticationId = request.AuthenticationId
        };
        var createdUser = await userService.UpsertUserAsync(user);
        return Ok(Aerobets.WebAPI.ServiceContracts.Structs.User.FromDomain(createdUser));
    }
    
    /// <summary>
    /// Updates a user
    /// </summary>
    /// <param name="userId">The id of the user</param>
    [HttpPut("{userId}")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ProblemDetails),
        Description = "Not authorized to update a user.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(User),
        Description = "User updated.")]
    public async Task<IActionResult> UpdateUser(string userId, UpdateUserRequest request)
    {
        var user = new UserDetails
        {
            UserId = userId,
            Email = request.Email,
            Phone = request.PhoneNumber,
            WcsUserName = request.WcsUserName,
            Language = request.Language,
            AuthenticationId = request.AuthenticationId
        };
        var createdUser = await userService.UpsertUserAsync(user);
        return Ok(Aerobets.WebAPI.ServiceContracts.Structs.User.FromDomain(createdUser));
    }
    
    /// <summary>
    /// Set the WCS password for a user
    /// </summary>
    /// <param name="userId">The id of the user</param>
    [HttpPut("{userId}/password/wcs")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ProblemDetails),
        Description = "Not authorized to set the password of a user.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(void),
        Description = "Password updated.")]
    public async Task<IActionResult> SetWcsPassword(string userId, UpdatePasswordRequest request)
    {
        // First validate if we can decrypt the password
        if (string.IsNullOrEmpty(request.EncryptedPassword))
        {
            return BadRequest("EncryptedPassword is required");
        }

        var decrypted = encryptor.Decrypt(request.EncryptedPassword);
        if (string.IsNullOrEmpty(decrypted))
        {
            return BadRequest("Password is not encrypted correctly");
        }

        var userUpdated = await userService.SetWcsPasswordAsync(userId, request.Username, request.EncryptedPassword);
        return userUpdated == false 
            ? UserNotFound(userId) 
            : Ok();
    }
}