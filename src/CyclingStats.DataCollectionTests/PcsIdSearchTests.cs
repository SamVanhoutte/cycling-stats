using CyclingStats.Models;

namespace CyclingStats.DataCollectionTests;

public class PcsIdSearchTests
{
    
    
    [Fact]
    public async Task TestRaceIdDataAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();

        var pcsId = await retriever.GetPcsRaceIdAsync(new RaceDetails { Id = "alpes-isere-tour", Name = "Alpes Isère Tour" });

        Assert.NotNull(pcsId);
        Assert.NotEmpty(pcsId);
        Assert.Equal("rhone-alpes-isere-tour/2025", pcsId);
        Assert.False(pcsId.StartsWith("race", StringComparison.InvariantCultureIgnoreCase));
    }
    
    [Fact]
    public async Task TestRaceIdPastYearDataAsync()
    {
        var retriever = await TestClientFactory.GetDataRetrieverAsync();

        var pcsId = await retriever.GetPcsRaceIdAsync(new RaceDetails { Id = "amstel-gold-race-ladies-edition/2024", Name = "Amstel Gold Race Ladies Edition" });

        Assert.NotNull(pcsId);
        Assert.NotEmpty(pcsId);
        Assert.Equal("amstel-gold-race-we/2024", pcsId);
        Assert.False(pcsId.StartsWith("race", StringComparison.InvariantCultureIgnoreCase));
    }
}