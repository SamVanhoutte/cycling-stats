using CyclingStats.DataAccess.Entities;
using CyclingStats.Models;
using Microsoft.EntityFrameworkCore;
using Rider = CyclingStats.DataAccess.Entities.Rider;

namespace CyclingStats.DataAccess;

public partial class CyclingDbContext : DbContext
{
    public CyclingDbContext(DbContextOptions<CyclingDbContext> options) : base(options)
    {
    }

    public DbSet<Rider> Riders { get; set; }
    public DbSet<Race> Races { get; set; }

    // public DbSet<StageRace> StageRaces { get; set; }
    public DbSet<RaceResult> Results { get; set; }
    public DbSet<RacePoint> Points { get; set; }
    public DbSet<RiderProfile> RiderProfiles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<StartGridEntry> StartGridEntries { get; set; }
    public DbSet<UserRider> UserFavoriteRiders { get; set; }
    public DbSet<UserGame> UserGames { get; set; }
    public DbSet<UserTeam> UserGameTeams { get; set; }
    public DbSet<PlayerToWatch> WatchedPlayers { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rider>().ToTable("Riders");
        modelBuilder.Entity<User>().ToTable("Users", "aerobets");
        modelBuilder.Entity<RiderProfile>().ToTable("RiderProfiles");
        modelBuilder.Entity<Race>().ToTable("Races");
        modelBuilder.Entity<RaceResult>().ToTable("RaceResults");
        modelBuilder.Entity<RacePoint>().ToTable("RacePoints");
        modelBuilder.Entity<StartGridEntry>().ToTable("StartGrids");
        modelBuilder.Entity<Game>().ToTable("Games");
        modelBuilder.Entity<UserRider>().ToTable("UserRiders", "aerobets");
        modelBuilder.Entity<UserGame>().ToTable("UserGames", "aerobets");
        modelBuilder.Entity<UserTeam>().ToTable("UserTeams", "aerobets");
        modelBuilder.Entity<PlayerToWatch>().ToTable("PlayerWatchLists", "aerobets");
        
        modelBuilder.Entity<UserRider>()
            .HasKey(ur => new { ur.UserId, ur.RiderId });

        modelBuilder.Entity<UserRider>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRiders)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRider>()
            .HasOne(ur => ur.Rider)
            .WithMany(r => r.UserRiders)
            .HasForeignKey(ur => ur.RiderId);
        
        modelBuilder.Entity<UserGame>()
            .HasKey(ur => new { ur.UserId, ur.GameId });

        modelBuilder.Entity<UserGame>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserGames)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserGame>()
            .HasOne(ur => ur.Game)
            .WithMany(r => r.UserGames)
            .HasForeignKey(ur => ur.GameId);
        
        modelBuilder.Entity<Race>()
            .Property(r => r.Status)
            .HasConversion(
                v => v.ToString(),
                v => (RaceStatus)Enum.Parse(typeof(RaceStatus), v))
            ;

        modelBuilder
            .Entity<Race>()
            .HasMany(u => u.Results)
            .WithOne(result => result.Race)
            .HasForeignKey(a => a.RaceId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
        
        modelBuilder
            .Entity<Race>()
            .HasMany(u => u.Points)
            .WithOne(result => result.Race)
            .HasForeignKey(a => a.RaceId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
        
        modelBuilder
            .Entity<User>()
            .HasMany(u => u.PlayersToWatch)
            .WithOne(profile => profile.Player)
            .HasForeignKey(a => a.PlayerUserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        modelBuilder
            .Entity<Rider>()
            .HasMany(u => u.Profiles)
            .WithOne(profile => profile.Rider)
            .HasForeignKey(a => a.RiderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<Game>()
            .HasMany(u => u.StartList)
            .WithOne(profile => profile.Game)
            .HasForeignKey(a => a.RaceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<StartGridEntry>()
            .HasOne(u => u.Rider)
            .WithMany()
            .HasForeignKey(a => a.RiderId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
        
        modelBuilder.Entity<User>()
            .HasMany(u => u.FavoriteRiders)
            .WithMany(r => r.FavoritedBy)
            .UsingEntity<UserRider>(
                j => j
                    .HasOne(ur => ur.Rider)
                    .WithMany(r => r.UserRiders)
                    .HasForeignKey(ur => ur.RiderId),
                j => j
                    .HasOne(ur => ur.User)
                    .WithMany(u => u.UserRiders)
                    .HasForeignKey(ur => ur.UserId));

        modelBuilder.Entity<UserTeam>()
            .HasOne(ur => ur.User)
            .WithMany(r => r.Teams)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder
            .Entity<UserTeam>();
    }

    public static CyclingDbContext CreateFromConnectionString(string connectionString)
    {
        var options = new DbContextOptionsBuilder<CyclingDbContext>().UseSqlServer(connectionString).Options;
        return new CyclingDbContext(options);
    }

    private const string durationQuery = @"
                                        select month(racedate) as month, year(racedate) as year, distance, elevation, PointsScale, UciScale, ParcoursType, ProfileScore, RaceRanking, StartlistQuality, Classification, Category, Duration
                                        from races 
                                        where duration > 600 and distance > 10 and (StageRace=0 OR StageId is not null)
                                        ";
}