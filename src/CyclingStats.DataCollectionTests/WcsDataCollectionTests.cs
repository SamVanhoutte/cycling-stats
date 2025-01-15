using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using Microsoft.Extensions.Options;
using WorldcyclingStats;

namespace CyclingStats.DataCollectionTests;

public class WcsDataCollectionTests
{
    
    [Fact(Skip = "Not implemented yet")]
    public async Task TestRetrieveTeamsAsync()
    {
        var retriever = await TestClientFactory.GetWcsRetrieverAsync();
        var teams = await retriever.GetTeamsAsync();
        Assert.True(teams.Count() > 2);
    }
    
    [Fact(Skip = "Long running test")]
    public async Task TestRetrieveTeamAsync()
    {
        var retriever = await TestClientFactory.GetWcsRetrieverAsync();
        var riders = await retriever.GetTeamRidersAsync("ineos-grenadiers");
        Assert.True(riders.Count() > 2);
    }
    
    [Fact]
    public async Task TestRetrieveOneDayRaceDetailsAsync()
    {
        var retriever = await TestClientFactory.GetWcsRetrieverAsync();
        var raceData = await retriever.GetRaceDataAsync("omloop-het-nieuwsblad", 2024);
        
        Assert.True(raceData.DetailsAvailable);
        Assert.True(raceData.Distance > 20);
        Assert.False(raceData.StageRace);
    }
    
    [Fact]
    public async Task TestRetrieveStageRaceDetailsAsync()
    {
        var retriever = await TestClientFactory.GetWcsRetrieverAsync();
        var raceData = await retriever.GetRaceDataAsync("tour-down-under", 2025);
        
        Assert.True(raceData.DetailsAvailable);
        Assert.True(raceData.Distance > 20);
        Assert.True(raceData.StageRace);
    }
}