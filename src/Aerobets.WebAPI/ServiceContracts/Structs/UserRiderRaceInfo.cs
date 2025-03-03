using CyclingStats.Models;
using CyclingStats.WebApi.Models;

namespace Aerobets.WebAPI.ServiceContracts.Structs;

public class UserRiderRaceInfo : RiderRaceInfo
{
    public RiderBookmarkSummary? Bookmark { get; set; }
    public IDictionary<string, int>? PastRaceResults { get; set; }
    public int LastYearResult { get; set; }
    public double RecommendationScore { get; set; }
    public RecommendationScores RecommendationScores { get; set; }

    public static UserRiderRaceInfo FromDomain(StartingRider gridEntry, RiderBookmark? bookmark, int lastYearResult,
        IDictionary<string, int> pastRaceResults, RecommendationScores recommendedScores)
    {
        var riderRaceInfo = RiderRaceInfo.FromDomain(gridEntry);
        var userRiderRaceInfo = new UserRiderRaceInfo
        {
            Rider = riderRaceInfo.Rider,
            Profile = riderRaceInfo.Profile,
            YouthRider = riderRaceInfo.YouthRider,
            RiderType = riderRaceInfo.RiderType,
            Stars = riderRaceInfo.Stars,
            Bookmark = RiderBookmarkSummary.FromDomain(bookmark),
            PastRaceResults = pastRaceResults,
            LastYearResult = lastYearResult,
            RecommendationScore = recommendedScores.Overall,
            RecommendationScores = recommendedScores
        };

        return userRiderRaceInfo;
    }

}