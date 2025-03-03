using CyclingStats.WebApi.Models;

namespace CyclingStats.WebAPI.ServiceContracts.Responses;

public class RaceDurationPredictionResponse(RaceDurationResult[] racePredictions)
{
    public long Count { get; set; } = racePredictions.Length;
    public long TotalDuration { get; set; } = racePredictions.Sum(rp=>rp.DurationSeconds);
    // public decimal AverageSpeed { get; set; } = racePredictions.Sum(rp=>rp.Distance) / racePredictions.Sum(rp=>rp.DurationSeconds);
    public RaceDurationResult[] Predictions { get; set; } = racePredictions.ToArray();
}