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
    private readonly IRiderService riderService;
    private readonly IDataRetriever resultCollector;

    public RiderProfileWorker(
        ILogger<RiderProfileWorker> logger,
        IRiderService riderService,
        IDataRetriever resultCollector,
        IOptions<ScheduleOptions> scheduleOptions,
        IOptions<SqlOptions> sqlSettings) : base(scheduleOptions, sqlSettings)
    {
        this.logger = logger;
        this.riderService = riderService;
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
                var existingRiders = await ctx.GetRidersAsync();
                var riderResultIds = await riderService.GetRidersFromResultsAsync();
                var ridersToUpdate = new List<Rider> { };
                
                // Take the riders that don't have an entity yet , from the results
                var missingRiderIds =riderResultIds.Where(id => !existingRiders.Any(rtu=>rtu.Id.Equals(id, StringComparison.CurrentCultureIgnoreCase))).ToList();
                
                if (config.OnlyMissingRiders)
                {
                    // Add the riders that have a status set to New
                    ridersToUpdate.AddRange(existingRiders.Where(rider => rider.Status== RiderStatus.New));
                    ridersToUpdate.AddRange(missingRiderIds.Select(id => new Models.Rider{Id = id, Name = "", Team = "", Status = RiderStatus.New}));
                }
                else
                {
                    ridersToUpdate.AddRange(missingRiderIds.Select(id => new Models.Rider{Id = id, Name = "", Team = "", Status = RiderStatus.New}));
                    ridersToUpdate.AddRange(existingRiders);
                }

                // Filter out riders that are complete for this month
                ridersToUpdate = ridersToUpdate.Where(r=>!(r.DetailsCompleted && r.ContainsProfile)).ToList();
                ridersToUpdate = ridersToUpdate.Where(r=>r.Status!= RiderStatus.NotFound && r.Status!= RiderStatus.Error && r.Status!= RiderStatus.Retired).ToList();
                Console.WriteLine($"Riders to update: {ridersToUpdate.Count}");
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

                    await riderService.UpsertRiderProfilesAsync(profiles);
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