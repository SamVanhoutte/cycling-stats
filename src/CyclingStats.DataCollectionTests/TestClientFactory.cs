using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ProcyclingStats;
using WorldcyclingStats;

namespace CyclingStats.DataCollectionTests;

public class TestClientFactory
{
    public static IConfigurationRoot Configuration
    {
        get
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddUserSecrets<WcsDataCollectionTests>();
            var configuration = configBuilder.Build();
            return configuration;
        }
    }

    public static async Task<IDataRetriever> GetWcsRetrieverAsync()
    {
        var wcsConfiguration = new WcsOptions();
        Configuration.GetSection("wcs").Bind(wcsConfiguration);
        return new WcsStatsCollector(new OptionsWrapper<WcsOptions>(wcsConfiguration));
    }
    
    public static async Task<IDataRetriever> GetPcsRetrieverAsync()
    {
        var pcsConfiguration = new PcsOptions();
        Configuration.GetSection("pcs").Bind(pcsConfiguration);
        return new PcsStatsCollector(new OptionsWrapper<PcsOptions>(pcsConfiguration));
    }
}