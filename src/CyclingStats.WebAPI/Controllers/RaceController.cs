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
[Route("races")]
public class RaceController(IRaceService raceService, IDataRetriever dataScraper) : AeroApiController
{
    /// <summary>
    /// Get the races
    /// </summary>
    [HttpGet()]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ProblemDetails),
        Description = "Not authorized to retrieve races.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(RaceListResponse),
        Description = "Race list.")]
    public async Task<IActionResult> GetRaces([FromQuery] int? year = null)
    {
        // Query by year
        var races = await raceService.GetRaceDataAsync();
        races = races.Where(
            r => r.Date?.Year == (year ?? r.Date?.Year)
        ).ToList();

        var raceList = new List<Race> { };
        // Take one day races first
        raceList.AddRange(races.Where(r => r.IsStageRace == false).Select(Race.FromDomain));

        // Then add the stage races with stages as children
        foreach (var race in races.Where(r => r.IsStageRace && string.IsNullOrEmpty(r.StageId)))
        {
            var stages = races.Where(r => r.StageRaceId?.Equals(race.Id) ?? false);
            raceList.Add(Race.FromDomain(race, stages.ToArray()));
        }

        raceList = raceList.OrderBy(r => r.Date).ToList();

        return Ok(new RaceListResponse(raceList));
    }

    /// <summary>
    /// Get the race details for a specific race
    /// </summary>
    /// <param name="raceId">The unique id of the race.</param>
    [HttpGet("{raceId}")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ProblemDetails),
        Description = "Not authorized to retrieve race.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(Race),
        Description = "Race.")]
    public async Task<IActionResult> GetRace(string raceId)
    {
        raceId = raceId.FromApiNotation()!;
        var raceDetails = await raceService.GetRaceAsync(raceId);
        if (raceDetails == null)
        {
            return NotFound($"Race {raceId} not found");
        }

        if (raceDetails.IsStageRace)
        {
            var stages = await raceService.GetStageRaceStagesAsync(raceId);
            var response = Race.FromDomain(raceDetails, stages.ToArray());
            return Ok(response);
        }
        else
        {
            var response = Race.FromDomain(raceDetails);
            return Ok(response);
        }
    }

    /// <summary>
    /// Get the start list for a specific race
    /// </summary>
    /// <param name="raceId">The unique id of the race.</param>
    [HttpGet("{raceId}/startlist")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ProblemDetails),
        Description = "Not authorized to retrieve race.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(StartListResponse),
        Description = "Race.")]
    public async Task<IActionResult> GetRaceStartList(string raceId)
    {
        raceId = raceId.FromApiNotation()!;
        var startGrid = await dataScraper.GetStartListAsync(raceId);
        var raceDetails = await raceService.GetRaceAsync(raceId);
        if (raceDetails == null)
        {
            return NotFound($"Race {raceId} not found");
        }

        var response = Race.FromDomain(raceDetails);
        return Ok(response);
    }

    /// <summary>
    /// Query for the available PCS id's, based on a race name
    /// </summary>
    /// <param name="year">The year for which you want to query the races.</param>
    [HttpGet("pcs/{query}/{year}")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ProblemDetails),
        Description = "Not authorized to retrieve races.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(RaceQueryResponse),
        Description = "Available races.")]
    public async Task<IActionResult> QueryPcsRaceId(string query, int year)
    {
        var queryResults = await dataScraper.QueryPcsRaceIdsAsync(query, year);

        return Ok(new RaceQueryResponse(queryResults));
    }


    /// <summary>
    /// Update race information
    /// </summary>
    [HttpPost()]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ProblemDetails),
        Description = "Not authorized to update race.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(void),
        Description = "Race updated.")]
    public async Task<IActionResult> UpdateRace(RaceUpdateRequest request)
    {
        request.RaceId = request.RaceId.FromApiNotation()!;
        request.PcsId = request.PcsId.FromApiNotation();
        if (!string.IsNullOrEmpty(request.PcsId))
        {
            var existingRace = await raceService.GetRaceAsync(request.RaceId);
            if (existingRace == null) return NotFound($"Race {request.RaceId} does not exist");
            existingRace.PcsId = request.PcsId;
            existingRace.Status = request.Status ?? existingRace.Status;
            await raceService.UpsertRaceDetailsAsync(existingRace, false);
        }
        else
        {
            if (request.Status != null)
            {
                await raceService.UpdateRaceStatusAsync(request.RaceId, request.Status.Value);
            }
        }

        return Ok();
    }
}