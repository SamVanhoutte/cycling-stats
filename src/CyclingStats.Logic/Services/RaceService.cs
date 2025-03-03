using CyclingStats.DataAccess;
using CyclingStats.DataAccess.Entities;
using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Options;
using Rider = CyclingStats.Models.Rider;

namespace CyclingStats.Logic.Services;

public class RaceService(IOptions<SqlOptions> sqlOptions, IDataRetriever dataScraper) : IRaceService
{
    private SqlOptions sqlSettings = sqlOptions.Value;

    public async Task<List<RaceDetails>> GetRaceDataAsync()
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        return (await ctx.GetAllRacesAsync()).Select(CreateRaceDetails).ToList();
    }

    public async Task<ICollection<RaceDetails>> GetRacesAsync(RaceStatus? status = null,
        RaceStatus? statusToExclude = null, bool? detailsCompleted = null,
        bool? pointsRetrieved = null, bool? resultsRetrieved = null)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        return (await ctx.GetAllRacesAsync(status, statusToExclude, detailsCompleted, pointsRetrieved,
            resultsRetrieved)).Select(CreateRaceDetails).ToList();
    }

    public async Task<RaceDetails?> GetRaceAsync(string raceId, bool includeResults = false, bool includePoints = false)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        IQueryable<Race> raceQuery = ctx.Races;
        if (includeResults)
        {
            raceQuery = raceQuery.Include(race => race.Results);
        }

        if (includePoints)
        {
            raceQuery = raceQuery.Include(race => race.Points);
        }

        var raceEntity = await raceQuery.FirstOrDefaultAsync(r => r.Id.Equals(raceId));
        return raceEntity == null ? null : CreateRaceDetails(raceEntity);
    }

    public async Task<List<RaceDetails>> GetStageRaceStagesAsync(string raceId)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        var raceEntities = await ctx.Races.Where(r =>
            r.StageRaceId != null && r.StageRaceId.Equals(raceId)).ToListAsync();
        return raceEntities.Select(CreateRaceDetails).ToList();
    }

    public async Task UpsertRaceResultsAsync(RaceDetails raceDetails)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        await ctx.UpsertRaceResultsAsync(raceDetails);
    }


    public async Task UpsertRacePointsAsync(RaceDetails raceDetails)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        await ctx.UpsertRacePointsAsync(raceDetails);
    }

    public async Task UpdateRaceStatusAsync(string raceId, RaceStatus newStatus)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        await ctx.UpdateRaceStatusAsync(raceId, newStatus);
    }

    public async Task MarkRaceAsErrorAsync(string raceId, string error)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        await ctx.MarkRaceAsErrorAsync(raceId, error);
    }

    public async Task UpsertRaceDetailsAsync(RaceDetails raceData, bool stageRaceBatch, RaceStatus? newStatus = null)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        await ctx.UpsertRaceDetailsAsync(raceData, newStatus, stageRaceBatch);
    }

    public async Task<StartGrid?> GetRaceStartGridAsync(string raceId)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        var game = await ctx.GetRaceStartGridAsync(raceId);
        var startGrid  = game== null ? null : CreateStartGrid(game);

        if(startGrid == null || startGrid.Updated.AddHours(1) < DateTime.UtcNow || startGrid.Riders.Count==0)
        {
            startGrid = await dataScraper.GetStartListAsync(raceId);
            if (startGrid == null)
            {
                return null;
            }
            await UpsertRaceStartGridAsync(raceId, startGrid);
        }
        return startGrid;
    }

    public async Task UpsertRaceStartGridAsync(string raceId, StartGrid startGrid)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        await ctx.EnsureRiderEntities(startGrid.Riders.Select(r => r.Rider));
        await ctx.UpdateGameStartGridAsync(raceId, startGrid);
    }

    private StartGrid CreateStartGrid(Game entity)
    {
        var gameStatus = (GameStatus)entity.Status;
        return new StartGrid
        {
            Status = gameStatus,
            StarBudget = entity.StarBudget ?? -1,
            Updated = entity.Updated,
            Riders = entity.StartList.Select(CreateGridEntry).ToList()
        };
    }

    private StartingRider CreateGridEntry(StartGridEntry startGridEntry)
    {
        var startingRider = new StartingRider
        {
            Stars = startGridEntry.Stars, Youth = startGridEntry.Youth ?? false
        };
        if (startGridEntry.Rider != null)
        {
            startingRider.Rider = RiderService.CreateRider(startGridEntry.Rider);
        }
        else
        {
            startingRider.Rider = new Rider { Id = startGridEntry.RiderId };
        }

        return startingRider;
    }
    
    public async Task<ICollection<RaceDetails>> FilloutResultsAsync(ICollection<RaceDetails> races)
    {
        await using var ctx = CyclingDbContext.CreateFromConnectionString(
            sqlSettings.ConnectionString);
        var completedRaces = new List<RaceDetails> { };
        foreach (var race in races)
        {
            if (!(race.Results?.Any() ?? false))
            {
                var fullRace = await GetRaceAsync(race.Id, true);
                completedRaces.Add(fullRace);
            }
            else
            {
                completedRaces.Add(race);
            }
        }

        return completedRaces;
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
            Results = entity.Results?.Select(rs => new RacerRaceResult{DelaySeconds = rs.Gap, Position = rs.Position, Rider = RiderService.CreateRider(rs.Rider, rs.RiderId)!}).ToList(),
            Points = entity.Points?.Select(pt => new RacerRacePoint{Position = pt.Position, Gc = pt.Gc, Points = pt.Points, Mc = pt.Mc, Pc = pt.Pc, Picked = pt.Picked, Stars = pt.Stars, Rider = RiderService.CreateRider(pt.Rider, pt.RiderId)!}).ToList(),
        };
    }
}