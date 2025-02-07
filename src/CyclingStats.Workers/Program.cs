using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Logic.Services;
using CyclingStats.Workers.Actions;
using CyclingStats.Workers.Workers;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<SqlOptions>(options =>
            hostContext.Configuration.GetSection("sql").Bind(options));
        services.Configure<WcsOptions>(options =>
            hostContext.Configuration.GetSection("wcs").Bind(options));
        services.Configure<PcsOptions>(options =>
            hostContext.Configuration.GetSection("pcs").Bind(options));
        services.Configure<ScheduleOptions>(options =>
            hostContext.Configuration.GetSection("schedule").Bind(options));
        services.AddSingleton<IDataRetriever, StatsCollector>();
        services.AddSingleton<IRaceService, RaceService>();
        services.AddHostedService<RaceResultWorker>();
        services.AddHostedService<RaceStartListWorker>();
        services.AddHostedService<RaceDataWorker>();
        services.AddHostedService<CalendarImportJob>();
        services.AddHostedService<RaceIdMatchWorker>();
        services.AddHostedService<RacePointsWorker>();
        services.AddHostedService<RiderProfileWorker>();
        
    })
    .Build();

await host.RunAsync();