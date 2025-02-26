using CyclingStats.DataAccess.Entities;
using CyclingStats.Models;
using Microsoft.EntityFrameworkCore;
using Rider = CyclingStats.DataAccess.Entities.Rider;

namespace CyclingStats.DataAccess;

public partial class CyclingDbContext 
{


    public async Task<ICollection<Models.Rider>> GetRidersAsync(bool? detailsCompleted = null,
        string? monthToComplete = null, RiderStatus? status = null)
    {
        var query = Riders
            .Include(r => r.Profiles)
            .Where(r => true);
        if (status != null)
        {
            query = query.Where(r => r.Status == status);
        }

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
        var recentProfile = entity.Profiles?.FirstOrDefault(p => p.Month == DateTime.UtcNow.ToString("yyMM"));
        var riderType =  (RiderType?)entity.RiderType ?? recentProfile?.RiderType ?? RiderType.Unknown;
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
            RiderType = riderType,
            Sprinter = recentProfile?.Sprinter ?? -1, Climber = recentProfile?.Climber ?? -1,
            Puncheur = recentProfile?.Puncheur ?? -1, GC = recentProfile?.GC ?? -1,
            OneDay = recentProfile?.OneDay ?? -1, TimeTrialist = recentProfile?.TimeTrialist ?? -1,
            UciRanking = recentProfile?.UciRanking ?? -1, PcsRanking = recentProfile?.PcsRanking ?? -1
        };
    }

    public async Task<List<string>> GetRiderIdsFromResultsAsync()
    {
        var query = await Results.Select(r => r.RiderId).Distinct().ToListAsync();
        return query;
    }


    public async Task UpsertRidersProfilesAsync(IEnumerable<Models.Rider> riders)
    {
        foreach (var rider in riders)
        {
            await UpsertRiderAsync(rider);
            await UpsertRiderProfileAsync(rider);
        }

        await base.SaveChangesAsync();
    }

    public async Task EnsureRiderEntities(IEnumerable<Models.Rider> riders)
    {
        // Step 1: take all rider ids that should be ensured
        var riderIds = riders.Select(r => r.Id).ToList();

        // Step 2: Query the database to find which of these Rider IDs already exist
        var existingRiderIds = await Riders
            .Where(r => riderIds.Contains(r.Id))
            .Select(r => r.Id)
            .ToListAsync();

        // Step 3: Filter out the existing Rider IDs from the list
        var newRiderIds = riderIds.Except(existingRiderIds).ToList();

        // Step 4: Add the new Rider entities to the database
        var newRiders = riders
            .Where(r => newRiderIds.Contains(r.Id))
            .Select(r => new Rider
            {
                Id = r.Id,
                Name = r.Name,
                Team = r.Team,
                Status = RiderStatus.New,
                DetailsCompleted = false
            });

        await Riders.AddRangeAsync(newRiders);
        await SaveChangesAsync();
    }

    private async Task UpsertRiderAsync(Models.Rider rider, RiderStatus? riderStatus = null)
    {
        var existingRider = await Riders.FindAsync(rider.Id);
        if (existingRider == null)
        {
            var entity = CreateRiderEntity(rider);
            entity.Status = riderStatus ?? entity.Status;
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
            existingRider.RiderType = (int?)rider.RiderType;
            existingRider.BirthYear = rider.BirthYear;
            existingRider.Weight = rider.Weight;
            existingRider.Status = riderStatus ?? rider.Status ?? existingRider.Status;
        }
    }

    private Rider CreateRiderEntity(Models.Rider rider)
    {
        return new Rider
        {
            Id = rider.Id, Name = rider.Name, Team = rider.Team,
            PcsId = rider.PcsId,
            Height = rider.Height, Ranking2019 = rider.Ranking2019,
            Ranking2020 = rider.Ranking2020, Ranking2021 = rider.Ranking2021,
            Ranking2022 = rider.Ranking2022, Ranking2023 = rider.Ranking2023,
            Ranking2024 = rider.Ranking2024, Ranking2025 = rider.Ranking2025,
            Ranking2026 = rider.Ranking2026, DetailsCompleted = true,
            RiderType = (int?)rider.RiderType,
            BirthYear = rider.BirthYear, Weight = rider.Weight,
            Status = rider.Status ?? RiderStatus.New
        };
    }

    private async Task UpsertRiderProfileAsync(Models.Rider rider)
    {
        if (rider.ContainsProfile)
        {
            var month = DateTime.UtcNow.ToString("yyMM");
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
}