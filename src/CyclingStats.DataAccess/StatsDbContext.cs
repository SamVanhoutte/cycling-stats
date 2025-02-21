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

    // public DbSet<StageRace> StageRaces { get; set; }
    public DbSet<RaceResult> Results { get; set; }
    public DbSet<RacePoint> Points { get; set; }
    public DbSet<RiderProfile> RiderProfiles { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rider>().ToTable("Riders");
        modelBuilder.Entity<User>().ToTable("Users", "aerobets");
        modelBuilder.Entity<RiderProfile>().ToTable("RiderProfiles");
        modelBuilder.Entity<Race>().ToTable("Races");
        modelBuilder.Entity<RaceResult>().ToTable("RaceResults");
        modelBuilder.Entity<RacePoint>().ToTable("RacePoints");
        // modelBuilder.Entity<StageRace>().ToTable("StageRace");

        modelBuilder.Entity<Race>()
            .Property(r => r.Status)
            .HasConversion(
                v => v.ToString(),
                v => (RaceStatus)Enum.Parse(typeof(RaceStatus), v))
            ;

        modelBuilder
            .Entity<Rider>()
            .HasMany(u => u.Profiles)
            .WithOne(profile => profile.Rider)
            .HasForeignKey(a => a.RiderId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public async Task<ICollection<Race>> GetAllRacesAsync(RaceStatus? status = null, RaceStatus? statusToExclude = null,
        bool? detailsCompleted = null, bool? markedForProcessing = null,
        bool? pointsRetrieved = null, bool? resultsRetrieved = null)
    {
        var query = Races.Where(r => r.Status == (status ?? r.Status));
        if (statusToExclude != null)
        {
            query = query.Where(r => r.Status != statusToExclude);
        }

        if (markedForProcessing != null)
        {
            query = query.Where(r => r.MarkForProcess == markedForProcessing);
        }

        if (detailsCompleted != null)
        {
            query = query.Where(r => r.DetailsCompleted == detailsCompleted);
        }

        if (pointsRetrieved != null)
        {
            query = query.Where(r => r.PointsRetrieved == pointsRetrieved);
        }

        if (resultsRetrieved != null)
        {
            query = query.Where(r => r.ResultsRetrieved == resultsRetrieved);
        }

        var races = await query.ToListAsync();
        return races;
    }
    
    public async Task<ICollection<User>> GetAllUsersAsync()
    {
        var users = await Users.ToListAsync();
        return users;
    }

    public async Task<ICollection<Models.Rider>> GetRidersAsync(bool? detailsCompleted = null,
        string? monthToComplete = null)
    {
        var query = Riders
            .Include(r => r.Profiles)
            .Where(r => true);
        if (detailsCompleted != null)
        {
            query = query.Where(r => r.DetailsCompleted == detailsCompleted);
        }

        if (monthToComplete != null)
        {
            query = query.Where(r => !r.Profiles.Any(p => p.Month == monthToComplete));
        }

        var riders = await query.ToListAsync();
        return riders.Select((entity) => { return CreateRider(entity); }).ToList();
    }

    private static Models.Rider CreateRider(Rider entity)
    {
        var recentProfile = entity.Profiles?.FirstOrDefault(p => p.Month == DateTime.Now.ToString("yyMM"));
        return new Models.Rider
        {
            Id = entity.Id, Name = entity.Name, Team = entity.Team,
            Height = entity.Height, Ranking2019 = entity.Ranking2019,
            Status = entity.Status, PcsId = entity.PcsId,
            Ranking2020 = entity.Ranking2020, Ranking2021 = entity.Ranking2021,
            Ranking2022 = entity.Ranking2022, Ranking2023 = entity.Ranking2023,
            Ranking2024 = entity.Ranking2024, Ranking2025 = entity.Ranking2025,
            Ranking2026 = entity.Ranking2026, DetailsCompleted = entity.DetailsCompleted,
            BirthYear = entity.BirthYear, Weight = entity.Weight,
            Sprinter = recentProfile?.Sprinter ?? -1, Climber = recentProfile?.Climber ?? -1,
            Puncheur = recentProfile?.Puncheur ?? -1, GC = recentProfile?.GC ?? -1,
            OneDay = recentProfile?.OneDay ?? -1, TimeTrialist = recentProfile?.TimeTrialist ?? -1,
            UciRanking = recentProfile?.UciRanking ?? -1, PcsRanking = recentProfile?.PcsRanking ?? -1
        };
    }

    public async Task<List<string>> GetRidersFromResultsAsync()
    {
        var query = await Results.Select(r => r.RiderId).Distinct().ToListAsync();
        return query;
    }

    public async Task UpsertRaceResultsAsync(Models.RaceDetails raceDetails)
    {
        if (raceDetails.Results.Any())
        {
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
                    //Results.Update(existingResult);
                }
            }

            await UpsertRaceDetailsAsync(raceDetails, RaceStatus.Finished);

            await SaveChangesAsync();
        }
    }

    public async Task UpsertRacePointsAsync(Models.RaceDetails raceDetails)
    {
        if (raceDetails.Points.Any())
        {
            await UpsertRaceDetailsAsync(raceDetails, RaceStatus.Finished);
            foreach (var racePoint in raceDetails.Points)
            {
                var existingPoint = await Points.FindAsync(racePoint.Rider.Id, raceDetails.Id);
                if (existingPoint == null)
                {
                    await Points.AddAsync(new RacePoint()
                    {
                        RiderId = racePoint.Rider.Id, RaceId = raceDetails.Id,
                        Points = racePoint.Points, Position = racePoint.Position,
                        Stars = racePoint.Stars, Picked = racePoint.Picked, Mc = racePoint.Mc ?? -1,
                        Pc = racePoint.Pc ?? -1, Gc = racePoint.Gc ?? -1
                    });
                }
                else
                {
                    existingPoint.Points = racePoint.Points;
                    existingPoint.Position = racePoint.Position;
                    existingPoint.Stars = racePoint.Stars;
                    existingPoint.Picked = racePoint.Picked;
                    existingPoint.Mc = racePoint.Mc ?? -1;
                    existingPoint.Pc = racePoint.Pc ?? -1;
                    existingPoint.Gc = racePoint.Gc ?? -1;
                    //Points.Update(existingPoint);
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
            //Races.Update(existingRace);
            await SaveChangesAsync();
        }
    }

    public async Task MarkRaceAsErrorAsync(string raceId, string error)
    {
        if (string.IsNullOrEmpty(raceId))
            return;
        var existingRace = await Races.FindAsync(raceId);
        if (existingRace != null)
        {
            existingRace.Status = RaceStatus.Error;
            existingRace.Error = error;
            existingRace.Updated = DateTime.Now;
            //Races.Update(existingRace);
            await SaveChangesAsync();
        }
    }

    public async Task UpsertRaceDetailsAsync(Models.RaceDetails raceData, RaceStatus? newStatus = null,
        bool stageRaceBatch = false) //, string? error = null)
    {
        string error = null;
        if (string.IsNullOrEmpty(raceData.Id))
            return;
        var existingRace = await Races.FindAsync(raceData.Id);
        if (existingRace == null)
        {
            var newRace =
                new Race
                {
                    Id = raceData.Id, Name = raceData.Name,
                    Distance = raceData.Distance,
                    StartlistQuality = raceData.StartlistQuality, PcsId = raceData.PcsId,
                    RaceDate = raceData.Date, RaceType = raceData.RaceType,
                    Category = raceData.Category, IsStageRace = raceData.StageRace,
                    ResultsRetrieved = raceData.ResultsRetrieved, StartListRetrieved = raceData.StartListRetrieved,
                    DetailsCompleted = raceData.DetailsCompleted,
                    PointsRetrieved = raceData.PointsRetrieved, GameOrganized = raceData.GameOrganized,
                    UciScale = raceData.UciScale, PointsScale = raceData.PointsScale, Elevation = raceData.Elevation,
                    DecidingMethod = raceData.DecidingMethod, Classification = raceData.Classification,
                    ProfileScore = raceData.ProfileScore, RaceRanking = raceData.RaceRanking,
                    MarkForProcess = raceData.MarkForProcess,
                    ProfileImageUrl = raceData.ProfileImageUrl, Error = error,
                    PcsUrl = raceData.PcsUrl, WcsUrl = raceData.WcsUrl, Duration = raceData.Duration,
                    StageId = raceData.StageId, StageRaceId = raceData.StageRaceId,
                    ParcoursType = raceData.ParcoursType, Updated = null,
                    Results = raceData.Results?.Select(r => new RaceResult
                    {
                        RiderId = r.Rider.Id, RaceId = raceData.Id, Gap = r.DelaySeconds, Position = r.Position
                    }).ToList()
                };
            if (newStatus != null) newRace.Status = newStatus.Value;
            await Races.AddAsync(newRace);
        }
        else
        {
            if (!stageRaceBatch)
            {
                existingRace.PcsId = raceData.PcsId;
                existingRace.StageId = raceData.StageId;
                existingRace.StageRaceId = raceData.StageRaceId;
                existingRace.Name = raceData.Name;
                existingRace.RaceType = raceData.RaceType ?? existingRace.RaceType;
                existingRace.Distance = raceData.Distance ?? existingRace.Distance;
                existingRace.RaceDate = raceData.Date ?? existingRace.RaceDate;
                existingRace.Category = raceData.Category ?? existingRace.Category;
                existingRace.ResultsRetrieved = raceData.ResultsRetrieved;
                existingRace.StartListRetrieved = raceData.StartListRetrieved;
                existingRace.DetailsCompleted = raceData.DetailsCompleted;
                existingRace.PointsRetrieved = raceData.PointsRetrieved;
                existingRace.GameOrganized = raceData.GameOrganized;
                existingRace.IsStageRace = raceData.StageRace;
                existingRace.UciScale = raceData.UciScale ?? existingRace.UciScale;
                existingRace.Duration = raceData.Duration ?? existingRace.Duration;
                existingRace.PointsScale = raceData.PointsScale ?? existingRace.PointsScale;
                existingRace.Error = error;
                existingRace.Elevation = raceData.Elevation ?? existingRace.Elevation;
                existingRace.PcsUrl = raceData.PcsUrl ?? existingRace.PcsUrl;
                existingRace.WcsUrl = raceData.WcsUrl ?? existingRace.WcsUrl;
                existingRace.MarkForProcess = raceData.MarkForProcess;
                existingRace.DecidingMethod = raceData.DecidingMethod ?? existingRace.DecidingMethod;
                existingRace.Classification = raceData.Classification ?? existingRace.Classification;
                existingRace.ProfileScore = raceData.ProfileScore ?? existingRace.ProfileScore;
                existingRace.RaceRanking = raceData.RaceRanking ?? existingRace.RaceRanking;
                existingRace.ProfileImageUrl = raceData.ProfileImageUrl ?? existingRace.ProfileImageUrl;
                existingRace.ParcoursType = raceData.ParcoursType ?? existingRace.ParcoursType;
                existingRace.StartlistQuality = raceData.StartlistQuality ?? existingRace.StartlistQuality;
            }
            existingRace.Updated = DateTime.Now;
            if (newStatus != null) existingRace.Status = newStatus.Value;
        }

        await SaveChangesAsync();
    }

    public async Task UpsertRiderProfilesAsync(IEnumerable<Models.Rider> riders)
    {
        foreach (var rider in riders)
        {
            await UpsertRiderAsync(rider);
            await UpsertRiderProfileAsync(rider);
        }

        await base.SaveChangesAsync();
    }

    private async Task UpsertRiderAsync(Models.Rider rider, RiderStatus? riderStatus = null)
    {
        var existingRider = await Riders.FindAsync(rider.Id);
        if (existingRider == null)
        {
            var entity = new Rider
            {
                Id = rider.Id, Name = rider.Name, Team = rider.Team,
                PcsId = rider.PcsId,
                Height = rider.Height, Ranking2019 = rider.Ranking2019,
                Ranking2020 = rider.Ranking2020, Ranking2021 = rider.Ranking2021,
                Ranking2022 = rider.Ranking2022, Ranking2023 = rider.Ranking2023,
                Ranking2024 = rider.Ranking2024, Ranking2025 = rider.Ranking2025,
                Ranking2026 = rider.Ranking2026, DetailsCompleted = true,
                BirthYear = rider.BirthYear, Weight = rider.Weight,
                Status = riderStatus ?? rider.Status ?? RiderStatus.New
            };

            await Riders.AddAsync(entity);
        }
        else
        {
            existingRider.Id = rider.Id;
            existingRider.PcsId = rider.PcsId;
            existingRider.Name = rider.Name;
            existingRider.DetailsCompleted = rider.DetailsCompleted;
            existingRider.Team = rider.Team;
            existingRider.Height = rider.Height;
            existingRider.Ranking2019 = rider.Ranking2019;
            existingRider.Ranking2020 = rider.Ranking2020;
            existingRider.Ranking2021 = rider.Ranking2021;
            existingRider.Ranking2022 = rider.Ranking2022;
            existingRider.Ranking2023 = rider.Ranking2023;
            existingRider.Ranking2024 = rider.Ranking2024;
            existingRider.Ranking2025 = rider.Ranking2025;
            existingRider.Ranking2026 = rider.Ranking2026;
            existingRider.BirthYear = rider.BirthYear;
            existingRider.Weight = rider.Weight;
            existingRider.Status = riderStatus ?? rider.Status ?? existingRider.Status;
        }
    }

    private async Task UpsertRiderProfileAsync(Models.Rider rider)
    {
        if (rider.ContainsProfile)
        {
            var month = DateTime.Now.ToString("yyMM");
            var existingProfile = await RiderProfiles.FindAsync(rider.Id, month);
            if (existingProfile == null)
            {
                await RiderProfiles.AddAsync(new RiderProfile
                {
                    RiderId = rider.Id, Month = month,
                    Sprinter = rider.Sprinter, Climber = rider.Climber, Puncheur = rider.Puncheur,
                    GC = rider.GC, OneDay = rider.OneDay, TimeTrialist = rider.TimeTrialist,
                    UciRanking = rider.UciRanking, PcsRanking = rider.PcsRanking,
                });
            }
            else
            {
                existingProfile.Sprinter = rider.Sprinter;
                existingProfile.Climber = rider.Climber;
                existingProfile.Puncheur = rider.Puncheur;
                existingProfile.GC = rider.GC;
                existingProfile.OneDay = rider.OneDay;
                existingProfile.TimeTrialist = rider.TimeTrialist;
                existingProfile.UciRanking = rider.UciRanking;
                existingProfile.PcsRanking = rider.PcsRanking;
            }
        }
    }

    public static StatsDbContext CreateFromConnectionString(string connectionString)
    {
        var options = new DbContextOptionsBuilder<StatsDbContext>().UseSqlServer(connectionString).Options;
        return new StatsDbContext(options);
    }

    private const string durationQuery = @"
                                        select month(racedate) as month, year(racedate) as year, distance, elevation, PointsScale, UciScale, ParcoursType, ProfileScore, RaceRanking, StartlistQuality, Classification, Category, Duration
                                        from races 
                                        where duration > 600 and distance > 10 and (StageRace=0 OR StageId is not null)
                                        ";
}