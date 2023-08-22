using Application.Common.Interfaces;
using Domain.Utility;
using Hangfire;
using Hangfire.Storage;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Serilog;

namespace WebApi;

public class Program
{
    private const string HumanReadbleFlag = "--human-logs";
    private static readonly ArgumentParser ArgumentParser = new();
    private static int _startupFailures;

    public static async Task Main(string[] args)
    {
        var isSeedCommand = ArgumentParser.FlagExists("--seed", args);
        var useHumanReadableLogs = ArgumentParser.FlagExists(HumanReadbleFlag, args);
        var loggingConfig = LoggingConfigurator.CreateLoggerConfiguration().AddAllRequiredSinks(useHumanReadableLogs);
        LoggingConfigurator.BootstrapLogger(loggingConfig);
        try
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                //resolve services for hangfire up from before processing begins:

                Log.Logger.Information("Application booted");
                try
                {
                    var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

                    Log.Logger.Information("Running Entity Framework Database Migrations...");
                    await SqlDatabaseSeedAsync.MigrateDatabase(context);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "An error occurred while migrating or seeding the database.");
                    throw;
                }
            }


            if (!isSeedCommand)
            {
                Log.Logger.Information("Starting Web Api");
                JobStorage.Current.GetConnection().GetRecurringJobs();
                using (new BackgroundJobServer())
                {
                    await host.RunAsync();
                }
            }
        }
        catch (Exception ex)
        {
            _startupFailures++;
            if (_startupFailures > 3)
            {
                Log.Logger.Fatal(ex, "Fatal error occurred.");
            }
            else
            {
                Log.Logger.Fatal(ex, "Error occurred. Retrying...");
                await Main(args);
            }
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        var useHumanReadableLogs = ArgumentParser.FlagExists(HumanReadbleFlag, args);
        return Host.CreateDefaultBuilder(args)
            .UseSerilog(
                (context, services, configuration) => configuration.AddAllRequiredSinks(useHumanReadableLogs), true)
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}

// var builder = WebApplication.CreateBuilder(args);
//
// // Add services to the container.
//
// builder.Services.AddControllers();
// // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
// builder.Services
//     .AddApplication()
//     .AddInfrastructure(builder.Configuration)
//     .AddPresentation();
//
//
// builder.Host.UseSerilog((context, configuration) =>
//     configuration.ReadFrom.Configuration(context.Configuration));
//
// var app = builder.Build();
//
// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }
//
//
// app.UseHttpsRedirection();
//
// app.UseAuthorization();
//
// app.MapControllers();
//
// app.Run();