namespace CyclingStats.Models.Configuration;

public class ScheduleOptions
{
    public IDictionary<string, ScheduledTaskSetting> WorkerSchedules { get; set; }
}

public class ScheduledTaskSetting
{
    public bool Disabled { get; set; }
    public int IntervalSeconds { get; set; }
}