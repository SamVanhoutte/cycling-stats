using CyclingStats.DataAccess.Entities;
using CyclingStats.Models;
using Microsoft.EntityFrameworkCore;
using Rider = CyclingStats.DataAccess.Entities.Rider;

namespace CyclingStats.DataAccess;

public partial class CyclingDbContext 
{
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

    public async Task UpdateGameStartGridAsync(string gameId, StartGrid startGrid)
    {
        try
        {
            // Now upsert the Game
            var existingRace = await Games.Include(game => game.StartList).Where(game => game.RaceId == gameId).FirstOrDefaultAsync();
            if (existingRace != null)
            {
                existingRace.Status = (int)startGrid.Status;
                existingRace.StarBudget = startGrid.StarBudget;
                existingRace.Updated = DateTime.UtcNow;
                existingRace.StartList ??= [];
                var existingDbEntries = existingRace.StartList!.ToList() ;
                var gridEntries = startGrid.Riders.Select(r => CreateStartGridEntry(gameId, r)).ToList();

                // Remove entries that are no longer in the new list
                foreach (var entry in existingDbEntries.Where(
                             entry => !gridEntries.Any(e => e.RiderId == entry.RiderId)))
                {
                    StartGridEntries.Remove(entry);
                }

                // Add or update entries
                foreach (var newEntry in gridEntries)
                {
                    var existingEntry = existingDbEntries.FirstOrDefault(e => e.RiderId == newEntry.RiderId);
                    if (existingEntry == null)
                    {
                        existingRace.StartList.Add(newEntry);
                    }
                    else
                    {
                        existingEntry.Stars = newEntry.Stars;
                        existingEntry.RiderType = newEntry.RiderType;
                        existingEntry.Youth = newEntry.Youth;
                    }
                }
                existingRace.StartList = startGrid.Riders.Select(r => CreateStartGridEntry(gameId, r)).ToList();
            }
            else
            {
                var newRace = new Game
                {
                    RaceId = gameId,
                    Status = (int)startGrid.Status,
                    StarBudget = startGrid.StarBudget,
                    Updated = DateTime.UtcNow,
                    StartList = startGrid.Riders.Select(r => CreateStartGridEntry(gameId, r)).ToList()
                };
                Games.Add(newRace);
            }

            await SaveChangesAsync();
        }
        catch (Exception e)
        {
            throw;
        }
    }

    private StartGridEntry CreateStartGridEntry(string raceId, StartingRider entry)
    {
        return new StartGridEntry
        {
            RiderId = entry.Rider.Id, RaceId = raceId, Created = DateTime.UtcNow,
            RiderType = (int)entry.RiderType, Stars = entry.Stars, Youth = entry.Youth
        };
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
            existingRace.Updated = DateTime.UtcNow;
            //Races.Update(existingRace);
            await SaveChangesAsync();
        }
    }

    public async Task<Game?> GetRaceStartGridAsync(string raceId)
    {
        var game = await Games
            .Include(game => game.StartList)
            .ThenInclude(entry => entry.Rider)
            .ThenInclude(rider => rider.Profiles)
            .Where(game => game.RaceId.Equals(raceId))
            .FirstOrDefaultAsync();
        return game;
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

            existingRace.Updated = DateTime.UtcNow;
            if (newStatus != null) existingRace.Status = newStatus.Value;
        }

        await SaveChangesAsync();
    }
}