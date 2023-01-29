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

    public async Task InsertRidersAsync(IEnumerable< Models.Rider> riders)
    {
        await base.AddRangeAsync(riders.Select(r=>new Entities.Rider
        {
            Id = r.Id, Name = r.Name, Team = r.Team
        }));
        await base.SaveChangesAsync();
    } 
}