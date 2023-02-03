using CyclingStats.Logic;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Logic.Services;
using CyclingStats.Models.Configuration;
using CyclingStats.Workers.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<SqlOptions>(options =>
            hostContext.Configuration.GetSection("sql").Bind(options));
        services.Configure<WcsOptions>(options =>
            hostContext.Configuration.GetSection("wcs").Bind(options));
        services.Configure<ScheduleOptions>(options =>
            hostContext.Configuration.GetSection("schedule").Bind(options));
        services.AddSingleton<IDataRetriever, StatsCollector>();
        services.AddHostedService<RaceResultWorker>();
        services.AddHostedService<RaceStartListWorker>();
        services.AddHostedService<RaceDataWorker>();
    })
    .Build();

await host.RunAsync();