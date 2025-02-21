using CyclingStats.Models;

namespace CyclingStats.Logic.Interfaces;

public interface IRaceDurationPredictor
{
    public Task<IDictionary<string, RaceDurationPrediction>> PredictRaceDurationsAsync(IEnumerable<RaceDetails> races);
}