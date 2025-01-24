using CyclingStats.Logic.Configuration;
using CyclingStats.Workers.Configuration;
using Microsoft.Extensions.Options;

namespace CyclingStats.Workers;

public abstract class BaseWorker<T> : BackgroundService where T: IWorkerConfig, new()
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
    protected abstract Task<bool> ProcessAsync(CancellationToken stoppingToken, T config);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var currentSchedule = scheduleOptions.LoadForWorker(WorkerName);
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!currentSchedule.Disabled)
            {
                T config = new T();
                config.LoadSettings(currentSchedule);
                Logger?.LogInformation(TaskDescription);
                var keepSpinning = await ProcessAsync(stoppingToken, config);
                if (currentSchedule.OneTimeJob ?? false)
                {
                    Logger?.LogInformation("{workerName} is a one time job. Break the loop", WorkerName);
                    return;
                }

                if (!keepSpinning)
                {
                    Logger?.LogInformation("{workerName} indicated to stop the loop.", WorkerName);
                    return;
                }
            }
            else
            {
                Logger?.LogInformation("{workerName} is disabled", WorkerName);
                return ;
            }

            await Task.Delay(TimeSpan.FromSeconds(currentSchedule.IntervalSeconds), stoppingToken);
        }
    }
    
    
}