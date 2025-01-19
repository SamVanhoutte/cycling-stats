using CyclingStats.DataAccess.Entities;
using CyclingStats.Models;
using Microsoft.EntityFrameworkCore;
using Rider = CyclingStats.DataAccess.Entities.Rider;

namespace CyclingStats.DataAccess;

public class StatsDbContext : DbContext
{
    public StatsDbContext(DbContextOptions<StatsDbContext> options) : base(options)
    {
    }

    public DbSet<Rider> Riders { get; set; }
    public DbSet<Race> Races { get; set; }
    public DbSet<StageRace> StageRaces { get; set; }
    public DbSet<RaceResult> Results { get; set; }
    public DbSet<RacePoint> Points { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rider>().ToTable("Riders");
        modelBuilder.Entity<Race>().ToTable("Races");
        modelBuilder.Entity<RaceResult>().ToTable("RaceResults");
        modelBuilder.Entity<RacePoint>().ToTable("RacePoints");
        modelBuilder.Entity<StageRace>().ToTable("StageRace");

        modelBuilder.Entity<Race>()
            .Property(r => r.Status)
            .HasConversion(
                v => v.ToString(),
                v => (RaceStatus)Enum.Parse(typeof(RaceStatus), v))
            ;
        modelBuilder.Entity<Race>()
            .HasOne(r => r.StageRace)
            .WithOne()
            .HasForeignKey<Race>(r => r.StageRaceId)
            .IsRequired(false);
    }

    public async Task<ICollection<Race>> GetAllRacesAsync(RaceStatus? status = null)
    {
        var races = await Races.Where(r =>
            r.Status == (status ?? r.Status)).ToListAsync();
        return races;
    }

    public async Task UpsertRaceResultsAsync(Models.RaceDetails raceDetails)
    {
        if (raceDetails.Results.Any())
        {
            await UpsertRaceDataAsync(raceDetails, RaceStatus.Finished);
            foreach (var result in raceDetails.Results)
            {
                var existingResult = await Results.FindAsync(result.Rider.Id, raceDetails.Id);
                if (existingResult == null)
                {
                    await Results.AddAsync(new RaceResult()
                    {
                        RiderId = result.Rider.Id, RaceId = raceDetails.Id,
                        Gap = result.DelaySeconds, Position = result.Position,
                    });
                }
                else
                {
                    existingResult.Gap = result.DelaySeconds;
                    existingResult.Position = result.Position;
                    Results.Update(existingResult);
                }
            }

            await SaveChangesAsync();
        }
    }

    public async Task UpdateRaceStatusAsync(string raceId, RaceStatus newStatus)
    {
        var existingRace = await Races.FindAsync(raceId);
        if (existingRace != null)
        {
            existingRace.Status = newStatus;
            Races.Update(existingRace);
            await SaveChangesAsync();
        }
    }

    public async Task UpsertRaceDataAsync(Models.RaceDetails raceData, RaceStatus newStatus)
    {
        if (string.IsNullOrEmpty(raceData.Id))
            return;
        var existingRace = await Races.FindAsync(raceData.Id);
        if (existingRace == null)
        {
            var newRace =
                new Race
                {
                    Id = raceData.Id, Name = raceData.Name, Distance = raceData.Distance,
                    StartlistQuality = raceData.StartlistQuality,
                    RaceDate = raceData.Date, RaceType = raceData.RaceType, Status = newStatus,
                    Category = raceData.Category, IsStageRace = raceData.StageRace ?? false,
                    UciScale = raceData.UciScale, PointsScale = raceData.PointsScale, Elevation = raceData.Elevation,
                    DecidingMethod = raceData.DecidingMethod, Classification = raceData.Classification,
                    ProfileScore = raceData.ProfileScore, RaceRanking = raceData.RaceRanking,
                    ProfileImageUrl = raceData.ProfileImageUrl, 
                    ParcoursType = raceData.ParcoursType,
                    Results = raceData.Results?.Select(r => new RaceResult
                    {
                        RiderId = r.Rider.Id, RaceId = raceData.Id, Gap = r.DelaySeconds, Position = r.Position
                    }).ToList()
                };
            if (!string.IsNullOrEmpty(raceData.StageId))
            {
                newRace.StageRace = new StageRace { StageRaceId = raceData.StageId! };
            }
            await Races.AddAsync(newRace);
        }
        else
        {
            existingRace.Name = raceData.Name;
            existingRace.Status = newStatus;
            existingRace.RaceType = raceData.RaceType;
            existingRace.Distance = raceData.Distance;
            existingRace.RaceDate = raceData.Date;
            existingRace.Category = raceData.Category;
            existingRace.IsStageRace = raceData.StageRace ?? false;
            existingRace.UciScale = raceData.UciScale;
            existingRace.PointsScale = raceData.PointsScale;
            existingRace.Elevation = raceData.Elevation;
            existingRace.DecidingMethod = raceData.DecidingMethod;
            existingRace.Classification = raceData.Classification;
            existingRace.ProfileScore = raceData.ProfileScore;
            existingRace.RaceRanking = raceData.RaceRanking;
            existingRace.ProfileImageUrl = raceData.ProfileImageUrl;
            existingRace.ParcoursType = raceData.ParcoursType;
            existingRace.StartlistQuality = raceData.StartlistQuality;
            if(!string.IsNullOrEmpty(raceData.StageId))
            {
                existingRace.StageRace = new StageRace { StageRaceId = raceData.StageId! };
            }
            Races.Update(existingRace);
        }

        await SaveChangesAsync();
    }

    public async Task UpsertRidersAsync(IEnumerable<Models.Rider> riders)
    {
        foreach (var rider in riders)
        {
            var existingRider = await Riders.FindAsync(rider.Id);
            if (existingRider == null)
            {
                await Riders.AddAsync(new Rider
                {
                    Id = rider.Id, Name = rider.Name, Team = rider.Team,
                    Sprinter = rider.Sprinter, Climber = rider.Climber, Puncheur = rider.Puncheur,
                    AllRounder = rider.AllRounder, OneDay = rider.OneDay, TimeTrialist = rider.TimeTrialist
                });
            }
            else
            {
                existingRider.Id = rider.Id;
                existingRider.Name = rider.Name;
                existingRider.Team = rider.Team;
                existingRider.Sprinter = rider.Sprinter;
                existingRider.Climber = rider.Climber;
                existingRider.Puncheur = rider.Puncheur;
                existingRider.AllRounder = rider.AllRounder;
                existingRider.OneDay = rider.OneDay;
                existingRider.TimeTrialist = rider.TimeTrialist;
                Riders.Update(existingRider);
            }
        }

        await base.SaveChangesAsync();
    }

    public static StatsDbContext CreateFromConnectionString(string connectionString)
    {
        var options = new DbContextOptionsBuilder<StatsDbContext>().UseSqlServer(connectionString).Options;
        return new StatsDbContext(options);
    }
}