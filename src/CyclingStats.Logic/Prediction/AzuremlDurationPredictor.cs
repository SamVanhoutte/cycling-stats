using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;

namespace CyclingStats.Logic.Prediction;

public class AzuremlDurationPredictor(AzuremlClient azuremlClient) : IRaceDurationPredictor
{
    public async Task<IDictionary<string, RaceDurationPrediction>> PredictRaceDurationsAsync(
        IEnumerable<RaceDetails> races)
    {
        var results = new Dictionary<string, RaceDurationPrediction> { };
        foreach (var race in races)
        {
            if (race.Distance != null)
            {
                var dataObject = new
                {
                    month = race.Date!.Value.Month,
                    year = race.Date!.Value.Year,
                    distance = race.Distance,
                    elevation = race.Elevation,
                    race.PointsScale,
                    race.UciScale,
                    race.ParcoursType,
                    race.ProfileScore,
                    race.RaceRanking,
                    race.StartlistQuality,
                    race.Classification,
                    race.Category
                };
                var result = await azuremlClient.CallInferenceAsync<double[]>(dataObject);
                if (result != null)
                {
                    results.Add(race.Id, new RaceDurationPrediction
                    {
                        Distance = (int)race.Distance,
                        Duration = result.First()
                    });
                }
            }
        }

        return results;
    }
}