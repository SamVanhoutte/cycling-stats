using CyclingStatsCollector.Entities;
using Microsoft.EntityFrameworkCore;

namespace CyclingStatsCollector.Data;

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
    }

    public async Task UpsertRidersAsync(IEnumerable<Models.Rider> riders)
    {
        foreach (var rider in riders)
        {
            var existingRider = await Riders.FindAsync(rider.Id);
            if (existingRider == null)
            {
                await Riders.AddAsync(new Rider{
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