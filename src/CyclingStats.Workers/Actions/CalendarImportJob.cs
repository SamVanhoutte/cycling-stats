using CyclingStats.DataAccess;
using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;
using CyclingStats.Workers.Configuration;
using Microsoft.Extensions.Options;

namespace CyclingStats.Workers.Actions;

public class CalendarImportJob : BaseWorker<BatchConfig>
{
    private readonly ILogger<CalendarImportJob> logger;
    private readonly IDataRetriever resultCollector;

    public CalendarImportJob(
        ILogger<CalendarImportJob> logger,
        IDataRetriever resultCollector,
        IOptions<ScheduleOptions> scheduleOptions,
        IOptions<SqlOptions> sqlSettings) : base(scheduleOptions, sqlSettings)
    {
        this.logger = logger;
        this.resultCollector = resultCollector;
    }

    protected override string TaskDescription =>"Imports calendar of the year.";
    protected override string WorkerName =>"CalendarImport";
    protected override ILogger Logger => logger;
    protected override async Task<bool> ProcessAsync(CancellationToken stoppingToken, BatchConfig config)
    {
        try
        {
            var yearToImport = config.ConfiguredYear?? DateTime.UtcNow.Year;
            var yearsRaces = await resultCollector.GetRaceCalendarAsync(yearToImport);
            
            using (var ctx = CyclingDbContext.CreateFromConnectionString(
                       sqlSettings.ConnectionString))
            {
                foreach (var race in yearsRaces)
                {
                    try
                    {
                        logger.LogInformation("Inserting data for race {RaceId}", race.Id);
                        await ctx.UpsertRaceDetailsAsync(race, RaceStatus.New);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        logger.LogError(e, "Error while importing race data of {Race}: {Exception}", race.Id, e.Message);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            logger.LogError(e, "Error while scraping race data: {Exception}", e.Message);
        }

        return false;
    }
}