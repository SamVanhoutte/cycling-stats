using Aerozure.Api;
using Microsoft.AspNetCore.Mvc;

namespace CyclingStats.WebAPI.Controllers;

public class AerobetsController : AeroApiController
{
    protected IActionResult RaceNotFound(string raceId)
    {
        return EntityNotFound("Race", raceId);
    }
}