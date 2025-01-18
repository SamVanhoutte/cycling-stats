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
    public DbSet<Result> Results { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rider>().ToTable("Riders");
        modelBuilder.Entity<Race>().ToTable("Races");
        modelBuilder.Entity<Result>().ToTable("Results");

        modelBuilder.Entity<Race>()
            .Property(r => r.Status)
            .HasConversion(
                v => v.ToString(),
                v => (RaceStatus) Enum.Parse(typeof(RaceStatus), v));
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
                    await Results.AddAsync(new Result()
                    {
                        RiderID = result.Rider.Id, RaceID = raceDetails.Id,
                        Gap = result.DelaySeconds, Position = result.Position
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
        var existingRace = await Races.FindAsync(raceData.Id);
        if (existingRace == null)
        {
            await Races.AddAsync(new Race
            {
                Id = raceData.Id, Name = raceData.Name, Distance = raceData.Distance,
                RaceDate = raceData.Date, RaceType = raceData.RaceType, Status = newStatus
            });
        }
        else
        {
            existingRace.Name = raceData.Name;
            existingRace.RaceType = raceData.RaceType;
            existingRace.Distance = raceData.Distance;
            existingRace.RaceDate = raceData.Date;
            existingRace.Status = newStatus;
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