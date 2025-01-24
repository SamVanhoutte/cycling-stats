using CyclingStats.DataAccess.Entities;
using CyclingStats.Logic.Exceptions;
using CyclingStats.Models;

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
    public async Task TestRetrieveRiderProfileAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var rider = await retriever.GetRiderProfileAsync(new Models.Rider{Id =  "sam-welsford"});
        Assert.NotNull( rider.Name);
        Assert.NotNull( rider.Team);
        Assert.Equal(1996, rider.BirthYear);
        Assert.Equal(180, rider.Height);
        Assert.Equal(79, rider.Weight);
    }
    
    [Fact]
    public async Task TestRetrieveRiderProfileWithoutRankingAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var rider = await retriever.GetRiderProfileAsync(new Models.Rider{Id =  "giacomo-garavaglia"});
        Assert.NotNull( rider.Name);
        Assert.NotNull( rider.Team);
        Assert.Equal(1996, rider.BirthYear);
        Assert.Equal(177, rider.Height);
        Assert.Equal(65, rider.Weight);
    }
    
    [Fact]
    public async Task TestRetrieveRiderProfileWithoutTeamAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var rider = await retriever.GetRiderProfileAsync(new Models.Rider{Id =  "alessandro-fedeli"});
        Assert.Equal(RiderStatus.Retired, rider.Status);
        Assert.NotNull( rider.Name);
        Assert.NotNull( rider.Team);
        Assert.Equal(1996, rider.BirthYear);
        Assert.Equal(176, rider.Height);
        Assert.Equal(65, rider.Weight);
    }

    [Fact]
    public async Task TestRetrieveOneDayRaceDetailsAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var raceData = await retriever.GetRaceDataAsync(GetRace("omloop-het-nieuwsblad/2024"));
        Assert.Single(raceData);
        Assert.True(raceData.Single().DetailsAvailable);
        Assert.True(raceData.Single().Distance > 20);
        Assert.False(raceData.Single().StageRace);
    }

    [Fact]
    public async Task TestRetrieveStageRaceDetailsAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var raceData = await retriever.GetRaceDataAsync(GetRace("tour-down-under/2025/stage-1"));

        Assert.True(raceData.Single().DetailsAvailable);
        Assert.True(raceData.Single().Distance > 20);
        Assert.True(raceData.Single().StageRace);
    }

    [Fact]
    public async Task TestGetStageRaceDataAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var results = await retriever.GetRaceDataAsync(GetRace("4-jours-de-dunkerque/2025"));

        Assert.NotNull(results);
    }

    [Fact]
    public async Task TestFutureRaceDataAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var races = await retriever.GetRaceDataAsync(GetRace("amstel-gold-race/2025"));

        Assert.NotNull(races);
        Assert.NotEmpty(races);
        Assert.NotNull(races.Single().Classification);
    }

    [Fact]
    public async Task TestFutureStageRaceDataAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var races = await retriever.GetRaceDataAsync(GetRace("4-jours-de-dunkerque/2025"));

        Assert.NotNull(races);
        Assert.NotEmpty(races);
        Assert.Equal(5, races.Count);
        Assert.False(races.First().Id.StartsWith("stage", StringComparison.InvariantCultureIgnoreCase));
    }



    [Fact]
    public async Task TestRaceIdDataAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();

        var pcsId = await retriever.GetPcsRaceIdAsync(new Race { Id = "alpes-isere-tour", Name = "Alpes Isère Tour" });

        Assert.NotNull(pcsId);
        Assert.NotEmpty(pcsId);
        Assert.Equal("rhone-alpes-isere-tour/2025", pcsId);
        Assert.False(pcsId.StartsWith("race", StringComparison.InvariantCultureIgnoreCase));
    }
    
    [Fact]
    public async Task TestRiderIdDataAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var pcsId = await retriever.GetPcsRiderIdAsync(new Models.Rider { Id = "jonas-hvideberg" });

        Assert.NotNull(pcsId);
        Assert.NotEmpty(pcsId);
        Assert.Equal("jonas-iversby-hvideberg", pcsId);
        Assert.False(pcsId.StartsWith("rider", StringComparison.InvariantCultureIgnoreCase));
    }

    
    [Fact]
    public async Task TestGetOneDayRaceResultsAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var results = await retriever.GetRaceResultsAsync(GetRace( "omloop-het-nieuwsblad/2024"));

        Assert.NotEmpty(results);
    }

    [Fact]
    public async Task TestGetOneDayRacePointsAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var results = await retriever.GetRacePointsAsync(GetRace("omloop-het-nieuwsblad/2024"));

        Assert.NotEmpty(results);
        Assert.True(results.First().Rider.Id.Length > 4);
        Assert.True(results.First().Points > 0);
        Assert.True(results.First().Stars > 0);
        Assert.True(results.First().Picked > 0);
    }
    
    [Fact]
    public async Task TestGetStageDayRacePointsAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var results = await retriever.GetRacePointsAsync(GetRace("tour-down-under/2025/stage-1", true));

        Assert.NotEmpty(results);
        Assert.True(results.First().Rider.Id.Length > 4);
        Assert.True(results.First().Points > 0);
        Assert.True(results.First().Stars > 0);
        Assert.True(results.First().Picked > 0);
    }
    
    [Fact]
    public async Task TestRaceCalendarAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var results = await retriever.GetRaceCalendarAsync(2024);

        Assert.NotEmpty(results);
        Assert.True(results.First().Name.Length > 4);
        Assert.True(results.First().Id.Length > 4);
    }
    
    [Fact]
    public async Task TestGetFutureStageDayRacePointsAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var task =  retriever.GetRacePointsAsync(GetRace("tour-down-under/2025/stage-4", true));
        var results = await Assert.ThrowsAsync<NoResultsAvailableException>(async () => await task);
    }

    private Race GetRace(string id, bool stageRace = false)
    {
        return new Race { Id = id, PcsId = id , IsStageRace = stageRace};
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