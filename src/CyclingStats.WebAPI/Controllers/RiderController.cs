using Aerozure.Api;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models.Extensions;
using CyclingStats.WebAPI.ServiceContracts.Requests;
using CyclingStats.WebAPI.ServiceContracts.Responses;
using CyclingStats.WebAPI.ServiceContracts.Structs;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace CyclingStats.WebAPI.Controllers;

[ApiController]
[Route("riders")]
public class RiderController(IRiderService riderService, IDataRetriever dataScraper)
    : AeroApiController
{
    /// <summary>
    /// Get the riders
    /// </summary>
    [HttpGet()]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ProblemDetails),
        Description = "Not authorized to retrieve riders.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(RiderListResponse),
        Description = "Rider list.")]
    public async Task<IActionResult> GetRiders()
    {
        // Query by year
        var riders = await riderService.GetRidersAsync(excludeRetiredRiders:true);


        return Ok(new RiderListResponse(riders.Select(Rider.FromDomain).ToArray()));
    }
    
    /// <summary>
    /// Get a rider
    /// </summary>
    /// <param name="riderId">The id of the rider you want to read</param>
    [HttpGet("{riderId}")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ProblemDetails),
        Description = "Not authorized to retrieve rider.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(Rider),
        Description = "Rider details.")]
    public async Task<IActionResult> GetRider(string riderId)
    {
        // Query by year
        var rider = await riderService.GetRiderAsync(riderId);
        if (rider == null) return EntityNotFound("Rider", riderId);

        return Ok(rider);
    }
}