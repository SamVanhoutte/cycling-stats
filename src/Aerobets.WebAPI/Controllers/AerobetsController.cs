using Aerozure.Api;
using Microsoft.AspNetCore.Mvc;

namespace Aerobets.WebAPI.Controllers;

public class AerobetsController : AeroApiController
{
    protected IActionResult RaceNotFound(string raceId)
    {
        return EntityNotFound("Race", raceId);
    }
    
    protected IActionResult UserNotFound(string userId)
    {
        return EntityNotFound("User", userId);
    }
}