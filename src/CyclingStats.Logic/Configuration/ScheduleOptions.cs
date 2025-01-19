namespace CyclingStats.Logic.Configuration;

public class ScheduleOptions
{
    public int DefaultWorkerIntervalSeconds { get; set; }
    public IDictionary<string, ScheduledTaskSetting>? WorkerSchedules { get; set; }

    public ScheduledTaskSetting LoadForWorker(string workerName)
    {
        if (WorkerSchedules?.ContainsKey(workerName) ?? false)
        {
            var configuredSchedule = WorkerSchedules[workerName];
            if (configuredSchedule.IntervalSeconds <= 0)
                configuredSchedule.IntervalSeconds = DefaultWorkerIntervalSeconds;
            return configuredSchedule;
        }

        return new ScheduledTaskSetting
        {
            IntervalSeconds = DefaultWorkerIntervalSeconds
        };
    }
}

public class ScheduledTaskSetting
{
    public bool Disabled { get; set; }
    public int IntervalSeconds { get; set; }
    public bool? OneTimeJob { get; set; }
}