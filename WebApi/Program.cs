using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Utility;
using Hangfire;
using Hangfire.Storage;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Seeders;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Backend.WebApi;

public abstract class Program
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

                Log.Logger.Information("Application booted");
                try
                {
                    var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

                    Log.Logger.Information("Running Entity Framework Database Migrations...");
                    await SqlDatabaseSeedAsync.MigrateDatabase(context);

                    Log.Logger.Information("Seeding roles and claims...");
                    await SeedRoleClaims(services, context);

                    Log.Logger.Information("Seeding users (if needed)...");
                    await SeedDefaultUsers(services, context);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "An error occurred while migrating or seeding the database");
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
                Log.Logger.Fatal(ex, "Fatal error occurred");
            }
            else
            {
                Log.Logger.Fatal(ex, "Error occurred. Retrying...");
                await Main(args);
            }
        }
    }

    private static async Task SeedRoleClaims(IServiceProvider services, IApplicationDbContext context)
    {
        var roleService = services.GetRequiredService<IRoleService>();
        var claimSeeder = new RoleClaimSeeder(context, roleService);
        await claimSeeder.SeedAllRoleClaims();
    }

    private static async Task SeedDefaultUsers(IServiceProvider services, IApplicationDbContext context)
    {
        var usersExist = await context.Users.AnyAsync();
        if (!usersExist)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var userSeeder = new DefaultUserSeeder(context, userManager);
            await userSeeder.Seed();
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