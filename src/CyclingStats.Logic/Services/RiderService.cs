using CyclingStats.DataAccess;
using CyclingStats.DataAccess.Entities;
using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rider = CyclingStats.Models.Rider;

namespace CyclingStats.Logic.Services;

public class RiderService(IOptions<SqlOptions> sqlOptions) : IRiderService
{
    private SqlOptions sqlSettings = sqlOptions.Value;


    public async Task<List<string>> GetRidersFromResultsAsync()
    {
        using (var ctx = StatsDbContext.CreateFromConnectionString(
                   sqlSettings.ConnectionString))
        {
            return await ctx.GetRiderIdsFromResultsAsync();
        }
    }

    public async Task UpsertRiderProfilesAsync(IEnumerable<Rider> riders)
    {
        using (var ctx = StatsDbContext.CreateFromConnectionString(
                   sqlSettings.ConnectionString))
        {
            await ctx.UpsertRidersProfilesAsync(riders);
            await ctx.SaveChangesAsync();
        }
    }

    public async Task<ICollection<Rider>> GetRidersAsync(bool? detailsCompleted = null, bool? excludeRetiredRiders = null,
        string? monthToComplete = null)
    {
        using (var ctx = StatsDbContext.CreateFromConnectionString(
                   sqlSettings.ConnectionString))
        {
            var query =  ctx.Riders
                .Include(r => r.Profiles)
                .Where(x => true);

            if (excludeRetiredRiders != null)
            {
                query = query.Where(x => x.Status != RiderStatus.Retired);
            }
            var riders = await query.ToListAsync();
            return riders.Select(CreateRider).ToList();
        }
    }

    public async Task<Rider?> GetRiderAsync(string riderId)
    {
        using (var ctx = StatsDbContext.CreateFromConnectionString(
                   sqlSettings.ConnectionString))
        {
            var rider =  await ctx.Riders
                .Include(r => r.Profiles)
                .Where(x => x.Id.Equals(riderId))
                .FirstOrDefaultAsync();

            return rider == null ? null : CreateRider(rider);
        }
    }

    public static Rider CreateRider(CyclingStats.DataAccess.Entities.Rider rider)
    {
        var profile = rider.Profiles?.OrderByDescending(pr => pr.Month).FirstOrDefault();
        var riderType = ((RiderType?)rider.RiderType) ?? profile?.RiderType ?? RiderType.Unknown;
        return new Rider
        {
            Id = rider.Id,
            Name = rider.Name,
            Team = rider.Team,
            Ranking2019 = rider.Ranking2019,
            Ranking2020 = rider.Ranking2020,
            Ranking2021 = rider.Ranking2021,
            Ranking2022 = rider.Ranking2022,
            Ranking2023 = rider.Ranking2023,
            Ranking2024 = rider.Ranking2024,
            Ranking2025 = rider.Ranking2025,
            Ranking2026 = rider.Ranking2026,
            DetailsCompleted = rider.DetailsCompleted,
            Status = rider.Status,
            RiderType = riderType,
            BirthYear = rider.BirthYear,
            Weight = rider.Weight,
            Height = rider.Height,
            PcsId = rider.PcsId,
            Climber = profile?.Climber ?? 0,
            GC = profile?.GC ?? 0,
            OneDay = profile?.OneDay ?? 0,
            Puncheur = profile?.Puncheur ?? 0,
            Sprinter = profile?.Sprinter ?? 0,
            TimeTrialist = profile?.TimeTrialist ?? 0,
            UciRanking = profile?.UciRanking ?? 0
        };
    }
}