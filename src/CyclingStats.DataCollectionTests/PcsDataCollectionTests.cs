using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using Microsoft.Extensions.Options;
using WorldcyclingStats;

namespace CyclingStats.DataCollectionTests;

public class PcsDataCollectionTests
{
    [Fact]
    public async Task TestRetrieveOneDayRaceDetailsAsync()
    {
        var retriever = await TestClientFactory.GetPcsRetrieverAsync();
        var raceData = await retriever.GetRaceDataAsync("omloop-het-nieuwsblad", 2024);

        Assert.True(raceData.DetailsAvailable);
        Assert.True(raceData.Distance > 20);
        Assert.NotEmpty(raceData.ProfileImageUrl);
        Assert.True(raceData.ProfileScore > 0);
        Assert.NotEmpty(raceData.UciScale);
        Assert.Equal(2, raceData.ParcoursType);
        Assert.True(raceData.RaceRanking > 0);
        Assert.False(raceData.ProfileImageUrl.Contains("logo"));
        Assert.True(raceData.ProfileImageUrl.Contains("profiles"));
        Assert.False(raceData.StageRace);
    }

    [Fact]
    public async Task TestRetrieveStageRaceDetailsAsync()
    {
        var retriever = await TestClientFactory.GetPcsRetrieverAsync();
        var raceData = await retriever.GetRaceDataAsync("tour-down-under", 2025, "stage-1");

        Assert.True(raceData.DetailsAvailable);
        Assert.True(raceData.Distance > 20);
        Assert.NotEmpty(raceData.ProfileImageUrl);
        Assert.False(raceData.ProfileImageUrl.Contains("logo"));
        Assert.True(raceData.ProfileImageUrl.Contains("profiles"));
        Assert.True(raceData.StageRace);
    }
}