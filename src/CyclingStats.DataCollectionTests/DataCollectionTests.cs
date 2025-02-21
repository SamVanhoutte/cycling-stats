using System.Runtime.InteropServices;
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
        var rider = await retriever.GetRiderProfileAsync(new Models.Rider { Id = "sam-welsford" });
        Assert.NotNull(rider.Name);
        Assert.NotNull(rider.Team);
        Assert.Equal(1996, rider.BirthYear);
        Assert.Equal(180, rider.Height);
        Assert.Equal(79, rider.Weight);
    }

    [Fact]
    public async Task TestRetrieveRiderProfileWithoutRankingAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var rider = await retriever.GetRiderProfileAsync(new Models.Rider { Id = "giacomo-garavaglia" });
        Assert.NotNull(rider.Name);
        Assert.NotNull(rider.Team);
        Assert.Equal(1996, rider.BirthYear);
        Assert.Equal(177, rider.Height);
        Assert.Equal(65, rider.Weight);
    }

    [Fact]
    public async Task TestRetrieveRiderProfileWithoutTeamAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var rider = await retriever.GetRiderProfileAsync(new Models.Rider { Id = "alessandro-fedeli" });
        Assert.Equal(RiderStatus.Retired, rider.Status);
        Assert.NotNull(rider.Name);
        Assert.NotNull(rider.Team);
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
        Assert.Equal(2024, raceData.Single().Date.Value.Year);
        Assert.False(raceData.Single().StageRace);
    }


    [Fact]
    public async Task TestRetrieveOneDayRaceDateAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var raceData = await retriever.GetRaceDataAsync(GetRace("omloop-het-nieuwsblad/2024"));
        Assert.Single(raceData);
        Assert.True(raceData.Single().DetailsAvailable);
        Assert.True(raceData.Single().Distance > 20);
        Assert.Equal(2024, raceData.Single().Date.Value.Year);
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
        var data = await retriever.GetRaceDataAsync(GetRace("4-jours-de-dunkerque/2025", true));

        Assert.NotNull(data);
        Assert.Equal(6, data.Count);
        Assert.NotNull(data.Last().Classification);
        Assert.NotNull(data.First().Distance);
    }

    [Theory]
    [InlineData("liege-la-gleize/2024/stage-2a", null)]
    [InlineData("grote-prijs-jean-pierre-monsere/2025", null)]
    [InlineData("tour-de-france/2024/stage-21", 2724)]
    [InlineData("tour-cycliste-international-de-la-guadeloupe/2024/stage-8", 12752)]
    [InlineData("gp-eccellenze-valli-del-soligo/2024", null)]
    [InlineData("tre-valli-varesine/2024", null, RaceStatus.Canceled)]
    public async Task TestGetStageRaceDurationDataAsync(string raceId, int? expectedDuration,
        RaceStatus? expectedStatus = null)
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var data = await retriever.GetRaceDataAsync(GetRace(raceId));

        Assert.NotNull(data);
        Assert.Equal(expectedDuration, data.Single().Duration);
        if (expectedStatus != null)
        {
            Assert.Equal(expectedStatus, data.Single().Status);
        }
    }

    [Fact]
    public async Task TestGetStageRaceMainDataAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var data = await retriever.GetRaceDataAsync(GetRace("tour-de-serbie/2024", true));
        Assert.NotNull(data);
        Assert.Equal(4, data.Count);
        Assert.NotNull(data.Last().Classification);
        Assert.NotNull(data.First().Distance);
    }

    [Fact]
    public async Task TestGetPastStageRaceDataAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var data = await retriever.GetRaceDataAsync(GetRace("4-jours-de-dunkerque/2023"));

        Assert.NotNull(data);
        Assert.NotEmpty(data);
        Assert.NotEmpty(data.First().Name);
        Assert.Equal(2023, data.First().Date.Value.Year);
    }

    [Fact]
    public async Task TestFutureRaceDataAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var races = await retriever.GetRaceDataAsync(GetRace("amstel-gold-race/2025"));

        Assert.NotNull(races);
        Assert.NotEmpty(races);
        Assert.NotNull(races.Single().Classification);
        Assert.Equal(2025, races.Single().Date.Value.Year);
    }

    [Fact]
    public async Task TestFutureStageRaceDataAsync()
    {
        var stageRaceId = "4-jours-de-dunkerque/2025";
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var races = await retriever.GetRaceDataAsync(GetRace(stageRaceId));

        Assert.NotNull(races);
        Assert.NotEmpty(races);
        Assert.Equal(5, races.Count);
        Assert.False(races.First().Id.StartsWith("stage", StringComparison.InvariantCultureIgnoreCase));
        Assert.Equal("stage-1", races.First().StageId);
        Assert.Equal(stageRaceId, races.First().StageRaceId);
    }


    [Fact]
    public async Task TestStageRaceNameDataAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var race = GetRace("4-jours-de-dunkerque/2024/stage-2", true);
        race.Name = Guid.NewGuid().ToString();
        var races = await retriever.GetRaceDataAsync(race);

        Assert.NotNull(races);
        Assert.NotEmpty(races);
        Assert.Equal(race.Name, races.Single()!.Name);
        Assert.NotNull(races.Single()!.UciScale);
        Assert.Equal("stage-2", races.Single()!.StageId);
        Assert.Equal("4-jours-de-dunkerque/2024", races.Single()!.StageRaceId);
    }

    [Fact]
    public async Task TestStageRaceCompletedDataAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var race = GetRace("tirreno-adriatico/2024/stage-5", true);
        race.Name = Guid.NewGuid().ToString();
        var races = await retriever.GetRaceDataAsync(race);

        Assert.NotNull(races);
        Assert.NotEmpty(races);
        Assert.Equal(race.Name, races.Single()!.Name);
        Assert.NotNull(races.Single()!.UciScale);
        Assert.Equal("stage-5", races.Single()!.StageId);
        Assert.Equal("tirreno-adriatico/2024", races.Single()!.StageRaceId);
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
        var results = await retriever.GetRaceResultsAsync(GetRace("omloop-het-nieuwsblad/2024"));

        Assert.NotEmpty(results.Item2);
    }

    [Fact]
    public async Task TestGetStageRaceResultsAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var results = await retriever.GetRaceResultsAsync(GetRace("tour-down-under/2025/stage-5", true));

        Assert.NotEmpty(results.Item2);
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
    public async Task TestStartgridAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();
        var results = await retriever.GetStartListAsync("tour-of-oman/2025");
        Assert.NotNull(results);
        Assert.DoesNotContain(results.Riders, r => r.Item2 == 0);
    }

    private RaceDetails GetRace(string id, bool stageRace = false)
    {
        return new RaceDetails { Id = id, PcsId = id, RaceType = stageRace ? "Multi-day race" : "One-day race" };
    }
}