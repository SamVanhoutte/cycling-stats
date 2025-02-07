using CyclingStats.DataAccess;
using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;
using CyclingStats.Workers.Configuration;
using Microsoft.Extensions.Options;

namespace CyclingStats.Workers.Workers;

public class RiderProfileWorker : BaseWorker<BatchConfig>
{
    private readonly ILogger<RiderProfileWorker> logger;
    private readonly IDataRetriever resultCollector;

    public RiderProfileWorker(
        ILogger<RiderProfileWorker> logger,
        IDataRetriever resultCollector,
        IOptions<ScheduleOptions> scheduleOptions,
        IOptions<SqlOptions> sqlSettings) : base(scheduleOptions, sqlSettings)
    {
        this.logger = logger;
        this.resultCollector = resultCollector;
    }

    protected override string TaskDescription => "Collecting rider information.";
    protected override string WorkerName => "RiderProfiles";
    protected override ILogger Logger => logger;

    protected override async Task<bool> ProcessAsync(CancellationToken stoppingToken, BatchConfig config)
    {
        try
        {
            using (var ctx = StatsDbContext.CreateFromConnectionString(
                       sqlSettings.ConnectionString))
            {
                // Get all riders that we have in the results
                var ridersToUpdate = (await ctx.GetRidersAsync()).ToList();
                var riderids = await ctx.GetRidersFromResultsAsync();

                // First check which riders are new to be inserted
                var missingRiderIds =riderids.Where(id => !ridersToUpdate.Any(rtu=>rtu.Id.Equals(id, StringComparison.CurrentCultureIgnoreCase))).ToList();
                var missingRiders = missingRiderIds.Select(id => new Models.Rider{Id = id, Name = "", Team = ""}).ToList();
                ridersToUpdate.AddRange(missingRiders);

                // Filter out riders that are complete for this month
                ridersToUpdate = ridersToUpdate.Where(r=>!(r.DetailsCompleted && r.ContainsProfile)).ToList();
                ridersToUpdate = ridersToUpdate.Where(r=>r.Status!= RiderStatus.NotFound && r.Status!= RiderStatus.Error).ToList();
                if (config.BatchSize > 0) ridersToUpdate = ridersToUpdate.Take(config.BatchSize).ToList();
                if(ridersToUpdate.Any())
                {
                    var profiles = new List<Models.Rider> { };
                    foreach (var ri in ridersToUpdate)
                    {
                        try
                        {
                            logger.LogInformation("Updating data for rider {RiderId}", ri.Id);
                            var riderProfile = await resultCollector.GetRiderProfileAsync(ri);
                            if (riderProfile == null)
                            {
                                if (string.IsNullOrEmpty(ri.PcsId))
                                {
                                    var pcsId = await resultCollector.GetPcsRiderIdAsync(ri);
                                    if (!string.IsNullOrEmpty(pcsId))
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"PCS Rider id looked up as {pcsId}");
                                        Console.ResetColor();
                                        ri.Status = RiderStatus.New;
                                        ri.PcsId = pcsId;
                                        ri.DetailsCompleted = false;
                                    }
                                    else
                                    {
                                        ri.Status = RiderStatus.NotFound;
                                        ri.DetailsCompleted = false;
                                    }
                                    profiles.Add( ri);
                                }
                            }
                            else
                            {
                                riderProfile.Status ??= RiderStatus.Active;
                                riderProfile.DetailsCompleted = true;
                                profiles.Add( riderProfile);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            logger.LogError(e, "Error while updating rider data of {Rider}: {Exception}", ri.Id, e.Message);
                        }
                    }

                    await ctx.UpsertRiderProfilesAsync(profiles);
                    await ctx.SaveChangesAsync(stoppingToken);
                    //var riderInfo = await resultCollector.GetRiderProfileAsync(ri)
                }
                else
                {
                    Console.WriteLine("No new riders found to update");
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            logger.LogError(e, "Error while scraping riders data: {Exception}", e.Message);
        }

        return true;
    }
}