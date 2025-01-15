using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using Microsoft.Extensions.Options;
using WorldcyclingStats;

namespace CyclingStats.DataCollectionTests;

public class WcsDataCollectionTests
{
    
    [Fact]
    public async Task TestRetrieveTeamsAsync()
    {
        var retriever = await TestClientFactory.GetWcsRetrieverAsync();
        var teams = await retriever.GetTeamsAsync();
        Assert.True(teams.Count() > 2);
    }
    
    [Fact]
    public async Task TestRetrieveTeamAsync()
    {
        var retriever = await TestClientFactory.GetWcsRetrieverAsync();
        var riders = await retriever.GetTeamRidersAsync("ineos-grenadiers");
        Assert.True(riders.Count() > 2);
    }
}