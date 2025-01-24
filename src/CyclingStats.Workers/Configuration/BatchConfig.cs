using CyclingStats.Logic.Configuration;
using CyclingStats.Models.Extensions;

namespace CyclingStats.Workers.Configuration;

public class BatchConfig : IWorkerConfig
{
    public void LoadSettings(ScheduledTaskSetting settings)
    {
        HoursAge = settings.Settings.GetIntSetting("ageHours", 1);
        BatchSize = settings.Settings.GetIntSetting("batchSize", 0);
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
    }

    public int? ConfiguredYear { get; set; }

    public int TopResults { get; set; }

    public List<string>? RaceScales { get; set; }

    public int BatchSize { get; set; }

    public int HoursAge { get; set; }
}

