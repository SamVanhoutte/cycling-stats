using CyclingStats.Logic.Configuration;
using CyclingStats.Models.Extensions;

namespace CyclingStats.Workers.Configuration;

public class BatchConfig : IWorkerConfig
{
    public void LoadSettings(ScheduledTaskSetting settings)
    {
        AgeMinutes = settings.Settings.GetIntSetting("ageMinutes", 1);
        BatchSize = settings.Settings.GetIntSetting("batchSize", 0);
        Year = settings.Settings.GetIntSetting("year", 0);
        TopResults = settings.Settings.GetIntSetting("topResults", 50);

        if (settings.Settings?.ContainsKey("uciScales") ?? false)
        {
            RaceScales = settings.Settings["uciScales"].ToLower().Split("/").ToList();
        }
        
        if(settings?.Settings.TryGetValue("year", out var year) ?? false)
        {
            if (int.TryParse(year, out var yearToImport))
            {
                ConfiguredYear = yearToImport;
            }
        }

        if (settings?.Settings.TryGetValue("onlyMissingRiders", out var onlyMissingRidersVal) ?? false)
        {
            if (bool.TryParse(onlyMissingRidersVal, out var onlyMissingRiders))
            {
                OnlyMissingRiders = onlyMissingRiders;
            }
        }
    }

    public int? ConfiguredYear { get; set; }

    public int TopResults { get; set; }

    public List<string>? RaceScales { get; set; }

    public int BatchSize { get; set; }
    public bool OnlyMissingRiders { get; set; }

    public int AgeMinutes { get; set; }
    public int Year { get; set; }
}

