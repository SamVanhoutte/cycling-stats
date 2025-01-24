using CyclingStats.Logic.Configuration;

namespace CyclingStats.Workers.Configuration;

public interface IWorkerConfig
{
    void LoadSettings(ScheduledTaskSetting settings);
}