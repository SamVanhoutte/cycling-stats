using CyclingStats.Logic.Configuration;
using Microsoft.Extensions.Options;

namespace CyclingStats.Workers;

public abstract class BaseWorker : BackgroundService
{
    protected readonly ScheduleOptions scheduleOptions;
    protected readonly SqlOptions sqlSettings;

    public BaseWorker(
        IOptions<ScheduleOptions> scheduleOptions,
        IOptions<SqlOptions> sqlSettings)
    {
        this.scheduleOptions = scheduleOptions.Value;
        this.sqlSettings = sqlSettings.Value;
    }

    protected abstract string TaskDescription { get; }
    protected abstract string WorkerName { get; }
    protected abstract ILogger Logger { get; }
    protected abstract Task ProcessAsync(CancellationToken stoppingToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var currentSchedule = scheduleOptions.LoadForWorker(WorkerName);
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!currentSchedule.Disabled)
            {
                Logger?.LogInformation(TaskDescription);
                await ProcessAsync(stoppingToken);
            }
            else
            {
                Logger?.LogInformation("{workerName} is disabled", WorkerName);
            }

            await Task.Delay(TimeSpan.FromSeconds(currentSchedule.IntervalSeconds), stoppingToken);
        }
    }
}