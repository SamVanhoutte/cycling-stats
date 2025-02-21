using Aerozure.Api;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models.Extensions;
using CyclingStats.WebAPI.ServiceContracts.Responses;
using CyclingStats.WebAPI.ServiceContracts.Structs;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using RaceDurationPrediction = CyclingStats.Models.RaceDurationPrediction;

namespace CyclingStats.WebAPI.Controllers;

[ApiController]
[Route("race/predict/{raceId}")]
public class RacePredictionController(
    IRaceService raceService,
    IDataRetriever dataScraper,
    IRaceDurationPredictor raceDurationPredictor) : AerobetsController
{
    /// <summary>
    /// Predict the duration of a race
    /// </summary>
    [HttpGet("duration")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, typeof(ProblemDetails),
        Description = "Not authorized to predict race.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(RaceDurationPredictionResponse),
        Description = "Race list.")]
    public async Task<IActionResult> GetDuration(string raceId)
    {
        raceId = raceId.FromApiNotation();
        var race = await raceService.GetRaceAsync(raceId);
        if (race == null)
        {
            return RaceNotFound(raceId);
        }

        IDictionary<string, RaceDurationPrediction> predictions;
        if (race.StageRace)
        {
            // Getting the races now
            var stages = await raceService.GetStageRaceStagesAsync(raceId);
            predictions = await raceDurationPredictor.PredictRaceDurationsAsync(stages);
        }
        else
        {
            predictions = await raceDurationPredictor.PredictRaceDurationsAsync([race]);
        }

        return Ok(new RaceDurationPredictionResponse(predictions.Select(p => new RaceDurationResult
        {
            RaceId = p.Key,
            DurationSeconds = (int)p.Value.Duration,
            Distance = p.Value.Distance,
            AverageSpeedKh = (decimal)(p.Value.Distance / p.Value.Duration) * 3600
        }).ToArray()));
    }
}