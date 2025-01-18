using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Logic.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using WorldcyclingStats;

namespace CyclingStats.DataCollectionTests;

public class TestClientFactory
{
    public static IConfigurationRoot Configuration
    {
        get
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddUserSecrets<DataCollectionTests>();
            var configuration = configBuilder.Build();
            return configuration;
        }
    }

    public static async Task<IDataRetriever> GetDataRetrieverAsync()
    {
        var wcsConfiguration = new WcsOptions();
        var pcsConfiguration = new PcsOptions();
        Configuration.GetSection("wcs").Bind(wcsConfiguration);
        Configuration.GetSection("pcs").Bind(pcsConfiguration);
        return new StatsCollector(new OptionsWrapper<WcsOptions>(wcsConfiguration), new OptionsWrapper<PcsOptions>(pcsConfiguration));
    }
    
    
}