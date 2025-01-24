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