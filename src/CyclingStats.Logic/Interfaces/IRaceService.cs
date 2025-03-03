using CyclingStats.Models;

namespace CyclingStats.Logic.Interfaces;

public interface IRaceService
{
    Task<List<RaceDetails>> GetRaceDataAsync();

    Task<ICollection<RaceDetails>> GetRacesAsync(RaceStatus? status = null, RaceStatus? statusToExclude = null,
        bool? detailsCompleted = null,
        bool? pointsRetrieved = null, bool? resultsRetrieved = null);

    Task<RaceDetails?> GetRaceAsync(string raceId, bool includeResults = false, bool includePoints = false);
    Task<List<RaceDetails>> GetStageRaceStagesAsync(string raceId);
    Task UpsertRacePointsAsync(Models.RaceDetails raceDetails);
    Task UpdateRaceStatusAsync(string raceId, RaceStatus newStatus);
    Task MarkRaceAsErrorAsync(string raceId, string error);
    Task UpsertRaceDetailsAsync(RaceDetails raceData, bool stageRaceBatch, RaceStatus? newStatus = null);
    Task<StartGrid?> GetRaceStartGridAsync(string raceId);
    //Task UpsertRaceDetailsAsync(Models.RaceDetails raceData, RaceStatus? newStatus = null) //, string? error = null)
    Task UpsertRaceStartGridAsync(string raceId, StartGrid startGrid);
    Task UpsertRaceResultsAsync(Models.RaceDetails raceDetails);
    Task<ICollection<RaceDetails>> FilloutResultsAsync(ICollection<RaceDetails> races);
}

