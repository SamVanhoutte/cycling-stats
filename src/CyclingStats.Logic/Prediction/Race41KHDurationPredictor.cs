using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;

namespace CyclingStats.Logic.Prediction;

public class Race41KHDurationPredictor : IRaceDurationPredictor
{
    public async Task<IDictionary<string, RaceDurationPrediction>> PredictRaceDurationsAsync(
        IEnumerable<RaceDetails> races)
    {
        var results = new Dictionary<string, RaceDurationPrediction> { };
        foreach (var race in races)
        {
            if (race.Distance != null)
            {
                results.Add(race.Id, new RaceDurationPrediction
                {
                    Accuracy = 0.66, Distance = (int)race.Distance, 
                    Duration = (double)(race.Distance.Value / 41) * 3600
                });
            }
        }

        return results;
    }
}