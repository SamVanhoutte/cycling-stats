using CyclingStats.DataAccess;
using CyclingStats.DataAccess.Entities;
using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CyclingStats.Logic.Services;

public class RaceService(IOptions<SqlOptions> sqlOptions) : IRaceService
{
    private SqlOptions sqlSettings = sqlOptions.Value;

    public async Task<List<RaceDetails>> GetRaceDataAsync()
    {
        using (var ctx = StatsDbContext.CreateFromConnectionString(
                   sqlSettings.ConnectionString))
        {
            return (await ctx.GetAllRacesAsync()).Select(CreateRaceDetails).ToList();
        }
    }

    public async Task<ICollection<RaceDetails>> GetRacesAsync(RaceStatus? status = null, RaceStatus? statusToExclude = null, bool? detailsCompleted = null,
        bool? pointsRetrieved = null, bool? resultsRetrieved = null)
    {
        using (var ctx = StatsDbContext.CreateFromConnectionString(
                   sqlSettings.ConnectionString))
        {
            return (await ctx.GetAllRacesAsync(status, statusToExclude, detailsCompleted, pointsRetrieved, resultsRetrieved)).Select(CreateRaceDetails).ToList();
        }
    }

    public async Task<RaceDetails?> GetRaceAsync(string raceId)
    {
        using (var ctx = StatsDbContext.CreateFromConnectionString(
                   sqlSettings.ConnectionString))
        {
            var raceEntity = await ctx.Races.FindAsync(raceId);
            return raceEntity == null? null : CreateRaceDetails(raceEntity);
        }
    }
    
    public async Task<List<RaceDetails>> GetStageRaceStagesAsync(string raceId)
    {
        using (var ctx = StatsDbContext.CreateFromConnectionString(
                   sqlSettings.ConnectionString))
        {
            var raceEntities = await ctx.Races.Where(r =>
                r.StageRaceId!=null && r.StageRaceId.Equals(raceId)).ToListAsync();
            return raceEntities.Select(CreateRaceDetails).ToList();
        }
    }

    public async Task UpsertRaceResultsAsync(RaceDetails raceDetails)
    {
        using (var ctx = StatsDbContext.CreateFromConnectionString(
                   sqlSettings.ConnectionString))
        {
            await ctx.UpsertRaceResultsAsync(raceDetails);
        }
    }

    public async Task UpsertRacePointsAsync(RaceDetails raceDetails)
    {
        using (var ctx = StatsDbContext.CreateFromConnectionString(
                   sqlSettings.ConnectionString))
        {
            await ctx.UpsertRacePointsAsync(raceDetails);
        }
    }

    public async Task UpdateRaceStatusAsync(string raceId, RaceStatus newStatus)
    {
        using (var ctx = StatsDbContext.CreateFromConnectionString(
                   sqlSettings.ConnectionString))
        {
            await ctx.UpdateRaceStatusAsync(raceId, newStatus);
        }
    }

    public async Task MarkRaceAsErrorAsync(string raceId, string error)
    {
        using (var ctx = StatsDbContext.CreateFromConnectionString(
                   sqlSettings.ConnectionString))
        {
            await ctx.MarkRaceAsErrorAsync(raceId, error);
        }
    }

    public async Task UpsertRaceDetailsAsync(RaceDetails raceData, bool stageRaceBatch, RaceStatus? newStatus = null)
    {
        using (var ctx = StatsDbContext.CreateFromConnectionString(
                   sqlSettings.ConnectionString))
        {
            await ctx.UpsertRaceDetailsAsync(raceData, newStatus, stageRaceBatch);
        }
    }

    private RaceDetails CreateRaceDetails(Race entity)
    {
        return new RaceDetails()
        {
            Id = entity.Id,
            Name = entity.Name,
            IsStageRace = entity.IsStageRace,
            Date = entity.RaceDate,
            RaceType = entity.RaceType,
            Distance = entity.Distance,
            Duration = entity.Duration,
            Status = entity.Status,
            StageRaceId = entity.StageRaceId,
            StageId = entity.StageId,
            ProfileImageUrl = entity.ProfileImageUrl,
            PointsScale = entity.PointsScale,
            UciScale = entity.UciScale,
            ParcoursType = entity.ParcoursType,
            ProfileScore = entity.ProfileScore,
            RaceRanking = entity.RaceRanking,
            Elevation = entity.Elevation,
            DecidingMethod = entity.DecidingMethod,
            Classification = entity.Classification,
            Category = entity.Category,
            PcsId = entity.PcsId,
            PcsUrl = entity.PcsUrl,
            WcsUrl = entity.WcsUrl,
            PointsRetrieved = entity.PointsRetrieved,
            Updated = entity.Updated,
            GameOrganized = entity.GameOrganized, DetailsCompleted = entity.DetailsCompleted,
            MarkForProcess = entity.MarkForProcess ?? false,
            ResultsRetrieved = entity.ResultsRetrieved,
            StartlistQuality = entity.StartlistQuality, StartListRetrieved = entity.StartListRetrieved,
        };
    }
}