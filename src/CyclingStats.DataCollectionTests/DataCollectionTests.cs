using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using Microsoft.Extensions.Options;
using WorldcyclingStats;

namespace CyclingStats.DataCollectionTests;

public class DataCollectionTests
{
    
    [Fact(Skip = "Not implemented yet")]
    public async Task TestRetrieveTeamsAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var teams = await retriever.GetTeamsAsync();
        Assert.True(teams.Count() > 2);
    }
    
    [Fact(Skip = "Long running test")]
    public async Task TestRetrieveTeamAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var riders = await retriever.GetTeamRidersAsync("ineos-grenadiers");
        Assert.True(riders.Count() > 2);
    }
    
    [Fact]
    public async Task TestRetrieveOneDayRaceDetailsAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var raceData = await retriever.GetRaceDataAsync("omloop-het-nieuwsblad", 2024);
        Assert.Single(raceData);
        Assert.True(raceData.Single().DetailsAvailable);
        Assert.True(raceData.Single().Distance > 20);
        Assert.False(raceData.Single().StageRace);
    }
    
    [Fact]
    public async Task TestRetrieveStageRaceDetailsAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var raceData = await retriever.GetRaceDataAsync("tour-down-under", 2025, "stage-1");
        
        Assert.True(raceData.Single().DetailsAvailable);
        Assert.True(raceData.Single().Distance > 20);
        Assert.True(raceData.Single().StageRace);
    }
    
    [Fact]
    public async Task TestGetRaceResultsAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var results = await retriever.GetRaceResultsAsync("tour-down-under" , 2024, "stage-1");

        Assert.NotEmpty(results);
    }
    
    [Fact]
    public async Task TestGetStageRaceDataAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var results = await retriever.GetRaceDataAsync("4-jours-de-dunkerque" , 2025);

        Assert.NotNull(results);
    }
    
    [Fact]
    public async Task TestFutureRaceDataAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var races = await retriever.GetRaceDataAsync("amstel-gold-race" , 2025);

        Assert.NotNull(races);
        Assert.NotEmpty(races);
        Assert.NotNull(races.Single().Classification);
    }
    
    [Fact]
    public async Task TestFutureStageRaceDataAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var races = await retriever.GetRaceDataAsync("4-jours-de-dunkerque" , 2025);

        Assert.NotNull(races);
        Assert.NotEmpty(races);
        Assert.Equal(5, races.Count);
    }
    
    
    
    [Fact]
    public async Task TestGetOneDayRaceResultsAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var results = await retriever.GetRaceResultsAsync("omloop-het-nieuwsblad" , 2024);

        Assert.NotEmpty(results);
    }
    
    [Fact]
    public async Task TestGetOneDayRacePointsAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var results = await retriever.GetRacePointsAsync("omloop-het-nieuwsblad" , 2024);

        Assert.NotEmpty(results);
        Assert.True(results.First().Rider.Id.Length > 4);
    }
    
    // [Fact]
    // public async Task TestRetrieveOneDayRaceDetailsAsync()
    // {
    //     var retriever = await TestClientFactory.GetDataRetrieverAsync();
    //     var raceData = await retriever.GetRaceDataAsync("omloop-het-nieuwsblad", 2024);
    //
    //     Assert.True(raceData.DetailsAvailable);
    //     Assert.True(raceData.Distance > 20);
    //     Assert.NotEmpty(raceData.ProfileImageUrl);
    //     Assert.True(raceData.ProfileScore > 0);
    //     Assert.NotEmpty(raceData.UciScale);
    //     Assert.Equal(2, raceData.ParcoursType);
    //     Assert.True(raceData.RaceRanking > 0);
    //     Assert.False(raceData.ProfileImageUrl.Contains("logo"));
    //     Assert.True(raceData.ProfileImageUrl.Contains("profiles"));
    //     Assert.False(raceData.StageRace);
    // }
    //
    // [Fact]
    // public async Task TestRetrieveStageRaceDetailsAsync()
    // {
    //     var retriever = await TestClientFactory.GetDataRetrieverAsync();
    //     var raceData = await retriever.GetRaceDataAsync("tour-down-under", 2025, "stage-1");
    //
    //     Assert.True(raceData.DetailsAvailable);
    //     Assert.True(raceData.Distance > 20);
    //     Assert.NotEmpty(raceData.ProfileImageUrl);
    //     Assert.False(raceData.ProfileImageUrl.Contains("logo"));
    //     Assert.True(raceData.ProfileImageUrl.Contains("profiles"));
    //     Assert.True(raceData.StageRace);
    // }
    //
    // [Fact]
    // public async Task TestGetRaceResultsAsync()
    // {
    //     var retriever = await TestClientFactory.GetDataRetrieverAsync();
    //     var results = await retriever.GetRaceResultsAsync("tour-down-under" , 2024, "stage-1");
    //
    // }
}