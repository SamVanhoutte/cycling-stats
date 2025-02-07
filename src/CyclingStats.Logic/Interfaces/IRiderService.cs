namespace CyclingStats.Logic.Interfaces;

public interface IRiderService
{
    Task<List<string>> GetRidersFromResultsAsync();
    Task UpsertRiderProfilesAsync(IEnumerable<Models.Rider> riders);
    Task<ICollection<Models.Rider>> GetRidersAsync(bool? detailsCompleted = null,
        string? monthToComplete = null);

}