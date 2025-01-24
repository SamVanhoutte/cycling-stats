namespace CyclingStats.Logic.Configuration;

public class ScheduledTaskSetting
{
    public bool Disabled { get; set; }
    public int IntervalSeconds { get; set; }
    public bool? OneTimeJob { get; set; }
    public Dictionary<string, string>? Settings { get; set; }
}