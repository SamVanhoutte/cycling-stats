using System.Reflection;
using Aerozure;
using Aerozure.Configuration;
using Aerozure.Encryption;
using Aerozure.Runtime;
using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Logic.Services;
using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using NSwag.Generation.AspNetCore;

namespace Aerobets.WebAPI;

public class Program
{
    public static void Main(String[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // builder.Services.AddApplicationInsightsTelemetry();

        builder.Services.AddControllers(options =>
        {
            options.ReturnHttpNotAcceptable = false;
            options.RespectBrowserAcceptHeader = false;

            //RestrictToJsonContentType(options);
            //ConfigureJsonFormatters(options);
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApiDocument(document => { GenerateOpenApiSpec(document, "v1"); });
        builder.Services.AddTransient<IUserService, UserService>();
        builder.Services.AddTransient<IRaceService, RaceService>();
        builder.Services.AddTransient<IGameService, GameService>();
        builder.Services.AddTransient<IDataRetriever, StatsCollector>();
        builder.Services.AddHealthChecks();


        var cfgBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory());

        if (RuntimeExecution.IsLocal)
        {
        }


        // Configure globally:
        FlurlHttp.Clients.WithDefaults(builder =>
            builder.BeforeCall(call =>
            {
                if (call.RequestBody != null)
                {
                    var requestBody = call.RequestBody.ToString();
                    Console.WriteLine("Request Payload: " + requestBody);
                }
            })
        );
        cfgBuilder.AddJsonFile($"appsettings.json", true, true)
            .AddJsonFile($"appsettings.dev.json", true, true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>();


        var configuration = cfgBuilder.Build();

        builder.Services
            .AddAerozure()
            // .AddSingleton<IErrorSerializerSettingsFactory, ExceptionSerializer>()
            .Configure<SqlOptions>(options =>
                configuration.GetSection("sql").Bind(options))
            .Configure<WcsOptions>(options =>
                configuration.GetSection("wcs").Bind(options))
            .Configure<PcsOptions>(options =>
                configuration.GetSection("pcs").Bind(options))
            .Configure<EncryptionSettings>(options =>
                configuration.GetSection("encryption").Bind(options))
            .Configure<AzuremlOptions>(options => configuration.GetSection("azureml").Bind(options))
            ;
        var app = builder.Build();
        app.UseOpenApi();
        app.UseSwaggerUi();

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();


        void GenerateOpenApiSpec(AspNetCoreOpenApiDocumentGeneratorSettings nswagSettings, string documentName)
        {
            string apiTitle = "Cotacol Backend API";

            bool includeAllOperations = false;
            if (builder.Environment.IsDevelopment() && builder.Configuration["IS_NSWAG_BUILD"] != "true")
            {
                // nswagSettings.OperationProcessors.Add(new AddHeaderParameter(CotacolHeaders.ApiSecretHeader,
                //     "The secret key of the API subscription.", null,
                //     isRequired: true));
                // nswagSettings.OperationProcessors.Add(new AddHeaderParameter(CotacolHeaders.AdminSecretHeader,
                //     "The secret key for the admin of the API.", null,
                //     isRequired: false));
                includeAllOperations = true;
            }

            if (!string.IsNullOrEmpty(builder.Configuration["API_TITLE"]))
            {
                apiTitle = builder.Configuration["API_TITLE"];
                apiTitle = apiTitle.Replace("_", " ");
            }

            string? apiType = null;

            if (!string.IsNullOrEmpty(builder.Configuration["API_TYPE"]))
            {
                apiType = builder.Configuration["API_TYPE"];
            }

            if (!string.IsNullOrEmpty(builder.Configuration["INCLUDE_ALL_OPERATIONS"]))
            {
                _ = bool.TryParse(builder.Configuration["INCLUDE_ALL_OPERATIONS"], out includeAllOperations);
            }

            //nswagSettings.OperationProcessors.Add(new OpenApiSpecOperationProcessor(includeAllOperations, apiType));
            nswagSettings.DocumentName = documentName;
            nswagSettings.Title = apiTitle;
            nswagSettings.Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        }

        void RestrictToJsonContentType(MvcOptions options)
        {
            var allButJsonInputFormatters =
                options.InputFormatters.Where(formatter => formatter is not SystemTextJsonInputFormatter);

            foreach (IInputFormatter inputFormatter in allButJsonInputFormatters)
            {
                options.InputFormatters.Remove(inputFormatter);
            }

            // Removing for text/plain, see https://docs.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-3.0#special-case-formatters
            options.OutputFormatters.RemoveType<StringOutputFormatter>();
        }

        //
        // Logger CreateLogConfiguration(string instrumentationKey)
        // {
        //     var logger = new LoggerConfiguration()
        //         .MinimumLevel.Debug()
        //         // .MinimumLevel.Override("Host.Results", LogEventLevel.Warning)
        //         // .MinimumLevel.Override("Azure.Messaging.ServiceBus", LogEventLevel.Error)
        //         // .MinimumLevel.Override("Azure.Core", LogEventLevel.Error)
        //         // .MinimumLevel.Override("Function", LogEventLevel.Warning)
        //         // .MinimumLevel.Override("Host.Aggregator", LogEventLevel.Warning)
        //         // .MinimumLevel.Override("Function.Thread", LogEventLevel.Warning)
        //         // .MinimumLevel.Override("DurableTask.AzureStorage", LogEventLevel.Warning)
        //         // .MinimumLevel.Override("DurableTask.Core", LogEventLevel.Warning)
        //         // .MinimumLevel.Override("Host.Triggers.DurableTask", LogEventLevel.Warning)
        //         // .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        //         // .MinimumLevel.Override("Worker", LogEventLevel.Warning)
        //         // .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        //         // .MinimumLevel.Override("DurableTask", LogEventLevel.Warning)
        //         // .MinimumLevel.Override("Host", LogEventLevel.Warning)
        //         // .MinimumLevel.Override("Function", LogEventLevel.Warning)
        //         .Enrich.FromLogContext()
        //         .Enrich.WithVersion()
        //         .WriteTo.Console()
        //         .WriteTo.AzureApplicationInsightsWithInstrumentationKey(instrumentationKey, LogEventLevel.Verbose)
        //         .CreateLogger();
        //
        //     return logger;
        // }

    }
    
}